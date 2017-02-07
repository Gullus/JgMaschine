﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineGlobalZeit
{
  public class AnmeldungAuswertung
  {
    private JgModelContainer _Db;
    public JgModelContainer Db { get { return _Db; } }

    private ComboBox _CmbJahr;
    private ComboBox _CmbMonat;

    public ObservableCollection<tabBediener> ListeBediener;

    // Wegen Optionen wird immer gleich das Ganze Jahr geladen

    public ObservableCollection<tabPausenzeit> ListePausen;
    public ObservableCollection<tabSollStunden> ListeSollstundenJahr;
    public ObservableCollection<tabFeiertage> ListeFeiertageJahr;
    public ObservableCollection<tabArbeitszeitRunden> ListeRundenJahr;

    public TimeSpan SollStundenMonat = TimeSpan.Zero;
    public List<tabFeiertage> ListeFeiertageMonat;
    public List<tabArbeitszeitRunden> ListeRundenMonat;

    public ArbeitszeitBediener AuswertungBediener = null;

    private CollectionViewSource _VsBediener;
    private CollectionViewSource _VsAnzeigeTage = null;

    public short Jahr
    {
      get { return Convert.ToInt16(_CmbJahr.SelectedItem); }
      set
      {
        _CmbJahr.SelectedItem = value;
        JahrGeandert();
        MonatGeandert();
      }
    }
    public byte Monat
    {
      get { return (byte)_CmbMonat.SelectedItem; }
      set
      {
        _CmbMonat.SelectedItem = value;
        MonatGeandert();
      }
    }

    public tabBediener AktuellerBediener { get { return (tabBediener)_VsBediener?.View?.CurrentItem ?? null; } }

    public AnmeldungAuswertung(JgModelContainer Db, ComboBox CmbJahr, ComboBox CmbMonat, CollectionViewSource VsBediener,
      ArbeitszeitSummen AzsKumulativ, ArbeitszeitSummen AzsGesamt, CollectionViewSource VsAnzeigeTage)
    {
      _Db = Db;
      _VsBediener = VsBediener;
      _VsAnzeigeTage = VsAnzeigeTage;

      var heute = DateTime.Now.Date;

      _CmbJahr = CmbJahr;
      var jahrStart = (Int16)(heute.Year - 10);
      for (Int16 i = jahrStart; i < jahrStart + 12; i++)
        _CmbJahr.Items.Add(i);
      _CmbJahr.SelectedIndex = _CmbJahr.Items.IndexOf((Int16)heute.Year);

      _CmbJahr.SelectionChanged += (sen, erg) =>
      {
        if ((sen as ComboBox).IsKeyboardFocusWithin)
        {
          JahrGeandert();
          MonatGeandert();
        }
      };

      _CmbMonat = CmbMonat;
      _CmbMonat.ItemsSource = Enum.GetValues(typeof(ZeitHelper.Monate));
      _CmbMonat.SelectedIndex = _CmbMonat.Items.IndexOf((ZeitHelper.Monate)heute.Month);

      _CmbMonat.SelectionChanged += (sen, erg) =>
      {
        if ((sen as ComboBox).IsKeyboardFocusWithin)
          MonatGeandert();
      };

      var bediener = _Db.tabBedienerSet.Where(w => (w.Status == EnumStatusBediener.Aktiv)).OrderBy(o => o.NachName).ToList();

      ListeBediener = new ObservableCollection<tabBediener>(bediener);
      _VsBediener.GroupDescriptions.Add(new PropertyGroupDescription("eStandort.Bezeichnung"));
      _VsBediener.Source = ListeBediener;
      _VsBediener.View.CurrentChanged += (sen, erg) =>
      {
        BenutzerGeaendert();
      };

      AuswertungBediener = new ArbeitszeitBediener(_Db)
      {
        AuswertungKumulativ = AzsKumulativ,
        AuswertungGesamt = AzsGesamt
      };

      VsAnzeigeTage.Source = AuswertungBediener.ListeTage;
      JahrGeandert();
    }

    public void JahrGeandert()
    {
      var pausen = _Db.tabPausenzeitSet.Where(w => (!w.DatenAbgleich.Geloescht));
      ListePausen = new ObservableCollection<tabPausenzeit>(pausen.ToList());

      var jahr = Jahr;

      var sollStunden = _Db.tabSollStundenSet.Where(f => (f.Jahr == jahr));
      ListeSollstundenJahr = new ObservableCollection<tabSollStunden>(sollStunden.ToList());

      var ersterJahr = new DateTime(jahr, 1, 1);
      var letzterJahr = new DateTime(jahr, 12, 31, 23, 59, 59);

      var feiertage = _Db.tabFeiertageSet.Where(w => (!w.DatenAbgleich.Geloescht) && (w.Datum >= ersterJahr) && (w.Datum <= letzterJahr));
      ListeFeiertageJahr = new ObservableCollection<tabFeiertage>(feiertage.ToList());

      var runden = _Db.tabArbeitszeitRundenSet.Where(w => (w.Jahr == jahr));
      ListeRundenJahr = new ObservableCollection<tabArbeitszeitRunden>(runden.ToList());

      MonatGeandert();
    }

    public void MonatGeandert()
    {
      var monat = Monat;

      var sollStunde = ListeSollstundenJahr.FirstOrDefault(f => (f.Monat == monat) && (!f.DatenAbgleich.Geloescht));
      SollStundenMonat = (sollStunde == null) ? TimeSpan.Zero : Helper.StringInZeit(sollStunde.SollStunden);

      ListeFeiertageMonat = ListeFeiertageJahr.Where(w => (w.Datum.Month == monat) && (!w.DatenAbgleich.Geloescht)).ToList();
      ListeRundenMonat = _Db.tabArbeitszeitRundenSet.Where(w => (w.Monat == monat) && (!w.DatenAbgleich.Geloescht)).ToList();

      var idisBediener = ListeBediener.Select(s => s.Id).ToArray();
      var listAuswertungen = _Db.tabArbeitszeitAuswertungSet.Where(w => (idisBediener.Contains(w.fBediener) && (w.Jahr == Jahr) && (w.Monat == monat))).ToList();

      foreach (var bediener in ListeBediener)
        bediener.eArbeitszeitHelper = listAuswertungen.FirstOrDefault(f => f.eBediener == bediener);

      BenutzerGeaendert();
    }

    public void BenutzerGeaendert()
    {
      AuswertungBediener.BedienerBerechnen(AktuellerBediener, Jahr, Monat, SollStundenMonat, ListeRundenMonat, ListeFeiertageMonat, ListePausen);
    }
  }

  public class ArbeitszeitBediener
  {
    private JgModelContainer _Db;
    private tabBediener _Bediener = null;

    public ArbeitszeitSummen AuswertungGesamt = null;
    public ArbeitszeitSummen AuswertungKumulativ = null;

    public ObservableCollection<tabArbeitszeitTag> ListeTage = new ObservableCollection<tabArbeitszeitTag>();

    public ArbeitszeitBediener(JgModelContainer Db)
    {
      _Db = Db;
    }

    public void BedienerBerechnen(tabBediener Bediener, short Jahr, byte Monat, TimeSpan SollStundenMonat,
      IEnumerable<tabArbeitszeitRunden> ListeRundenMonat,
      IEnumerable<tabFeiertage> ListeFeiertageMonat,
      IEnumerable<tabPausenzeit> ListePausen)
    {
      if (Bediener == null)
        return;

      _Bediener = Bediener;

      var alleAuswertungenBediener = _Db.tabArbeitszeitAuswertungSet.Where(w => (w.fBediener == Bediener.Id) && (w.Jahr == Jahr) && (w.Monat <= Monat)).ToList();

      var auswErster = alleAuswertungenBediener.FirstOrDefault(w => w.Monat == 0);
      if (auswErster == null)
        auswErster = ArbeitszeitAuswertungErstellen(Bediener, Jahr, 0, TimeSpan.Zero);

      _Bediener.eArbeitszeitHelper = alleAuswertungenBediener.FirstOrDefault(w => (w.Monat == Monat));
      if (Bediener.eArbeitszeitHelper == null)
        Bediener.eArbeitszeitHelper = ArbeitszeitAuswertungErstellen(Bediener, Jahr, Monat, SollStundenMonat);

      var auswKumulativ = alleAuswertungenBediener.Where(w => (w.Monat > 0) && (w.Monat < Monat)).ToList();

      var sumUeberstunden = new TimeSpan(auswKumulativ.Sum(s => ZeitHelper.StringInZeit(s.Ueberstunden).Ticks));
      AuswertungKumulativ.UeberstundenString = ZeitHelper.ZeitInString(sumUeberstunden);

      var sumUeberstBezahlt = new TimeSpan(auswKumulativ.Sum(s => ZeitHelper.StringInZeit(s.AuszahlungUeberstunden).Ticks));
      AuswertungKumulativ.UeberstundenBezahltString = ZeitHelper.ZeitInString(sumUeberstBezahlt);

      AuswertungKumulativ.KrankAnzeige = (Int16)auswKumulativ.Sum(s => s.Krank);
      AuswertungKumulativ.UrlaubAnzeige = (Int16)auswKumulativ.Sum(s => s.Urlaub);
      AuswertungKumulativ.UrlaubVorjahrAnzeige = auswErster.Urlaub;
      AuswertungKumulativ.UeberstundenVorjahrString = auswErster.Ueberstunden;
      AuswertungKumulativ.RestUrlaubAnzeige = auswErster.Urlaub;

      ListeFuerJedenTagErstellen(_Db, Bediener.eArbeitszeitHelper, ListeRundenMonat, ListeFeiertageMonat, ListePausen);

      BerechneUeberstundenAusTagen(ListeTage);
      BerechneUeberstundenBezahlt();
      BerechneUrlaub(ListeTage);
      BerechneKrank(ListeTage);

      BerechneNachtschicht(ListeTage);
      BerechneFeiertag(ListeTage);

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
        SollStunden = Helper.ZeitInString(SollStundenMonat),
        Status = EnumStatusArbeitszeitAuswertung.InArbeit,
      };
      DbSichern.AbgleichEintragen(az.DatenAbgleich, EnumStatusDatenabgleich.Neu);
      _Db.tabArbeitszeitAuswertungSet.Add(az);
      _Db.SaveChanges();

      return az;
    }

    public void BerechneUeberstundenGesamt()
    {
      var erg = AuswertungKumulativ.fUeberstundenVorjahr + AuswertungKumulativ.fUeberstunden
        + ZeitHelper.StringInZeit(_Bediener.eArbeitszeitHelper.Ueberstunden)
        - AuswertungGesamt.fUeberstundenBezahlt;
      AuswertungGesamt.UeberstundenString = ZeitHelper.ZeitInString(erg);
    }

    public bool Kontrolle24StundenOK(TimeSpan Zeit)
    {
      return (Zeit >= TimeSpan.Zero) && (Zeit < new TimeSpan(24, 0, 0));
    }

    public void ListeFuerJedenTagErstellen(JgModelContainer Db, tabArbeitszeitAuswertung AuswertungBediener,
      IEnumerable<tabArbeitszeitRunden> ListeRundenMonat,
      IEnumerable<tabFeiertage> ListeFeiertageMonat,
      IEnumerable<tabPausenzeit> ListePausen)
    {
      // Werte für Tage berechnen
      var auswTage = Db.tabArbeitszeitTagSet.Where(w => w.fArbeitszeitAuswertung == AuswertungBediener.Id).ToList();

      var anzTageMonat = DateTime.DaysInMonth(AuswertungBediener.Jahr, AuswertungBediener.Monat);
      ListeTage.Clear();

      var monatErster = Helper.MonatErster(AuswertungBediener.Jahr, AuswertungBediener.Monat);
      var monatLetzter = Helper.MonatLetzter(AuswertungBediener.Jahr, AuswertungBediener.Monat);

      var alleZeiten = Db.tabArbeitszeitSet.Where(w => (w.fBediener == AuswertungBediener.fBediener)
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
          DbSichern.AbgleichEintragen(auswTag.DatenAbgleich, EnumStatusDatenabgleich.Neu);
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
              var zeitAnmeldung = DatumInZeit(zeit.Anmeldung);

              if ((ersteAnmeldungZeit == TimeSpan.Zero) || (zeitAnmeldung < ersteAnmeldungZeit))
                ersteAnmeldungZeit = zeitAnmeldung;

              var wg = ListeRundenMonat.FirstOrDefault(f => (zeitAnmeldung >= f.ZeitVon) && (zeitAnmeldung <= f.ZeitBis) && (f.fStandort == zeit.fStandort));
              if (wg != null)
                zeit.AnmeldungGerundetWert = zeit.Anmeldung.Value.Date.Add(wg.RundenAufZeit);

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

        ListeTage.Add(auswTag);
      }

      foreach (var auswTag in ListeTage)
      {
        if ((!auswTag.IstManuellGeaendert) && (auswTag.sArbeitszeiten.Count > 0))
        {
          auswTag.Pause = auswTag.PauseBerechnet;
          auswTag.Zeit = (auswTag.IstFehlerZeit) ? TimeSpan.Zero : auswTag.ZeitBerechnet;
          auswTag.Nachtschicht = (auswTag.IstFehlerNachtschicht) ? TimeSpan.Zero : auswTag.NachtschichtBerechnet;
          auswTag.IstManuellGeaendert = true;

          DbSichern.AbgleichEintragen(auswTag.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }

        if (auswTag.ArbeitszeitTagGeaendert == null)
          auswTag.ArbeitszeitTagGeaendert = WertManuellGeaendert;
      }

      Db.SaveChanges();
    }

    private TimeSpan DatumInZeit(DateTime? Datum)
    {
      return new TimeSpan(Datum.Value.Hour, Datum.Value.Minute, 0);
    }

    private void WertManuellGeaendert(tabArbeitszeitTag AuswertungTag, string PropertyName)
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
        BerechneUrlaub(ListeTage);
        BerechneUeberstundenAusTagen(ListeTage);
      }
      else if (PropertyName == "Krank")
      {
        BerechneKrank(ListeTage);
        BerechneUeberstundenAusTagen(ListeTage);
      }
      else if (PropertyName == "Feiertag")
        BerechneFeiertag(ListeTage);
      else if (PropertyName == "Nachtschicht")
        BerechneNachtschicht(ListeTage);

      DbSichern.AbgleichEintragen(AuswertungTag.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      DbSichern.AbgleichEintragen(_Bediener.eArbeitszeitHelper.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      _Db.SaveChanges();
    }

    private void BerechneNachtschicht(IEnumerable<tabArbeitszeitTag> listeTage)
    {
      var sumNachtschicht = new TimeSpan(listeTage.Sum(c => c.Nachtschicht.Ticks));
      _Bediener.eArbeitszeitHelper.NachtschichtenAnzeige = ZeitHelper.ZeitInString(sumNachtschicht);
    }

    private void BerechneFeiertag(IEnumerable<tabArbeitszeitTag> listeTage)
    {
      var sumZeit = new TimeSpan(listeTage.Sum(c => c.Feiertag.Ticks));
      _Bediener.eArbeitszeitHelper.FeiertageAnzeige = ZeitHelper.ZeitInString(sumZeit);
    }

    private void BerechneUeberstundenBezahlt()
    {
      var ueberstundenBezahlt = ZeitHelper.ZeitStringAddieren(AuswertungKumulativ.UeberstundenBezahltString, _Bediener.eArbeitszeitHelper.AuszahlungUeberstunden);
      AuswertungGesamt.UeberstundenBezahltString = ZeitHelper.ZeitInString(ueberstundenBezahlt);
    }

    private void BerechneKrank(IEnumerable<tabArbeitszeitTag> listeTage)
    {
      var krank = Convert.ToInt16(listeTage.Count(c => c.Krank));
      _Bediener.eArbeitszeitHelper.KrankAnzeige = krank;
      AuswertungGesamt.KrankAnzeige = (short)(krank + AuswertungKumulativ.fKrank);
    }

    private void BerechneUrlaub(IEnumerable<tabArbeitszeitTag> listeTage)
    {
      var urlaub = Convert.ToByte(listeTage.Count(c => c.Urlaub));
      _Bediener.eArbeitszeitHelper.UrlaubAnzeige = urlaub;

      AuswertungGesamt.UrlaubAnzeige = (byte)(urlaub + AuswertungKumulativ.fUrlaub);
      AuswertungKumulativ.UrlaubOffenAnzeige = (short)(_Bediener.Urlaubstage + AuswertungKumulativ.fRestUrlaub - urlaub - AuswertungKumulativ.fUrlaub);
    }

    private void BerechneUeberstundenAusTagen(IEnumerable<tabArbeitszeitTag> listeTage)
    {
      var sollStunden = Helper.StringInZeit(_Bediener.eArbeitszeitHelper.SollStunden);
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

      _Bediener.eArbeitszeitHelper.UeberstundenAnzeige = ZeitHelper.ZeitInString(Helper.StringInZeit(_Bediener.eArbeitszeitHelper.SollStunden) - istStunden);

      BerechneUeberstundenGesamt();
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

    public void SetSollstunden(string Sollstunden)
    {
      var zeit = new ZeitHelper(Sollstunden, false);
      if (zeit.IstOk)
      {
        _Bediener.eArbeitszeitHelper.SollstundenAnzeige = zeit.AsString;
        _Bediener.eArbeitszeitHelper.Ueberstunden = ZeitHelper.ZeitInString(zeit.AsTime - AuswertungKumulativ.fIstStunden);
        BerechneUeberstundenGesamt();

        DbSichern.AbgleichEintragen(_Bediener.eArbeitszeitHelper.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        _Db.SaveChanges();
      }
    }

    public void SetUebestundenAuszahlung(string UeberstundenAuszahlung)
    {
      var zeit = new ZeitHelper(UeberstundenAuszahlung, false);
      if (zeit.IstOk)
      {
        _Bediener.eArbeitszeitHelper.AuszahlungUeberstundenAnzeige = zeit.AsString;
        AuswertungGesamt.UeberstundenBezahltString = ZeitHelper.ZeitInString(AuswertungKumulativ.fUeberstundenBezahlt + zeit.AsTime);
        BerechneUeberstundenGesamt();

        _Db.SaveChanges();
      }
    }
  }

  public class ArbeitszeitSummen : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public TimeSpan fIstStunden = TimeSpan.Zero;
    public string IstStundenString
    {
      get { return Helper.ZeitInString(fIstStunden); }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          fIstStunden = zeit.AsTime;
          NotifyPropertyChanged();
        }
      }
    }

    public TimeSpan fUeberstunden = TimeSpan.Zero;
    public string UeberstundenString
    {
      get { return Helper.ZeitInString(fUeberstunden); }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          fUeberstunden = zeit.AsTime;
          NotifyPropertyChanged();
          NotifyPropertyChanged("Ueberstunden");
        }
      }
    }

    public TimeSpan fUeberstundenVorjahr = TimeSpan.Zero;
    public string UeberstundenVorjahrString
    {
      get { return Helper.ZeitInString(fUeberstundenVorjahr); }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this.fUeberstundenVorjahr = zeit.AsTime;
          NotifyPropertyChanged();
        }
      }
    }

    public TimeSpan fUeberstundenBezahlt = TimeSpan.Zero;
    public string UeberstundenBezahltString
    {
      get { return Helper.ZeitInString(fUeberstundenBezahlt); }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this.fUeberstundenBezahlt = zeit.AsTime;
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

    public short fRestUrlaub = 0;
    public short RestUrlaubAnzeige
    {
      get { return this.fRestUrlaub; }
      set
      {
        if (value != this.fRestUrlaub)
        {
          this.fRestUrlaub = value;
          NotifyPropertyChanged();
        }
      }
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

    public short fUrlaubVorjahr = 0;
    public short UrlaubVorjahrAnzeige
    {
      get { return fUrlaubVorjahr; }
      set
      {
        if (value != fUrlaubVorjahr)
        {
          fUrlaubVorjahr = value;
          NotifyPropertyChanged();
        }
      }
    }
  }
}
