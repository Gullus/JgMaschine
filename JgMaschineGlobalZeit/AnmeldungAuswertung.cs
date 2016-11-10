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

    public string ZeitInString(TimeSpan Zeit)
    {
      return ((int)Zeit.TotalHours).ToString("D2") + ":" + Zeit.Minutes.ToString();
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

    private void AuswertungInit(tabArbeitszeitAuswertung AuswertungNeu)
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

      // Kumulative Werte für Auswertung erfassen
      var auswAlle = _Db.tabArbeitszeitAuswertungSet.Where(w => (w.fBediener == _AktAuswertung.fBediener) && (w.Jahr == _Jahr) && (w.Monat < _Monat)).ToList();
      var ersterAuswertung = auswAlle.FirstOrDefault(w => w.Monat == 0);
      var auswKumulativ = auswAlle.Where(w => (w.Monat > 0)).ToList();

      var zeitUeberstundenKumulativ = new TimeSpan(auswKumulativ.Sum(s => StringInZeit(s.Ueberstunden).Ticks));
      var zeitUeberstundenBezahlt = new TimeSpan(auswKumulativ.Sum(s => StringInZeit(s.AuszahlungUeberstunden).Ticks));
      var ueberstundenVorjahr = StringInZeit((ersterAuswertung == null) ? "00:00" : ersterAuswertung.Ueberstunden);
      var sollStunden = StringInZeit(_AktAuswertung.SollStunden);
      var ueberStunden = StringInZeit(_AktAuswertung.Ueberstunden);

      AuswertungKumulativ.Ueberstunden = ZeitInString(zeitUeberstundenKumulativ - zeitUeberstundenBezahlt); ;
      AuswertungMonat.SollStunden = _AktAuswertung.SollStunden;
      AuswertungMonat.UeberstundenVorjahr = ZeitInString(ueberstundenVorjahr); 
      AuswertungMonat.Ueberstunden = _AktAuswertung.Ueberstunden;

      AuswertungMonat.IstStunden = ZeitInString(sollStunden + ueberStunden);
      AuswertungGesamt.Ueberstunden = ZeitInString(ueberStunden + ueberstundenVorjahr - zeitUeberstundenKumulativ);

      AuswertungMonat.RestUrlaub = (ersterAuswertung == null) ? (byte)0 : ersterAuswertung.Urlaub;
      AuswertungKumulativ.Urlaub = (short)auswKumulativ.Where(w => w.Monat > 0).Sum(s => s.Urlaub);
      AuswertungMonat.UrlaubsTage = _Bediener.Urlaubstage;
      AuswertungMonat.Urlaub = _AktAuswertung.Urlaub;
      AuswertungMonat.UrlaubOffen = (short)(_Bediener.Urlaubstage + AuswertungMonat.RestUrlaub - AuswertungKumulativ.Urlaub - _AktAuswertung.Urlaub);
      AuswertungGesamt.Urlaub = (byte)(_AktAuswertung.Urlaub + AuswertungKumulativ.Urlaub);

      AuswertungKumulativ.Krank = Convert.ToInt16(auswKumulativ.Sum(s => s.Krank));
      AuswertungMonat.Krank = _AktAuswertung.Krank;
      AuswertungGesamt.Krank = (short)(_AktAuswertung.Krank + AuswertungKumulativ.Krank);

      AuswertungMonat.Feiertage = _AktAuswertung.Feiertage;
      AuswertungMonat.Krank = _AktAuswertung.Krank;

      AuswertungMonat.Nachtschichten = _AktAuswertung.Nachtschichten;
      AuswertungMonat.AuszahlungUeberstunden = _AktAuswertung.AuszahlungUeberstunden;
    }

    public void AuswertungBedienerTageErstellen(tabArbeitszeitAuswertung AuswertungNeu)
    {
      AuswertungInit(AuswertungNeu);

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
          //Pausenzeit Eintragen
          if (auswTag.DatenAbgleich.Status == EnumStatusDatenabgleich.Neu)
          {
            auswTag.Pause = new TimeSpan(1, 0, 0);
            var zeitAnmeldung = zeiten.Min(z => z.Anmeldung);
            var zeitAuswahl = new TimeSpan(zeitAnmeldung.Hour, zeitAnmeldung.Minute, 0);
            var dsPause = ListePausenzeiten.FirstOrDefault(w => (zeitAuswahl >= w.ZeitVon) && (zeitAuswahl <= w.ZeitBis));
            if (dsPause != null)
              auswTag.Pause = dsPause.Pausenzeit;
          }

          foreach (var zeit in zeiten)
          {
            // Kontrolle ob Zeiten an Tagesauswertung hängt
            if (zeit.eArbeitszeitAuswertung != auswTag)
              zeit.eArbeitszeitAuswertung = auswTag;

            auswTag.ZeitBerechnet += zeit.Dauer;
            auswTag.NachtschichtBerechnet += NachtSchichtBerechnen(22, 0, 8, 0, zeit.Anmeldung, (DateTime)zeit.Abmeldung);
          }

          auswTag.ZeitBerechnet -= auswTag.Pause;

          if (auswTag.DatenAbgleich.Status == EnumStatusDatenabgleich.Neu)
          {
            auswTag.Zeit = auswTag.ZeitBerechnet;
            auswTag.Nachtschicht = auswTag.NachtschichtBerechnet;
          }
        }

        listeAnzeigeTage.Add(auswTag);
      }

      foreach (var auswTag in listeAnzeigeTage)
      {
        if (auswTag.ArbeitszeitTagGeaendert == null)
          auswTag.ArbeitszeitTagGeaendert = WertTagGeaendert;
      }

      _Db.SaveChanges();
      _VsAuswertungTage.Source = listeAnzeigeTage;
    }

    private void WertTagGeaendert(tabArbeitszeitTag Sender, string PropertyName)
    {
      var listeTage = (ObservableCollection<tabArbeitszeitTag>)_VsAuswertungTage.Source;

      if ((PropertyName == "Zeit") || (PropertyName == "Pause"))
      {
        var zz = new ZeitHelper(_AktAuswertung.SollStunden, false);
        var sollStunden = zz.AsTime;
        var sumZeit = new TimeSpan(listeTage.Sum(c => c.Zeit.Ticks));
        var sumPause = new TimeSpan(listeTage.Sum(c => c.Pause.Ticks));

        AuswertungMonat.IstStunden = ZeitInString(sumZeit - sumPause);

        var ueberStunden = sumZeit - sollStunden - sumPause;
        AuswertungMonat.Ueberstunden = ZeitInString(ueberStunden);

        zz = new ZeitHelper(AuswertungKumulativ.Ueberstunden, false);
        AuswertungGesamt.Ueberstunden = ZeitInString(zz.AsTime + ueberStunden);

        _AktAuswertung.Ueberstunden = AuswertungMonat.Ueberstunden;

      }

      else if (PropertyName == "Urlaub")
      {
        var urlaub = Convert.ToByte(listeTage.Count(c => c.Urlaub));
        AuswertungMonat.Urlaub = urlaub;
        AuswertungGesamt.Urlaub = (byte)(urlaub + AuswertungKumulativ.Urlaub);

        _AktAuswertung.Urlaub = urlaub;
        AuswertungMonat.UrlaubOffen = (short)(_Bediener.Urlaubstage + AuswertungMonat.RestUrlaub - AuswertungKumulativ.Urlaub - _AktAuswertung.Urlaub);
      }

      else if (PropertyName == "Krank")
      {
        var krank = Convert.ToInt16(listeTage.Count(c => c.Krank));
        AuswertungMonat.Krank = krank;
        AuswertungGesamt.Krank = (short)(krank + AuswertungKumulativ.Krank);

        _AktAuswertung.Krank = krank;
      }

      else if (PropertyName == "Feiertag")
      {
        var sumZeit = new TimeSpan(listeTage.Sum(c => c.Feiertag.Ticks));
        AuswertungMonat.Feiertage = ZeitInString(sumZeit);
        _AktAuswertung.Feiertage = AuswertungMonat.Feiertage;
      }

      else if (PropertyName == "Nachtschicht")
      {
        var sumNachtschicht = new TimeSpan(listeTage.Sum(c => c.Nachtschicht.Ticks));
        AuswertungMonat.Nachtschichten = ZeitInString(sumNachtschicht);
        _AktAuswertung.Nachtschichten = AuswertungMonat.Nachtschichten;
      }

      DbSichern.AbgleichEintragen(Sender.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      DbSichern.AbgleichEintragen(_AktAuswertung.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      _Db.SaveChanges();
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
        AuswertungMonat.SollStunden = zeit.AsString;
        _AktAuswertung.SollStunden = zeit.AsString;
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

    private string _SollStunden = "00:00";
    public string SollStunden
    {
      get { return this._SollStunden; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._SollStunden = zeit.AsString;
          NotifyPropertyChanged();
        }
      }
    }

    private string _IstStunden = "00:00";
    public string IstStunden
    {
      get { return this._IstStunden; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._IstStunden = zeit.AsString;
          NotifyPropertyChanged();
        }
      }
    }

    private string _Nachtschichten = "00:00";
    public string Nachtschichten
    {
      get { return this._Nachtschichten; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._Nachtschichten = zeit.AsString;
          NotifyPropertyChanged();
        }
      }
    }

    private string _Ueberstunden = "00:00";
    public string Ueberstunden
    {
      get { return this._Ueberstunden; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._Ueberstunden = zeit.AsString;
          NotifyPropertyChanged();
        }
      }
    }

    private string _UeberstundenVorjahr = "00:00";
    public string UeberstundenVorjahr
    {
      get { return this._UeberstundenVorjahr; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._UeberstundenVorjahr = zeit.AsString;
          NotifyPropertyChanged();
        }
      }
    }

    private string _UeberstundenBezahlen = "00:00";
    public string AuszahlungUeberstunden
    {
      get { return this._UeberstundenBezahlen; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._UeberstundenBezahlen = zeit.AsString;
          NotifyPropertyChanged();
        }
      }
    }

    private string _Feiertage = "00:00";
    public string Feiertage
    {
      get { return this._Feiertage; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this._Feiertage = zeit.AsString;
          NotifyPropertyChanged();
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
