using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JgMaschineData;
using JgZeitHelper;

namespace JgMaschineGlobalZeit
{
  public class ArbeitszeitBediener
  {
    public JgModelContainer Db;

    public tabBediener Bediener { get; set; }   // Für Auswahl in Reporten
    public Guid IdBediener { get { return Bediener.Id; } }

    public ArbeitszeitHelperVorjahr AuswertungVorjahr { get; set; } = new ArbeitszeitHelperVorjahr();
    public ArbeitszeitHelperAktuell AuswertungKumulativ { get; set; } = new ArbeitszeitHelperAktuell();
    public ArbeitszeitHelperMonat AuswertungMonat { get; set; } = new ArbeitszeitHelperMonat();
    public ArbeitszeitHelperAktuell AuswertungGesamt { get; set; } = new ArbeitszeitHelperAktuell();

    public ObservableCollection<tabArbeitszeitTag> ListeTage { get; set; } = new ObservableCollection<tabArbeitszeitTag>();

    public ArbeitszeitBediener()
    { }

    public ArbeitszeitBediener(JgModelContainer NeuDb)
    {
      Db = NeuDb;
    }

    public void BedienerBerechnen(tabBediener Bediener, short Jahr, byte Monat, TimeSpan SollStundenMonat,
      IEnumerable<tabArbeitszeitRunden> ListeRundenMonat,
      IEnumerable<tabFeiertage> ListeFeiertageMonat,
      IEnumerable<tabPausenzeit> ListePausen,
      bool WerteAusTagenNeuBerechnen = true)
    {
      this.Bediener = Bediener;

      var alleAuswertungenBediener = Db.tabArbeitszeitAuswertungSet.Where(w => (w.fBediener == Bediener.Id) && (w.Jahr == Jahr) && (w.Monat <= Monat)).ToList();

      var auswErster = alleAuswertungenBediener.FirstOrDefault(w => w.Monat == 0);
      if (auswErster == null)
        auswErster = ArbeitszeitAuswertungErstellen(Bediener, Jahr, 0, TimeSpan.Zero);
      AuswertungVorjahr.AzAuswertung = auswErster;

      var auswMonat = alleAuswertungenBediener.FirstOrDefault(w => (w.Monat == Monat));
      if (auswMonat == null)
        auswMonat = ArbeitszeitAuswertungErstellen(Bediener, Jahr, Monat, SollStundenMonat);
      AuswertungMonat.AzAuswertung = auswMonat;


      var auswKumulativ = alleAuswertungenBediener.Where(w => (w.Monat > 0) && (w.Monat < Monat)).ToList();

      var sumUeberstunden = new TimeSpan(auswKumulativ.Sum(s => JgZeit.StringInZeit(s.Ueberstunden).Ticks));
      AuswertungKumulativ.UeberstundenAnzeige = JgZeit.ZeitInString(sumUeberstunden);

      var sumUeberstBezahlt = new TimeSpan(auswKumulativ.Sum(s => JgZeit.StringInZeit(s.AuszahlungUeberstunden).Ticks));
      AuswertungKumulativ.UeberstundenBezahltAnzeige = JgZeit.ZeitInString(sumUeberstBezahlt);

      AuswertungKumulativ.KrankAnzeige = (Int16)auswKumulativ.Sum(s => s.Krank);
      AuswertungKumulativ.UrlaubAnzeige = (Int16)auswKumulativ.Sum(s => s.Urlaub);

      ListeFuerJedenTagErstellen(Db, Bediener.eArbeitszeitHelper, ListeRundenMonat, ListeFeiertageMonat, ListePausen);
      if (WerteAusTagenNeuBerechnen)
      {
        BerechneUeberstundenAusTagen(ListeTage);
        BerechneNachtschicht(ListeTage);
        BerechneFeiertag(ListeTage);
      }

      BerechneUeberstundenBezahlt();
      BerechneUrlaub(ListeTage, WerteAusTagenNeuBerechnen);
      BerechneKrank(ListeTage, WerteAusTagenNeuBerechnen);
      BerechneUeberstundenGesamt();
    }

    private tabArbeitszeitAuswertung ArbeitszeitAuswertungErstellen(tabBediener Bediener, short Jahr, byte Monat, TimeSpan SollStundenMonat)
    {
      var az = new tabArbeitszeitAuswertung()
      {
        Id = Guid.NewGuid(),
        eBediener = Bediener,
        Jahr = Jahr,
        Monat = Monat,
        Urlaub = 0,
        AuszahlungUeberstunden = "00:00",
        SollStunden = JgZeit.ZeitInString(SollStundenMonat),
        Status = EnumStatusArbeitszeitAuswertung.InArbeit,
      };
      Db.tabArbeitszeitAuswertungSet.Add(az);
      Db.SaveChanges();

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

    public void ListeFuerJedenTagErstellen(JgModelContainer Db, tabArbeitszeitAuswertung AuswertungBediener,
      IEnumerable<tabArbeitszeitRunden> ListeRundenMonat,
      IEnumerable<tabFeiertage> ListeFeiertageMonat,
      IEnumerable<tabPausenzeit> ListePausen,
      bool ListeTageErstellen = true)
    {
      // Werte für Tage berechnen
      var auswTage = Db.tabArbeitszeitTagSet.Where(w => w.fArbeitszeitAuswertung == AuswertungBediener.Id).ToList();

      var anzTageMonat = DateTime.DaysInMonth(AuswertungBediener.Jahr, AuswertungBediener.Monat);
      ListeTage.Clear();

      var monatErster = JgZeit.ErsterImMonat(AuswertungBediener.Jahr, AuswertungBediener.Monat);
      var monatLetzter = JgZeit.LetzerImMonat(AuswertungBediener.Jahr, AuswertungBediener.Monat);

      var alleZeiten = Db.tabArbeitszeitSet.Where(w => (w.fBediener == AuswertungBediener.fBediener) && (!w.DatenAbgleich.Geloescht)
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
          Db.tabArbeitszeitTagSet.Add(auswTag);
        }

        var aktDatum = new DateTime(AuswertungBediener.Jahr, AuswertungBediener.Monat, tag);
        auswTag.Wochentag = aktDatum.ToString("ddd");

        auswTag.IstSonnabend = aktDatum.DayOfWeek == DayOfWeek.Saturday;
        auswTag.IstSonntag = aktDatum.DayOfWeek == DayOfWeek.Sunday;
        auswTag.IstFeiertag = ListeFeiertageMonat.FirstOrDefault(f => f.Datum == aktDatum) != null;

        auswTag.ZeitBerechnet = TimeSpan.Zero;
        auswTag.NachtschichtBerechnet = TimeSpan.Zero;

        var zeiten = alleZeiten.Where(w => (w.Anmeldung?.Day == tag) || ((w.Abmeldung == null) && (w.Abmeldung?.Day == tag))).ToList();
        var ersteAnmeldungZeit = TimeSpan.Zero;

        if (zeiten.Count > 0)
        {
          foreach (var zeit in zeiten)
          {
            // Kontrolle ob Zeiten an Tagesauswertung hängt
            if (zeit.eArbeitszeitAuswertung != auswTag)
              zeit.eArbeitszeitAuswertung = auswTag;

            if (zeit.Anmeldung != null)
            {
              // Anfangszeiten Runden heraussuchen
              var zeitAnmeldung = JgZeit.DatumInZeitMinute(zeit.Anmeldung.Value);

              if ((ersteAnmeldungZeit == TimeSpan.Zero) || (zeitAnmeldung < ersteAnmeldungZeit))
                ersteAnmeldungZeit = zeitAnmeldung;

              var wg = ListeRundenMonat.FirstOrDefault(f => (zeitAnmeldung >= f.ZeitVon) && (zeitAnmeldung <= f.ZeitBis) && (f.fStandort == zeit.fStandort));
              if (wg != null)
                zeit.AnmeldungGerundetWert = zeit.Anmeldung.Value.Date.Add(wg.RundenArbeitszeitBeginn);

              if (zeit.Abmeldung != null)
              {
                auswTag.ZeitBerechnet += zeit.DauerGerundet;
                auswTag.NachtschichtBerechnet += NachtSchichtBerechnen(22, 0, 8, 0, zeit.AnmeldungGerundet.Value, zeit.Abmeldung.Value);
              }
            }
          }
          auswTag.ZeitBerechnet = ZeitAufMinuteRunden(auswTag.ZeitBerechnet);
          auswTag.NachtschichtBerechnet = ZeitAufMinuteRunden(auswTag.NachtschichtBerechnet);

          // Pause berechnen

          if (ersteAnmeldungZeit == TimeSpan.Zero)
            auswTag.PauseBerechnet = new TimeSpan(1, 0, 0);
          else
          {
            var dsPause = ListePausen.FirstOrDefault(w => (ersteAnmeldungZeit >= w.ZeitVon) && (ersteAnmeldungZeit <= w.ZeitBis));
            if (dsPause != null)
              auswTag.PauseBerechnet = dsPause.Pausenzeit;
          }

          auswTag.ZeitBerechnet -= auswTag.PauseBerechnet;
          auswTag.IstFehlerZeit = !Kontrolle24StundenOK(auswTag.ZeitBerechnet);
          auswTag.IstFehlerNachtschicht = !Kontrolle24StundenOK(auswTag.NachtschichtBerechnet);
        }

        auswTag.ArbeitszeitTagGeaendert = WertWurdeManuellGeaendert;

        ListeTage.Add(auswTag);
      }

      if (AuswertungBediener.Status == EnumStatusArbeitszeitAuswertung.InArbeit)
      {
        foreach (var auswTag in ListeTage)
        {
          if (!auswTag.IstManuellGeaendert)
          {
            if (auswTag.Pause != auswTag.PauseBerechnet)
              auswTag.Pause = auswTag.PauseBerechnet;

            var z = (auswTag.IstFehlerZeit) ? TimeSpan.Zero : auswTag.ZeitBerechnet;
            if (z != auswTag.Zeit)
              auswTag.Zeit = z;

            z = (auswTag.IstFehlerNachtschicht) ? TimeSpan.Zero : auswTag.NachtschichtBerechnet;
            if (z != auswTag.Nachtschicht)
              auswTag.Nachtschicht = z;
          }
        }

        Db.SaveChanges();
      }
    }

    private void WertWurdeManuellGeaendert(tabArbeitszeitTag AuswertungTag, string PropertyName)
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

        BerechneUeberstundenAusTagen(ListeTage);
      }
      else if (PropertyName == "Zeit")
        BerechneUeberstundenAusTagen(ListeTage);
      else if (PropertyName == "Urlaub")
      {
        BerechneUrlaub(ListeTage, true);
        BerechneUeberstundenAusTagen(ListeTage);
      }
      else if (PropertyName == "Krank")
      {
        BerechneKrank(ListeTage, true);
        BerechneUeberstundenAusTagen(ListeTage);
      }
      else if (PropertyName == "Feiertag")
        BerechneFeiertag(ListeTage);
      else if (PropertyName == "Nachtschicht")
        BerechneNachtschicht(ListeTage);

      AuswertungTag.IstManuellGeaendert = true;

      Db.SaveChanges();
    }

    private void BerechneNachtschicht(IEnumerable<tabArbeitszeitTag> listeTage)
    {
      var sumNachtschicht = new TimeSpan(listeTage.Sum(c => c.Nachtschicht.Ticks));
      AuswertungMonat.NachtschichtAnzeige = JgZeit.ZeitInString(sumNachtschicht);
    }

    private void BerechneFeiertag(IEnumerable<tabArbeitszeitTag> listeTage)
    {
      var sumZeit = new TimeSpan(listeTage.Sum(c => c.Feiertag.Ticks));
      AuswertungMonat.FeiertageAnzeige = JgZeit.ZeitInString(sumZeit);
    }

    private void BerechneUeberstundenBezahlt()
    {
      var ueberstundenBezahlt = AuswertungKumulativ.fUeberstundenBezahlt + AuswertungMonat.fUeberstundenBezahlt;
      AuswertungGesamt.UeberstundenBezahltAnzeige = JgZeit.ZeitInString(ueberstundenBezahlt);
    }

    private void BerechneKrank(IEnumerable<tabArbeitszeitTag> listeTage, bool WerteAusTagenNeuBerechnen)
    {
      var krank = Convert.ToInt16(listeTage.Count(c => c.Krank));
      AuswertungMonat.KrankAnzeige = krank;
      AuswertungGesamt.KrankAnzeige = (short)(krank + AuswertungKumulativ.fKrank);
    }

    private void BerechneUrlaub(IEnumerable<tabArbeitszeitTag> listeTage, bool WerteAusTagenNeuBerechnen)
    {
      if (WerteAusTagenNeuBerechnen)
      {
        var urlaub = Convert.ToByte(listeTage.Count(c => c.Urlaub));
        AuswertungMonat.UrlaubAnzeige = urlaub;
      }

      AuswertungGesamt.UrlaubAnzeige = (byte)(AuswertungMonat.fUrlaub + AuswertungKumulativ.fUrlaub);
      AuswertungMonat.UrlaubOffenAnzeige = (short)(AuswertungMonat.UrlaubImJahr + AuswertungVorjahr.fUrlaub - AuswertungMonat.fUrlaub - AuswertungKumulativ.fUrlaub);
    }

    private void BerechneUeberstundenAusTagen(IEnumerable<tabArbeitszeitTag> listeTage)
    {
      var sumZeit = TimeSpan.Zero;
      var hinzuTage = 0;
      foreach (var tag in listeTage)
      {
        sumZeit += tag.Zeit;
        if (tag.IstFeiertag && !tag.IstSonnabend && !tag.IstSonntag)
          ++hinzuTage;
        else if (tag.Urlaub)
          ++hinzuTage;
        else if (tag.Krank)
          ++hinzuTage;
      }

      var istStunden = new TimeSpan(8 * hinzuTage, 0, 0) + ZeitAufMinuteRunden(sumZeit);
      AuswertungMonat.UeberstundenAnzeige = JgZeit.ZeitInString(istStunden - AuswertungMonat.fSollStunden);
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

      // Damit Frühschicht berücksichtigt wird, beginnt Nachtschicht ein Tag vorher 
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
        var istStunden = JgZeit.StringInZeit(AuswertungMonat.IstStundenAnzeige);
        AuswertungMonat.SollStundenAnzeige = JgZeit.ZeitInString(Sollstunden);
        AuswertungMonat.UeberstundenAnzeige = JgZeit.ZeitInString(istStunden - Sollstunden);
        BerechneUeberstundenGesamt();
        AuswertungMonat.NotifyPropertyChanged("IstStundenAnzeige");
        Db.SaveChanges();
      }
    }

    public void SetUebestundenAuszahlung(TimeSpan UeberstundenAuszahlung)
    {
      if (UeberstundenAuszahlung != AuswertungMonat.fUeberstundenBezahlt)
      {
        AuswertungMonat.UeberstundenBezahltAnzeige = JgZeit.ZeitInString(UeberstundenAuszahlung);
        AuswertungGesamt.UeberstundenBezahltAnzeige = JgZeit.ZeitInString(AuswertungKumulativ.fUeberstundenBezahlt + UeberstundenAuszahlung);
        BerechneUeberstundenGesamt();

        Db.SaveChanges();
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
          if (_AzAuswertung != null)
            _AzAuswertung.Ueberstunden = JgZeit.ZeitInString(zeit);
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
          if (_AzAuswertung != null)
            _AzAuswertung.Urlaub = fUrlaub;
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
          if (_AzAuswertung != null)
            _AzAuswertung.AuszahlungUeberstunden = JgZeit.ZeitInString(zeit);
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
          if (_AzAuswertung != null)
            _AzAuswertung.Krank = value;
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

      fNachtschicht = JgZeit.StringInZeit(_AzAuswertung.Nachtschichten);
      fFeiertage = JgZeit.StringInZeit(_AzAuswertung.Feiertage);
      fSollStunden = JgZeit.StringInZeit(_AzAuswertung.SollStunden);
      NotifyPropertyChanged("NachtschichtAnzeige");
      NotifyPropertyChanged("FeiertageAnzeige");
      NotifyPropertyChanged("IstStundenAnzeige");
      NotifyPropertyChanged("SollStundenAnzeige");
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

    public TimeSpan fNachtschicht = TimeSpan.Zero;
    public string NachtschichtAnzeige
    {
      get { return JgZeit.ZeitInString(fNachtschicht); }
      set
      {
        var zeit = JgZeit.StringInZeit(value, fNachtschicht);
        if (fNachtschicht != zeit)
        {
          fNachtschicht = zeit;
          if (_AzAuswertung != null)
            _AzAuswertung.Nachtschichten = JgZeit.ZeitInString(zeit);
          NotifyPropertyChanged();
        }
      }
    }

    public TimeSpan fFeiertage = TimeSpan.Zero;
    public string FeiertageAnzeige
    {
      get { return JgZeit.ZeitInString(fFeiertage); }
      set
      {
        var zeit = JgZeit.StringInZeit(value, fFeiertage);
        if (zeit != fFeiertage )
        {
          fFeiertage = zeit;
          if (_AzAuswertung != null)
            _AzAuswertung.Feiertage = JgZeit.ZeitInString(zeit);
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
          if (_AzAuswertung != null)
            _AzAuswertung.SollStunden = JgZeit.ZeitInString(zeit);
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
      get { return _AzAuswertung?.eBediener?.Urlaubstage ?? 0;  }
    }  
  }
}
