using System;
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

    private byte _Monat = 1;
    private Int16 _Jahr = 2000;

    private tabArbeitszeitAuswertung _AktAuswertung = null;

    public DateTime MonatErster
    {
      get { return new DateTime(_Jahr, _Monat, 1); }
    }
    public DateTime MonatLetzter
    {
      get { return new DateTime(_Jahr, _Monat, DateTime.DaysInMonth(_Jahr, _Monat), 23, 59, 59); }
    }

    public Guid[] IdisBediener;
    private CollectionViewSource _VsBediener;
    private tabBediener _Bediener { get { return (tabBediener)_VsBediener.View.CurrentItem; } }

    public ArbeitszeitAuswertungDs AuswertungGesamt = null;
    public ArbeitszeitAuswertungDs AuswertungKumulativ = null;
    public ArbeitszeitAuswertungDs AuswertungMonat = null;

    private CollectionViewSource _VsAuswertungTage;

    private ComboBox _CmbJahr;
    private ComboBox _CmbMonat;

    private List<tabArbeitszeitAuswertung> _ListeAuswertungen = new List<tabArbeitszeitAuswertung>();

    private TimeSpan _SollStunden = TimeSpan.Zero;
    public short Jahr
    {
      get { return _Jahr; }
      set
      {
        _Jahr = value;
        JahrGeandert();
        MonatGeandert();
      }
    }
    public byte Monat
    {
      get { return _Monat; }
      set
      {
        _Monat = value;
        MonatGeandert();
      }
    }

    public ObservableCollection<tabPausenzeit> ListePausenzeiten;
    public ObservableCollection<tabSollStunden> ListeSollStunden;
    public ObservableCollection<tabFeiertage> ListeFeiertage;
    public ObservableCollection<tabBediener> ListeBediener;

    public AnmeldungAuswertung(JgModelContainer Db, ComboBox CmbJahr, ComboBox CmbMonat,
      CollectionViewSource VsBediener, CollectionViewSource VsAuswertungTage)
    {
      _Db = Db;
      var heute = DateTime.Now.Date;
      _Jahr = (Int16)heute.Year;
      _Monat = (byte)heute.Month;

      _CmbJahr = CmbJahr;
      var jahrStart = (Int16)(heute.Year - 10);
      for (Int16 i = jahrStart; i < jahrStart + 12; i++)
        _CmbJahr.Items.Add(i);
      _CmbJahr.SelectedItem = _Jahr;
      _CmbJahr.SelectionChanged += Cmb_SelectionChangedJahr;

      _CmbMonat = CmbMonat;
      for (int i = 1; i < 13; i++)
        _CmbMonat.Items.Add((new DateTime(2000, i, 1)).ToString("MMMM"));
      _CmbMonat.SelectedIndex = _Monat - 1;
      _CmbMonat.SelectionChanged += Cmb_SelectionChangedMonat;

      var bediener = _Db.tabBedienerSet.Where(w => (w.Status == EnumStatusBediener.Aktiv)).OrderBy(o => o.NachName).ToList();
      ListeBediener = new ObservableCollection<tabBediener>(bediener);
      IdisBediener = ListeBediener.Select(s => s.Id).ToArray();
      _VsBediener = VsBediener;
      _VsBediener.Source = ListeBediener;
      _VsBediener.View.CurrentChanged += (sen, erg) =>
      {
        BedienerGeandert();
      };

      _VsAuswertungTage = VsAuswertungTage;
      JahrGeandert();
      MonatGeandert();
    }

    private void BedienerGeandert()
    {
      var aktAuswertung = _ListeAuswertungen.FirstOrDefault(f => f.eBediener == _Bediener);
      AuswertungBedienerTageErstellen(aktAuswertung);
    }

    private void Cmb_SelectionChangedJahr(object sender, SelectionChangedEventArgs e)
    {
      if ((sender as ComboBox).IsKeyboardFocusWithin)
      {
        JahrGeandert();
        MonatGeandert();
      }
    }

    private void Cmb_SelectionChangedMonat(object sender, SelectionChangedEventArgs e)
    {
      if ((sender as ComboBox).IsKeyboardFocusWithin)
        MonatGeandert();
    }

    public void JahrGeandert()
    {
      ListePausenzeiten = new ObservableCollection<tabPausenzeit>(_Db.tabPausenzeitSet.ToList());

      var von = new DateTime(_Jahr, 1, 1);
      var bis = new DateTime(_Jahr, 12, 31, 23, 59, 59);
      var feiertage = _Db.tabFeiertageSet.Where(w => (!w.DatenAbgleich.Geloescht) && (w.Datum >= von) && (w.Datum <= bis)).ToList();
      ListeFeiertage = new ObservableCollection<tabFeiertage>(feiertage);

      var sollStunden = _Db.tabSollStundenSet.Where(w => (w.Jahr == _Jahr)).ToList();
      ListeSollStunden = new ObservableCollection<tabSollStunden>(sollStunden);
    }

    public void MonatGeandert()
    {
      _Monat = (byte)(_CmbMonat.SelectedIndex + 1);
      _Jahr = Convert.ToInt16(_CmbJahr.SelectedItem);

      var dsSollstunde = ListeSollStunden.FirstOrDefault(f => f.Monat == _Monat);
      _SollStunden = (dsSollstunde == null) ? TimeSpan.Zero : StringInZeit(dsSollstunde.SollStunden);

      _ListeAuswertungen = _Db.tabArbeitszeitAuswertungSet.Where(w => (IdisBediener.Contains(w.fBediener) && (w.Jahr == _Jahr) && (w.Monat == _Monat))).ToList();

      foreach (var bediener in ListeBediener)
      {
        var ausw = _ListeAuswertungen.FirstOrDefault(f => f.eBediener == bediener);
        if (ausw == null)
          bediener.StatusArbeitszeit = EnumStatusArbeitszeitAuswertung.Leer;
        else
          bediener.StatusArbeitszeit = ausw.Status;
      }
    }

    public TimeSpan StringInZeit(string Zeit)
    {
      var erg = TimeSpan.Zero;
      if (!string.IsNullOrWhiteSpace(Zeit))
      {
        try
        {
          var werte = Zeit.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
          var stunde = Convert.ToInt32(werte[0]);
          var minute = Convert.ToInt32(werte[1]);
          return new TimeSpan(stunde, stunde < 0 ? -1 * minute : minute, 0);
        }
        catch { }
      }
      return erg;
    }

    public void BerechneUeberstundenGesamt()
    {
      AuswertungGesamt.Ueberstunden = AuswertungMonat.UeberstundenVorjahr + AuswertungKumulativ.Ueberstunden - AuswertungGesamt.UeberstundenBezahlt + AuswertungMonat.Ueberstunden;
    }

    private void AuswertungInit(tabArbeitszeitAuswertung AuswertungNeu, ObservableCollection<tabArbeitszeitTag> ListeTage)
    {
      // Kumulative Werte für Auswertung erfassen
      var auswAlle = _Db.tabArbeitszeitAuswertungSet.Where(w => (w.fBediener == _AktAuswertung.fBediener) && (w.Jahr == _Jahr) && (w.Monat < _Monat)).ToList();
      var ersterAuswertung = auswAlle.FirstOrDefault(w => w.Monat == 0);
      var auswKumulativ = auswAlle.Where(w => (w.Monat > 0)).ToList();

      AuswertungMonat.SollStundenString = _AktAuswertung.SollStunden;

      BerechneUeberstunden(ListeTage);
      AuswertungMonat.IstStunden = AuswertungMonat.SollStunden + AuswertungMonat.Ueberstunden;

      AuswertungKumulativ.UeberstundenBezahlt = new TimeSpan(auswKumulativ.Sum(s => StringInZeit(s.AuszahlungUeberstunden).Ticks));
      AuswertungMonat.UeberstundenBezahltString = _AktAuswertung.AuszahlungUeberstunden;
      AuswertungGesamt.UeberstundenBezahlt = AuswertungKumulativ.UeberstundenBezahlt + AuswertungMonat.UeberstundenBezahlt;

      AuswertungKumulativ.Ueberstunden = new TimeSpan(auswKumulativ.Sum(s => StringInZeit(s.Ueberstunden).Ticks));
      AuswertungMonat.UeberstundenVorjahrString = (ersterAuswertung == null) ? "00:00" : ersterAuswertung.Ueberstunden;
      BerechneUeberstundenGesamt();

      BerechneNachtschicht(ListeTage);
      BerechneFeiertag(ListeTage);

      AuswertungKumulativ.Urlaub = Convert.ToInt16(auswKumulativ.Sum(s => s.Urlaub));
      AuswertungMonat.RestUrlaub = (ersterAuswertung == null) ? (byte)0 : ersterAuswertung.Urlaub;
      AuswertungMonat.UrlaubsTage = _Bediener.Urlaubstage;
      BerechneUrlaub(ListeTage);
      AuswertungMonat.UrlaubOffen = (short)(_Bediener.Urlaubstage + AuswertungMonat.RestUrlaub - AuswertungKumulativ.Urlaub - _AktAuswertung.Urlaub);

      AuswertungGesamt.Urlaub = (byte)(_AktAuswertung.Urlaub + AuswertungKumulativ.Urlaub);

      AuswertungKumulativ.Krank = Convert.ToInt16(auswKumulativ.Sum(s => s.Krank));
      BerechneKrank(ListeTage);
      AuswertungGesamt.Krank = (short)(_AktAuswertung.Krank + AuswertungKumulativ.Krank);
    }
    
    public bool Kontrolle24StundenOK(TimeSpan Zeit)
    {
      return (Zeit >= TimeSpan.Zero) && (Zeit < new TimeSpan(24, 0, 0));
    }

    public void AuswertungBedienerTageErstellen(tabArbeitszeitAuswertung AuswertungNeu)
    {
      _AktAuswertung = AuswertungNeu;
      if (_AktAuswertung == null)
      {
        var dsSollStunden = _Db.tabSollStundenSet.FirstOrDefault(f => (f.Jahr == _Jahr) && (f.Monat == _Monat));

        _AktAuswertung = new tabArbeitszeitAuswertung()
        {
          Id = Guid.NewGuid(),
          eBediener = _Bediener,
          Jahr = _Jahr,
          Monat = _Monat,
          Urlaub = 0,
          SollStunden = dsSollStunden?.SollStunden,
          Status = EnumStatusArbeitszeitAuswertung.InArbeit,
        };
        DbSichern.AbgleichEintragen(_AktAuswertung.DatenAbgleich, EnumStatusDatenabgleich.Neu);
        _Db.tabArbeitszeitAuswertungSet.Add(_AktAuswertung);
        _Db.SaveChanges();
        _ListeAuswertungen.Add(_AktAuswertung);

        _AktAuswertung.eBediener.StatusArbeitszeit = EnumStatusArbeitszeitAuswertung.InArbeit;
      }

      // Werte für Tage berechnen
      var auswTage = _Db.tabArbeitszeitTagSet.Where(w => w.fArbeitszeitAuswertung == _AktAuswertung.Id).ToList();
      var anzTage = DateTime.DaysInMonth(_Jahr, _Monat);
      var listeAnzeigeTage = new ObservableCollection<tabArbeitszeitTag>();

      var alleZeiten = _Db.tabArbeitszeitSet.Where(w => (w.fBediener == _Bediener.Id) && (w.Anmeldung >= MonatErster) && (w.Anmeldung < MonatLetzter)).ToList();
      for (byte tag = 1; tag <= anzTage; tag++)
      {
        var auswTag = auswTage.FirstOrDefault(f => f.Tag == tag);
        if (auswTag == null)
        {
          auswTag = new tabArbeitszeitTag()
          {
            Id = Guid.NewGuid(),
            fArbeitszeitAuswertung = _AktAuswertung.Id,
            Tag = tag
          };
          DbSichern.AbgleichEintragen(auswTag.DatenAbgleich, EnumStatusDatenabgleich.Neu);
          _Db.tabArbeitszeitTagSet.Add(auswTag);
        }

        var aktDatum = new DateTime(_Jahr, _Monat, tag);
        auswTag.Wochentag = aktDatum.ToString("ddd");

        auswTag.IstSonnabend = aktDatum.DayOfWeek == DayOfWeek.Saturday;
        auswTag.IstSonntag = aktDatum.DayOfWeek == DayOfWeek.Sunday;
        auswTag.IstFeiertag = ListeFeiertage.FirstOrDefault(f => f.Datum == aktDatum) != null;

        var zeiten = alleZeiten.Where(w => w.Anmeldung.Day == tag).ToList();

        if (zeiten.Count > 0)
        {
          auswTag.ZeitBerechnet = TimeSpan.Zero;
          auswTag.NachtschichtBerechnet = TimeSpan.Zero;

          foreach (var zeit in zeiten)
          {
            // Kontrolle ob Zeiten an Tagesauswertung hängt
            if (zeit.eArbeitszeitAuswertung != auswTag)
              zeit.eArbeitszeitAuswertung = auswTag;

            auswTag.ZeitBerechnet += zeit.Dauer;
            if (zeit.Abmeldung == null)
              auswTag.NachtschichtBerechnet = TimeSpan.Zero;
            else
              auswTag.NachtschichtBerechnet += NachtSchichtBerechnen(22, 0, 8, 0, zeit.Anmeldung, zeit.Abmeldung.Value);
          }
          auswTag.ZeitBerechnet = ZeitAufMinuteRunden(auswTag.ZeitBerechnet);
          auswTag.NachtschichtBerechnet = ZeitAufMinuteRunden(auswTag.NachtschichtBerechnet);

          if (auswTag.DatenAbgleich.Status == EnumStatusDatenabgleich.Neu)
          {
            auswTag.Pause = new TimeSpan(1, 0, 0);
            if (auswTag.ZeitBerechnet < new TimeSpan(4, 0, 0))
              auswTag.Pause = TimeSpan.Zero;
            else
            {
              var zeitAnmeldung = zeiten.Min(z => z.Anmeldung);
              var zeitAuswahl = new TimeSpan(zeitAnmeldung.Hour, zeitAnmeldung.Minute, 0);
              var dsPause = ListePausenzeiten.FirstOrDefault(w => (zeitAuswahl >= w.ZeitVon) && (zeitAuswahl <= w.ZeitBis));
              if (dsPause != null)
                auswTag.Pause = dsPause.Pausenzeit;
            }

            auswTag.IstFehlerZeit = false;
            auswTag.Zeit = auswTag.ZeitBerechnet - auswTag.Pause;
            if (!Kontrolle24StundenOK(auswTag.Zeit))
            {
              auswTag.IstFehlerZeit = true;
              auswTag.Zeit = TimeSpan.Zero;
            }

            auswTag.Nachtschicht = auswTag.NachtschichtBerechnet;
            auswTag.IstFehlerNachtschicht = false;
            if (!Kontrolle24StundenOK(auswTag.Nachtschicht))
            {
              auswTag.IstFehlerNachtschicht = true;
              auswTag.Nachtschicht = TimeSpan.Zero;
            }
          }
    
          auswTag.ZeitBerechnet -= auswTag.Pause;
        }

        listeAnzeigeTage.Add(auswTag);
      }

      foreach (var auswTag in listeAnzeigeTage)
      {
        if (auswTag.ArbeitszeitTagGeaendert == null)
          auswTag.ArbeitszeitTagGeaendert = WertManuellGeaendert;
      }

      AuswertungInit(AuswertungNeu, listeAnzeigeTage);

      _Db.SaveChanges();
      _VsAuswertungTage.Source = listeAnzeigeTage;
    }



    private void WertManuellGeaendert(tabArbeitszeitTag Sender, string PropertyName)
    {
      var listeTage = (ObservableCollection<tabArbeitszeitTag>)_VsAuswertungTage.Source;

      if ((PropertyName == "Zeit") || (PropertyName == "Pause"))
        BerechneUeberstunden(listeTage);
      else if (PropertyName == "Urlaub")
        BerechneUrlaub(listeTage);
      else if (PropertyName == "Krank")
        BerechneKrank(listeTage);
      else if (PropertyName == "Feiertag")
        BerechneFeiertag(listeTage);
      else if (PropertyName == "Nachtschicht")
        BerechneNachtschicht(listeTage);

      DbSichern.AbgleichEintragen(Sender.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      DbSichern.AbgleichEintragen(_AktAuswertung.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      _Db.SaveChanges();
    }

    private void BerechneNachtschicht(ObservableCollection<tabArbeitszeitTag> listeTage)
    {
      AuswertungMonat.Nachtschichten = new TimeSpan(listeTage.Sum(c => c.Nachtschicht.Ticks));
      _AktAuswertung.Nachtschichten = AuswertungMonat.NachtschichtenString;
    }

    private void BerechneFeiertag(ObservableCollection<tabArbeitszeitTag> listeTage)
    {
      var sumZeit = new TimeSpan(listeTage.Sum(c => c.Feiertag.Ticks));
      AuswertungMonat.Feiertage = sumZeit;
      _AktAuswertung.Feiertage = AuswertungMonat.FeiertageString;

      BerechneUeberstunden(listeTage);
    }

    private void BerechneKrank(ObservableCollection<tabArbeitszeitTag> listeTage)
    {
      var krank = Convert.ToInt16(listeTage.Count(c => c.Krank));
      AuswertungMonat.Krank = krank;
      AuswertungGesamt.Krank = (short)(krank + AuswertungKumulativ.Krank);
      _AktAuswertung.Krank = krank;

      BerechneUeberstunden(listeTage);
    }

    private void BerechneUrlaub(ObservableCollection<tabArbeitszeitTag> listeTage)
    {
      var urlaub = Convert.ToByte(listeTage.Count(c => c.Urlaub));
      AuswertungMonat.Urlaub = urlaub;
      AuswertungGesamt.Urlaub = (byte)(urlaub + AuswertungKumulativ.Urlaub);
      _AktAuswertung.Urlaub = urlaub;
      AuswertungMonat.UrlaubOffen = (short)(_Bediener.Urlaubstage + AuswertungMonat.RestUrlaub - AuswertungKumulativ.Urlaub - _AktAuswertung.Urlaub);

      BerechneUeberstunden(listeTage);
    }

    private void BerechneUeberstunden(ObservableCollection<tabArbeitszeitTag> listeTage)
    {
      var sollStunden = StringInZeit(_AktAuswertung.SollStunden);
      var sumZeit = TimeSpan.Zero;
      var hinzuTage = 0;
      foreach(var tag in listeTage)
      {
        sumZeit += tag.Zeit - tag.Pause;
        if (tag.IstFeiertag && !tag.IstSonnabend && !tag.IstSonntag)
          ++hinzuTage;
        else if (tag.Urlaub)
          ++hinzuTage;
        else if (tag.Krank)
          ++hinzuTage; 
      }

      AuswertungMonat.IstStunden = new TimeSpan(8 * hinzuTage, 0, 0) + ZeitAufMinuteRunden(sumZeit);
      var ueberStunden = AuswertungMonat.IstStunden - sollStunden;
      AuswertungMonat.Ueberstunden = ueberStunden;
      BerechneUeberstundenGesamt();

      _AktAuswertung.Ueberstunden = AuswertungMonat.ZeitInString(ueberStunden);
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
        AuswertungMonat.SollStunden = zeit.AsTime;
        AuswertungMonat.Ueberstunden = AuswertungMonat.IstStunden - AuswertungMonat.SollStunden;
        BerechneUeberstundenGesamt();

        _AktAuswertung.SollStunden = zeit.AsString;
        _AktAuswertung.Ueberstunden = AuswertungMonat.UeberstundenString;
        DbSichern.AbgleichEintragen(_AktAuswertung.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        _Db.SaveChanges();
      }
    }

    public void SetUebestundenAuszahlung(string UeberstundenAuszahlung)
    {
      var zeit = new ZeitHelper(UeberstundenAuszahlung, false);
      if (zeit.IstOk)
      {
        AuswertungMonat.UeberstundenBezahlt = zeit.AsTime;
        AuswertungGesamt.UeberstundenBezahlt = AuswertungKumulativ.UeberstundenBezahlt + AuswertungMonat.UeberstundenBezahlt;
        BerechneUeberstundenGesamt();

        _AktAuswertung.AuszahlungUeberstunden = zeit.AsString;
        DbSichern.AbgleichEintragen(_AktAuswertung.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        _Db.SaveChanges();
      }
    }
  }

  public class ArbeitszeitAuswertungDs : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string ZeitInString(TimeSpan Zeit)
    {
      return ((int)Zeit.TotalHours).ToString("D2") + ":" + ((Zeit.Minutes < 0) ? -1 *  Zeit.Minutes : Zeit.Minutes).ToString("D2");
    }

    private TimeSpan _SollStunden = TimeSpan.Zero;
    public TimeSpan SollStunden
    {
      get { return _SollStunden; }
      set
      {
        if (value != _SollStunden)
        {
          _SollStunden = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("SollStundenString");
        }
      }
    }

    public string SollStundenString
    {
      get { return ZeitInString(_SollStunden); }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          _SollStunden = zeit.AsTime;
          NotifyPropertyChanged();
          NotifyPropertyChanged("SollStunden");
        }
      }
    }

    private TimeSpan _IstStunden = TimeSpan.Zero;
    public TimeSpan IstStunden
    {
      get { return _IstStunden; }
      set
      {
        if (value != _IstStunden)
        {
          _IstStunden = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("IstStundenString");
        }
      }
    }
    public string IstStundenString
    {
      get { return ZeitInString(this._IstStunden); }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._IstStunden = zeit.AsTime;
          NotifyPropertyChanged();
          NotifyPropertyChanged("IstStunden");
        }
      }
    }

    private TimeSpan _Nachtschichten = TimeSpan.Zero;
    public TimeSpan Nachtschichten
    {
      get { return _Nachtschichten; }
      set
      {
        if (value != _Nachtschichten)
        {
          _Nachtschichten = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("NachtschichtenString");
        }
      }
    }

    public string NachtschichtenString
    {
      get { return ZeitInString(_Nachtschichten); }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._Nachtschichten = zeit.AsTime;
          NotifyPropertyChanged();
          NotifyPropertyChanged("Nachtschichten");
        }
      }
    }

    private TimeSpan _Ueberstunden = TimeSpan.Zero;
    public TimeSpan Ueberstunden
    {
      get { return _Ueberstunden; }
      set
      {
        if (value != _Ueberstunden)
        {
          _Ueberstunden = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("UeberstundenString");
        }
      }
    }

    public string UeberstundenString
    {
      get { return ZeitInString(_Ueberstunden); }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._Ueberstunden = zeit.AsTime;
          NotifyPropertyChanged();
          NotifyPropertyChanged("Ueberstunden");
        }
      }
    }

    private TimeSpan _UeberstundenVorjahr = TimeSpan.Zero;
    public TimeSpan UeberstundenVorjahr
    {
      get { return _UeberstundenVorjahr; }
      set
      {
        if (value != _UeberstundenVorjahr)
        {
          _UeberstundenVorjahr = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("UeberstundenVorjahrString");
        }
      }
    }
    public string UeberstundenVorjahrString
    {
      get { return ZeitInString(_UeberstundenVorjahr); }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._UeberstundenVorjahr = zeit.AsTime;
          NotifyPropertyChanged();
          NotifyPropertyChanged("UeberstundenVorjahr");
        }
      }
    }

    private TimeSpan _UeberstundenBezahlt = TimeSpan.Zero;
    public TimeSpan UeberstundenBezahlt
    {
      get { return _UeberstundenBezahlt; }
      set
      {
        if (value != _UeberstundenBezahlt)
        {
          _UeberstundenBezahlt = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("UeberstundenBezahltString");
        }
      }
    }

    public string UeberstundenBezahltString
    {
      get { return ZeitInString(_UeberstundenBezahlt); }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._UeberstundenBezahlt = zeit.AsTime;
          NotifyPropertyChanged();
          NotifyPropertyChanged("UeberstundenBezahlt");
        }
      }
    }

    private TimeSpan _Feiertage = TimeSpan.Zero;
    public TimeSpan Feiertage
    {
      get { return _Feiertage; }
      set
      {
        if (value != _Feiertage)
        {
          _Feiertage = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("FeiertageString");
        }
      }
    }
    public string FeiertageString
    {
      get { return ZeitInString(_Feiertage); }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._Feiertage = zeit.AsTime;
          NotifyPropertyChanged();
          NotifyPropertyChanged("Feiertage");
        }
      }
    }

    private short _UrlaubsTage = 0;
    public short UrlaubsTage
    {
      get { return this._UrlaubsTage; }
      set
      {
        if (value != this._UrlaubsTage)
        {
          this._UrlaubsTage = value;
          NotifyPropertyChanged();
        }
      }
    }

    private short _RestUrlaub = 0;
    public short RestUrlaub
    {
      get { return this._RestUrlaub; }
      set
      {
        if (value != this._RestUrlaub)
        {
          this._RestUrlaub = value;
          NotifyPropertyChanged();
        }
      }
    }

    private short _Urlaub = 0;
    public short Urlaub
    {
      get { return this._Urlaub; }
      set
      {
        if (value != this._Urlaub)
        {
          this._Urlaub = value;
          NotifyPropertyChanged();
        }
      }
    }

    private short _UrlaubOffen = 0;
    public short UrlaubOffen
    {
      get { return this._UrlaubOffen; }
      set
      {
        if (value != this._UrlaubOffen)
        {
          this._UrlaubOffen = value;
          NotifyPropertyChanged();
        }
      }
    }

    private short _Krank = 0;
    public short Krank
    {
      get { return this._Krank; }
      set
      {
        if (value != this._Krank)
        {
          this._Krank = value;
          NotifyPropertyChanged();
        }
      }
    }
  }
}
