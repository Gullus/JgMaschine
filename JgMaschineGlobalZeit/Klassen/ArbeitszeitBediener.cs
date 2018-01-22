using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using JgMaschineData;
using JgZeitHelper;

namespace JgMaschineGlobalZeit
{
    public class ArbeitszeitBediener
    {
        private JgModelContainer _Db;
        private ArbeitszeitRunden _AzRunden;

        public tabBediener Bediener { get; set; }   // Für Auswahl in Reporten
        public Guid IdBediener { get { return Bediener.Id; } }

        public ArbeitszeitHelperVorjahr AuswertungVorjahr { get; set; } = new ArbeitszeitHelperVorjahr();
        public ArbeitszeitHelperAktuell AuswertungKumulativ { get; set; } = new ArbeitszeitHelperAktuell();
        public ArbeitszeitHelperMonat AuswertungMonat { get; set; } = new ArbeitszeitHelperMonat();
        public ArbeitszeitHelperAktuell AuswertungGesamt { get; set; } = new ArbeitszeitHelperAktuell();

        public bool IstReadOnly { get { return Bediener?.EArbeitszeitHelper?.Status == EnumStatusArbeitszeitAuswertung.InArbeit; } }

        public ObservableCollection<tabArbeitszeitTag> ListeTage { get; set; } = new ObservableCollection<tabArbeitszeitTag>();

        public ArbeitszeitBediener()
        { }

        public ArbeitszeitBediener(JgModelContainer Db, ArbeitszeitRunden AzRunden)
        {
            Init(Db, AzRunden);
        }

        public void Init(JgModelContainer Db, ArbeitszeitRunden AzRunden)
        {
            _Db = Db;
            _AzRunden = AzRunden;
        }

        public void BedienerBerechnen(tabBediener Bediener, short Jahr, byte Monat, TimeSpan SollStundenMonat,
          IEnumerable<tabFeiertage> ListeFeiertageMonat, IEnumerable<tabPausenzeit> ListePausen)
        {
            if (Bediener == null)
                return;

            this.Bediener = Bediener;
            var alleAuswertungenBediener = _Db.tabArbeitszeitAuswertungSet.Where(w => (w.fBediener == Bediener.Id) && (w.Jahr == Jahr) && (w.Monat <= Monat)).ToList();

            var auswErster = alleAuswertungenBediener.FirstOrDefault(w => w.Monat == 0);
            if (auswErster == null)
                auswErster = ArbeitszeitAuswertungErstellen(Bediener, Jahr, 0, TimeSpan.Zero);

            AuswertungVorjahr.AzAuswertung = auswErster;

            var auswMonat = alleAuswertungenBediener.FirstOrDefault(w => (w.Monat == Monat));
            if (auswMonat == null)
                auswMonat = ArbeitszeitAuswertungErstellen(Bediener, Jahr, Monat, SollStundenMonat);

            AuswertungMonat.AzAuswertung = auswMonat;
            Bediener.EArbeitszeitHelper = auswMonat;

            var auswKumulativ = alleAuswertungenBediener.Where(w => (w.Monat > 0) && (w.Monat < Monat)).ToList();

            var sumUeberstunden = new TimeSpan(auswKumulativ.Sum(s => JgZeit.StringInZeit(s.Ueberstunden).Ticks));
            AuswertungKumulativ.UeberstundenAnzeige = JgZeit.ZeitInString(sumUeberstunden);
            var sumUeberstBezahlt = new TimeSpan(auswKumulativ.Sum(s => JgZeit.StringInZeit(s.AuszahlungUeberstunden).Ticks));
            AuswertungKumulativ.UeberstundenBezahltAnzeige = JgZeit.ZeitInString(sumUeberstBezahlt);
            AuswertungKumulativ.KrankAnzeige = (byte)auswKumulativ.Sum(s => s.Krank);
            AuswertungKumulativ.UrlaubAnzeige = (byte)auswKumulativ.Sum(s => s.Urlaub);

            var berechneteWerteInDb = AuswertungMonat.AzAuswertung.Status == EnumStatusArbeitszeitAuswertung.InArbeit;

            ListeFuerJedenTagErstellen(Bediener.EArbeitszeitHelper, ListeFeiertageMonat, ListePausen, berechneteWerteInDb);

            BerechneKrank();
            BerechneUrlaub();
            BerechneFeiertage();
            BerechneFeiertagZuschlag();

            BerechneNachtschichtZuschlag();
            BerechneUeberstundenBezahlt();

            BerechneUeberstundenAusTagen();
            BerechneUeberstundenGesamt();

            if (berechneteWerteInDb)
                DatenMonatInDatenbank();

            return;
        }

        private tabArbeitszeitAuswertung ArbeitszeitAuswertungErstellen(tabBediener Bediener, short Jahr, byte Monat, TimeSpan SollStundenMonat)
        {
            var az = new tabArbeitszeitAuswertung()
            {
                Id = Guid.NewGuid(),
                fBediener = Bediener.Id,
                Jahr = Jahr,
                Monat = Monat,
                Urlaub = 0,
                AuszahlungUeberstunden = "00:00",
                SollStunden = JgZeit.ZeitInString(SollStundenMonat),
                Status = EnumStatusArbeitszeitAuswertung.InArbeit,
            };
            _Db.tabArbeitszeitAuswertungSet.Add(az);
            _Db.SaveChanges();

            return az;
        }

        public void BerechneUeberstundenGesamt()
        {
            var erg = AuswertungVorjahr.fUeberstunden + AuswertungKumulativ.fUeberstunden + AuswertungMonat.fUeberstunden - AuswertungGesamt.fUeberstundenBezahlt;
            AuswertungGesamt.UeberstundenAnzeige = JgZeit.ZeitInString(erg);
        }

        public bool Kontrolle24StundenOK(TimeSpan Zeit)
        {
            return (Zeit >= TimeSpan.Zero) && (Zeit < new TimeSpan(24, 0, 0));
        }

        public void ListeFuerJedenTagErstellen(tabArbeitszeitAuswertung AuswertungBediener,
          IEnumerable<tabFeiertage> ListeFeiertageMonat,
          IEnumerable<tabPausenzeit> ListePausen, bool WerteInDb)
        {
            if (AuswertungBediener == null)
                return;

            ListeTage.Clear();

            // Werte für Tage berechnen
            var auswTage = _Db.tabArbeitszeitTagSet.Where(w => w.fArbeitszeitAuswertung == AuswertungBediener.Id).ToList();

            var anzTageMonat = DateTime.DaysInMonth(AuswertungBediener.Jahr, AuswertungBediener.Monat);

            var monatErster = JgZeit.ErsterImMonat(AuswertungBediener.Jahr, AuswertungBediener.Monat);
            var monatLetzter = JgZeit.LetzerImMonat(AuswertungBediener.Jahr, AuswertungBediener.Monat);

            var alleZeiten = _Db.tabArbeitszeitSet.Where(w => (w.fBediener == AuswertungBediener.fBediener) && (!w.DatenAbgleich.Geloescht)
              && (
                ((w.Anmeldung != null) && (w.Anmeldung >= monatErster) && (w.Anmeldung <= monatLetzter))
                ||
                ((w.Anmeldung == null) && (w.Abmeldung != null) && (w.Abmeldung >= monatErster) && (w.Abmeldung <= monatLetzter))
                )
            ).ToList();

            for (byte tag = 1; tag <= anzTageMonat; tag++)
            {
                var auswTag = auswTage.FirstOrDefault(f => f.Tag == tag);
                if (auswTag == null)
                {
                    auswTag = new tabArbeitszeitTag()
                    {
                        Id = Guid.NewGuid(),
                        fArbeitszeitAuswertung = AuswertungBediener.Id,
                        Tag = tag
                    };
                    _Db.tabArbeitszeitTagSet.Add(auswTag);
                }

                var aktDatum = new DateTime(AuswertungBediener.Jahr, AuswertungBediener.Monat, tag);
                auswTag.Wochentag = aktDatum.ToString("ddd");

                auswTag.IstSonnabend = aktDatum.DayOfWeek == DayOfWeek.Saturday;
                auswTag.IstSonntag = aktDatum.DayOfWeek == DayOfWeek.Sunday;
                auswTag.IstFeiertag = ListeFeiertageMonat.FirstOrDefault(f => f.Datum == aktDatum) != null;

                auswTag.ZeitBerechnet = TimeSpan.Zero;
                auswTag.NachtschichtBerechnet = TimeSpan.Zero;

                var zeiten = alleZeiten.Where(w => (w.Anmeldung?.Day == tag) || ((w.Abmeldung == null) && (w.Abmeldung?.Day == tag))).ToList();

                if (zeiten.Count > 0)
                {
                    foreach (var zeit in zeiten)
                    {
                        // Kontrolle ob Zeiten an Tagesauswertung hängt
                        if (zeit.eArbeitszeitAuswertung != auswTag)
                            zeit.eArbeitszeitAuswertung = auswTag;

                        zeit.AnmeldungGerundet = _AzRunden.GetZeitGerundet(EnumZeitpunkt.Anmeldung, zeit.fStandort, zeit.Anmeldung);
                        zeit.AbmeldungGerundet = _AzRunden.GetZeitGerundet(EnumZeitpunkt.Abmeldung, zeit.fStandort, zeit.Abmeldung);

                        if ((zeit.Anmeldung != null) && (zeit.Abmeldung != null))
                        {
                            auswTag.ZeitBerechnet += zeit.DauerGerundet;
                            auswTag.NachtschichtBerechnet += NachtSchichtBerechnen(22, 0, 8, 0, zeit.AnmeldungGerundet.Value, zeit.Abmeldung.Value);
                        }
                    }
                    auswTag.ZeitBerechnet = ZeitAufMinuteRunden(auswTag.ZeitBerechnet);
                    auswTag.NachtschichtBerechnet = ZeitAufMinuteRunden(auswTag.NachtschichtBerechnet);

                    // Pause berechnen

                    var ersteAnmeldungZeit = zeiten.Where(w => (w.Anmeldung != null)).Min(m => m.Anmeldung);
                    if (ersteAnmeldungZeit == null)
                        auswTag.PauseBerechnet = new TimeSpan(1, 0, 0);
                    else
                    {
                        var anmZeit = JgZeit.DatumInZeitMinute(ersteAnmeldungZeit.Value);
                        var dsPause = ListePausen.FirstOrDefault(w => (anmZeit >= w.ZeitVon) && (anmZeit <= w.ZeitBis));
                        if (dsPause != null)
                            auswTag.PauseBerechnet = dsPause.Pausenzeit;
                    }

                    auswTag.ZeitBerechnet -= auswTag.PauseBerechnet;
                    auswTag.IstFehlerZeit = !Kontrolle24StundenOK(auswTag.ZeitBerechnet);
                    auswTag.IstFehlerNachtschicht = !Kontrolle24StundenOK(auswTag.NachtschichtBerechnet);
                }

                auswTag.ArbeitszeitTagGeaendert = OnWertWurdeManuellGeaendert;

                ListeTage.Add(auswTag);
            }

            if (WerteInDb)
            {
                foreach (var tag in ListeTage)
                {
                    if (!tag.IstManuellGeaendert)
                    {
                        if (tag.Pause != tag.PauseBerechnet)
                        {
                            tag.Pause = JgZeit.KontrolleZeitDb(tag.PauseBerechnet);
                            tag.NotifyPropertyChanged("PauseAnzeige");
                        }

                        if (tag.Zeit != tag.ZeitBerechnet)
                        {
                            tag.Zeit = JgZeit.KontrolleZeitDb(tag.ZeitBerechnet);
                            tag.NotifyPropertyChanged("ZeitAnzeige");
                        }

                        if (tag.NachtschichtZuschlag != tag.NachtschichtBerechnet)
                        {
                            tag.NachtschichtZuschlag = JgZeit.KontrolleZeitDb(tag.NachtschichtBerechnet);
                            tag.NotifyPropertyChanged("NachtschichtZuschlagAnzeige");
                        }

                        if (tag.Feiertag != tag.IstFeiertag)
                        {
                            tag.Feiertag = tag.IstFeiertag;
                            tag.NotifyPropertyChanged("FeiertagAnzeige");
                        }
                    }
                }
            }
        }

        private void DatenMonatInDatenbank()
        {
            var dsAz = AuswertungMonat.AzAuswertung;

            dsAz.Ueberstunden = JgZeit.ZeitInString(AuswertungMonat.fUeberstunden);

            dsAz.NachtschichtZuschlaege = JgZeit.ZeitInString(AuswertungMonat.fNachtschichtZuschlaege);
            dsAz.FeiertagZuschlaege = JgZeit.ZeitInString(AuswertungMonat.fFeiertagZuschlaege);

            dsAz.Urlaub = (byte)AuswertungMonat.fUrlaub;
            dsAz.Feiertage = (byte)AuswertungMonat.fFeiertage;
            dsAz.Krank = (byte)AuswertungMonat.fKrank;

            _Db.SaveChanges();
        }

        private void OnWertWurdeManuellGeaendert(tabArbeitszeitTag AuswertungTag, string PropertyName)
        {
            if (PropertyName == "Pause")
            {
                var zeiten = AuswertungTag.sArbeitszeiten.Where(w => (w.Anmeldung != null) && (w.Abmeldung != null)).ToList();

                AuswertungTag.ZeitBerechnet = TimeSpan.Zero;
                AuswertungTag.NachtschichtBerechnet = TimeSpan.Zero;

                foreach (var zeit in zeiten)
                {
                    AuswertungTag.ZeitBerechnet += zeit.Dauer;
                    AuswertungTag.NachtschichtBerechnet += NachtSchichtBerechnen(22, 0, 8, 0, zeit.Anmeldung.Value, zeit.Abmeldung.Value);
                }
                AuswertungTag.ZeitBerechnet = ZeitAufMinuteRunden(AuswertungTag.ZeitBerechnet - AuswertungTag.Pause);
                AuswertungTag.NachtschichtBerechnet = ZeitAufMinuteRunden(AuswertungTag.NachtschichtBerechnet);

                BerechneUeberstundenAusTagen();
                AuswertungMonat.AzAuswertung.Ueberstunden = JgZeit.ZeitInString(AuswertungMonat.fUeberstunden);
            }
            else if (PropertyName == "Zeit")
            {
                BerechneUeberstundenAusTagen();
                AuswertungMonat.AzAuswertung.Ueberstunden = JgZeit.ZeitInString(AuswertungMonat.fUeberstunden);
            }
            else if (PropertyName == "Urlaub")
            {
                BerechneUrlaub();
                AuswertungMonat.AzAuswertung.Urlaub = (byte)AuswertungMonat.fUrlaub;
                BerechneUeberstundenAusTagen();
                AuswertungMonat.AzAuswertung.Ueberstunden = JgZeit.ZeitInString(AuswertungMonat.fUeberstunden);
            }
            else if (PropertyName == "Krank")
            {
                BerechneKrank();
                AuswertungMonat.AzAuswertung.Krank = (byte)AuswertungMonat.fKrank;
                BerechneUeberstundenAusTagen();
                AuswertungMonat.AzAuswertung.Ueberstunden = JgZeit.ZeitInString(AuswertungMonat.fUeberstunden);
            }
            else if (PropertyName == "Feiertag")
            {
                BerechneFeiertage();
                AuswertungMonat.AzAuswertung.Feiertage = (byte)AuswertungMonat.fFeiertage;
                BerechneUeberstundenAusTagen();
                AuswertungMonat.AzAuswertung.Ueberstunden = JgZeit.ZeitInString(AuswertungMonat.fUeberstunden);
            }
            else if (PropertyName == "FeiertagZuschlag")
            {
                BerechneFeiertagZuschlag();
                AuswertungMonat.AzAuswertung.FeiertagZuschlaege = JgZeit.ZeitInString(AuswertungMonat.fFeiertagZuschlaege);
            }
            else if (PropertyName == "NachtschichtZuschlag")
            {
                BerechneNachtschichtZuschlag();
                AuswertungMonat.AzAuswertung.NachtschichtZuschlaege = JgZeit.ZeitInString(AuswertungMonat.fNachtschichtZuschlaege);
            }

            if (AuswertungTag.IstManuellGeaendert == false)
            {
                AuswertungTag.IstManuellGeaendert = true;
                AuswertungTag.NotifyPropertyChanged("IstManuellGeaendert");
            }

            _Db.SaveChanges();
        }

        private void BerechneNachtschichtZuschlag()
        {
            var sumNachtschicht = new TimeSpan(ListeTage.Sum(c => c.NachtschichtZuschlag.Ticks));
            AuswertungMonat.NachtschichtZuschlaegeAnzeige = JgZeit.ZeitInString(sumNachtschicht);
        }

        private void BerechneFeiertage()
        {
            AuswertungMonat.FeiertageAnzeige = Convert.ToByte(ListeTage.Count(c => c.Feiertag));
        }

        private void BerechneFeiertagZuschlag()
        {
            var sumZeit = new TimeSpan(ListeTage.Sum(c => c.FeiertagZuschlag.Ticks));
            AuswertungMonat.FeiertagZuschlaegeAnzeige = JgZeit.ZeitInString(sumZeit);
        }

        private void BerechneUeberstundenBezahlt()
        {
            var ueberstundenBezahlt = AuswertungKumulativ.fUeberstundenBezahlt + AuswertungMonat.fUeberstundenBezahlt;
            AuswertungGesamt.UeberstundenBezahltAnzeige = JgZeit.ZeitInString(ueberstundenBezahlt);
        }

        private void BerechneKrank()
        {
            var krank = Convert.ToInt16(ListeTage.Count(c => c.Krank));
            AuswertungMonat.KrankAnzeige = krank;
            AuswertungGesamt.KrankAnzeige = (short)(krank + AuswertungKumulativ.fKrank);
        }

        private void BerechneUrlaub()
        {
            var urlaub = Convert.ToByte(ListeTage.Count(c => c.Urlaub));
            AuswertungMonat.UrlaubAnzeige = urlaub;
            AuswertungGesamt.UrlaubAnzeige = (byte)(AuswertungMonat.fUrlaub + AuswertungKumulativ.fUrlaub);
            AuswertungMonat.UrlaubOffenAnzeige = (short)(AuswertungMonat.UrlaubImJahr + AuswertungVorjahr.fUrlaub - AuswertungMonat.fUrlaub - AuswertungKumulativ.fUrlaub);
        }

        private void BerechneUeberstundenAusTagen()
        {
            var istZeit = new TimeSpan(ListeTage.Sum(c => c.Zeit.Ticks));
            var fTage = new TimeSpan(8 * (AuswertungMonat.fUrlaub + AuswertungMonat.fKrank + AuswertungMonat.fFeiertage), 0, 0);

            AuswertungMonat.UeberstundenAnzeige = JgZeit.ZeitInString(istZeit + fTage - AuswertungMonat.fSollStunden);

            BerechneUeberstundenGesamt();
            AuswertungMonat.NotifyPropertyChanged("IstStundenAnzeige");
        }

        public TimeSpan ZeitAufMinuteRunden(TimeSpan WertZumRunden)
        {
            return new TimeSpan((int)WertZumRunden.TotalHours, WertZumRunden.Minutes, 0);
        }

        public static TimeSpan NachtSchichtBerechnen(int NachtschichtStundeVon, int NachtschichtMinuteVon, int LaengeNachtschichtStunde, int LaengeNachtschichtMinute, DateTime DatumVon, DateTime DatumBis)
        {
            var sumNachtschicht = TimeSpan.Zero;

            // Damit Frühschicht berücksichtigt wird, beginnt NachtschichtZuschlag ein Tag vorher 
            var mDatum = DatumVon.Date.AddDays(-1);

            while (true)
            {
                var nsBeginn = new DateTime(mDatum.Year, mDatum.Month, mDatum.Day, NachtschichtStundeVon, NachtschichtMinuteVon, 0);
                var nsEnde = nsBeginn.AddHours(LaengeNachtschichtStunde).AddMinutes(LaengeNachtschichtMinute);
                mDatum = mDatum.AddDays(1);

                if (DatumBis < nsBeginn)
                    break;

                if (DatumVon < nsBeginn)
                    DatumVon = nsBeginn;

                if ((DatumVon >= nsBeginn) && (DatumVon < nsEnde))
                {
                    if (DatumBis <= nsEnde)
                    {
                        sumNachtschicht += DatumBis - DatumVon;
                        break;
                    }
                    sumNachtschicht += nsEnde - DatumVon;
                }
            };

            return sumNachtschicht;
        }

        public void SetSollstunden(TimeSpan Sollstunden)
        {
            if (Sollstunden != AuswertungMonat.fSollStunden)
            {
                var sollStunden = JgZeit.ZeitInString(Sollstunden);
                AuswertungMonat.AzAuswertung.SollStunden = sollStunden;
                _Db.SaveChanges();

                AuswertungMonat.SollStundenAnzeige = sollStunden;
                BerechneUeberstundenAusTagen();
                BerechneUeberstundenGesamt();
                AuswertungMonat.NotifyPropertyChanged("IstStundenAnzeige");
            }
        }

        public void SetUebestundenAuszahlung(TimeSpan UeberstundenAuszahlung)
        {
            if (UeberstundenAuszahlung != AuswertungMonat.fUeberstundenBezahlt)
            {
                AuswertungMonat.UeberstundenBezahltAnzeige = JgZeit.ZeitInString(UeberstundenAuszahlung);
                AuswertungMonat.AzAuswertung.AuszahlungUeberstunden = AuswertungMonat.UeberstundenBezahltAnzeige;
                AuswertungGesamt.UeberstundenBezahltAnzeige = JgZeit.ZeitInString(AuswertungKumulativ.fUeberstundenBezahlt + UeberstundenAuszahlung);

                BerechneUeberstundenGesamt();

                _Db.SaveChanges();
            }
        }
    }

    // ------------------------------------------------

    public abstract class ArbeitszeitHelperStamm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected abstract void AzAuswertungNeu();

        protected tabArbeitszeitAuswertung _AzAuswertung = null;
        public tabArbeitszeitAuswertung AzAuswertung
        {
            get { return _AzAuswertung; }
            set
            {
                if (value != _AzAuswertung)
                {
                    _AzAuswertung = value;
                    fUeberstunden = JgZeit.StringInZeit(_AzAuswertung.Ueberstunden);
                    fUrlaub = _AzAuswertung.Urlaub;
                    NotifyPropertyChanged("UeberstundenAnzeige");
                    NotifyPropertyChanged("UrlaubAnzeige");
                    AzAuswertungNeu();
                }
            }
        }

        public TimeSpan fUeberstunden = TimeSpan.Zero;
        public string UeberstundenAnzeige
        {
            get { return JgZeit.ZeitInString(fUeberstunden); }
            set
            {
                var zeit = JgZeit.StringInZeit(value, fUeberstunden);
                if (fUeberstunden != zeit)
                {
                    fUeberstunden = zeit;
                    NotifyPropertyChanged();
                }
            }
        }

        public short fUrlaub = 0;
        public short UrlaubAnzeige
        {
            get { return fUrlaub; }
            set
            {
                if (value != fUrlaub)
                {
                    fUrlaub = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }

    public class ArbeitszeitHelperVorjahr : ArbeitszeitHelperStamm
    {
        protected override void AzAuswertungNeu()
        { }
    }

    public class ArbeitszeitHelperAktuell : ArbeitszeitHelperStamm
    {
        protected override void AzAuswertungNeu()
        {
            fUeberstundenBezahlt = JgZeit.StringInZeit(_AzAuswertung.AuszahlungUeberstunden, TimeSpan.Zero);
            fKrank = _AzAuswertung.Krank;
            NotifyPropertyChanged("UeberstundenBezahltAnzeige");
            NotifyPropertyChanged("KrankAnzeige");
        }

        public TimeSpan fUeberstundenBezahlt = TimeSpan.Zero;
        public string UeberstundenBezahltAnzeige
        {
            get { return JgZeit.ZeitInString(fUeberstundenBezahlt); }
            set
            {
                var zeit = JgZeit.StringInZeit(value, fUeberstundenBezahlt);
                if (zeit != fUeberstundenBezahlt)
                {
                    fUeberstundenBezahlt = zeit;
                    NotifyPropertyChanged();
                }
            }
        }

        public short fKrank = 0;
        public short KrankAnzeige
        {
            get { return fKrank; }
            set
            {
                if (value != fKrank)
                {
                    fKrank = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }

    public class ArbeitszeitHelperMonat : ArbeitszeitHelperAktuell
    {
        protected override void AzAuswertungNeu()
        {
            base.AzAuswertungNeu();

            fSollStunden = JgZeit.StringInZeit(_AzAuswertung.SollStunden);
            fFeiertage = _AzAuswertung.Feiertage;
            fNachtschichtZuschlaege = JgZeit.StringInZeit(_AzAuswertung.NachtschichtZuschlaege);
            fFeiertagZuschlaege = JgZeit.StringInZeit(_AzAuswertung.FeiertagZuschlaege);
            NotifyPropertyChanged("NachtschichtZuschlaegeAnzeige");
            NotifyPropertyChanged("FeiertagZuschlaegeAnzeige");
            NotifyPropertyChanged("FeiertagAnzeige");
            NotifyPropertyChanged("IstStundenAnzeige");
            NotifyPropertyChanged("SollStundenAnzeige");
            NotifyPropertyChanged("FeiertageAnzeige");
            NotifyPropertyChanged("UrlaubImJahr");
        }

        private short fUrlaubOffen = 0;
        public short UrlaubOffenAnzeige
        {
            get { return fUrlaubOffen; }
            set
            {
                if (value != fUrlaubOffen)
                {
                    fUrlaubOffen = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TimeSpan fNachtschichtZuschlaege = TimeSpan.Zero;
        public string NachtschichtZuschlaegeAnzeige
        {
            get { return JgZeit.ZeitInString(fNachtschichtZuschlaege); }
            set
            {
                var zeit = JgZeit.StringInZeit(value, fNachtschichtZuschlaege);
                if (fNachtschichtZuschlaege != zeit)
                {
                    fNachtschichtZuschlaege = zeit;
                    NotifyPropertyChanged();
                }
            }
        }

        public TimeSpan fFeiertagZuschlaege = TimeSpan.Zero;
        public string FeiertagZuschlaegeAnzeige
        {
            get { return JgZeit.ZeitInString(fFeiertagZuschlaege); }
            set
            {
                var zeit = JgZeit.StringInZeit(value, fFeiertagZuschlaege);
                if (zeit != fFeiertagZuschlaege)
                {
                    fFeiertagZuschlaege = zeit;
                    NotifyPropertyChanged();
                }
            }
        }

        public byte fFeiertage = 0;
        public byte FeiertageAnzeige
        {
            get { return fFeiertage; }
            set
            {
                if (value != fFeiertage)
                {
                    fFeiertage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TimeSpan fSollStunden = TimeSpan.Zero;
        public string SollStundenAnzeige
        {
            get { return JgZeit.ZeitInString(fSollStunden); }
            set
            {
                var zeit = JgZeit.StringInZeit(value, fSollStunden);
                if (zeit != fSollStunden)
                {
                    fSollStunden = zeit;
                    NotifyPropertyChanged();
                }
            }
        }

        public string IstStundenAnzeige
        {
            get { return JgZeit.ZeitInString(fSollStunden + fUeberstunden); }
        }

        public byte UrlaubImJahr
        {
            get { return _AzAuswertung?.eBediener?.Urlaubstage ?? 0; }
        }
    }
}
