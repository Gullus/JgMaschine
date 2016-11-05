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

    private tabSollStunden _DsSollStunden = null;

    public tabArbeitszeitAuswertung AuswertungGesamt = null;
    public tabArbeitszeitAuswertung AuswertungKumulativ = null;
    public tabArbeitszeitAuswertung AuswertungMonat = null;

    private CollectionViewSource _VsAuswertungTage;

    private ComboBox _CmbJahr;
    private ComboBox _CmbMonat;

    private List<tabArbeitszeitAuswertung> _ListeAuswertungen = new List<tabArbeitszeitAuswertung>();

    public short Jahr
    {
      get { return _Jahr; }
      set
      {
        _Jahr = value;
        MonatOderJahrGeandert();
      }
    }

    public byte Monat
    {
      get { return _Monat; }
      set
      {
        _Monat = value;
        MonatOderJahrGeandert();
      }
    }

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
      _CmbJahr.SelectionChanged += Cmb_SelectionChanged;

      _CmbMonat = CmbMonat;
      for (int i = 1; i < 13; i++)
        _CmbMonat.Items.Add((new DateTime(2000, i, 1)).ToString("MMMM"));
      _CmbMonat.SelectedIndex = _Monat - 1;
      _CmbMonat.SelectionChanged += Cmb_SelectionChanged;

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
      MonatOderJahrGeandert();
    }

    private void BedienerGeandert()
    {
      var aktAuswertung = _ListeAuswertungen.FirstOrDefault(f => f.eBediener == _Bediener);
      AuswertungBedienerTageErstellen(aktAuswertung);
    }

    private void Cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if ((sender as ComboBox).IsKeyboardFocusWithin)
        MonatOderJahrGeandert();
    }

    public void MonatOderJahrGeandert()
    {
      _Monat = (byte)(_CmbMonat.SelectedIndex + 1);
      _Jahr = Convert.ToInt16(_CmbJahr.SelectedItem);

      _DsSollStunden = _Db.tabSollStundenSet.FirstOrDefault(f => (f.Jahr == _Jahr) && (f.Monat == _Monat));

      var listeFeiertage = _Db.tabFeiertageSet.Where(w => (!w.DatenAbgleich.Geloescht) && (w.Datum >= MonatErster) && (w.Datum <= MonatLetzter)).OrderBy(o => o.Datum);
      ListeFeiertage = new ObservableCollection<tabFeiertage>(listeFeiertage.ToList());
      ListeFeiertage.CollectionChanged += (sen, erg) =>
      {
        switch (erg.Action)
        {
          case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
            foreach (var ds in erg.NewItems)
            {
              var feiertag = (tabFeiertage)ds;
              feiertag.Id = Guid.NewGuid();
              feiertag.Datum = DateTime.Now.Date;
              DbSichern.AbgleichEintragen(feiertag.DatenAbgleich, EnumStatusDatenabgleich.Neu);
              _Db.tabFeiertageSet.Add(feiertag);
            }
            _Db.SaveChanges();
            break;
          case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
            foreach (var ds in erg.OldItems)
            {
              var feiertag = (tabFeiertage)ds;
              feiertag.DatenAbgleich.Geloescht = true;
              DbSichern.AbgleichEintragen(feiertag.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
            }
            break;
        }
      };

      _ListeAuswertungen = _Db.tabArbeitszeitAuswertungSet.Where(w => (IdisBediener.Contains(w.fBediener) && (w.Jahr == _Jahr) && (w.Monat == _Monat))).ToList();

      var merkePos = _VsBediener.View.CurrentPosition;
      foreach (var bediener in ListeBediener)
      {
        var ausw = _ListeAuswertungen.FirstOrDefault(f => f.eBediener == bediener);
        if (ausw == null)
          bediener.StatusArbeitszeit = EnumStatusArbeitszeitAuswertung.Leer;
        else
          bediener.StatusArbeitszeit = ausw.Status;
      }
      _VsBediener.View.Refresh();
      _VsBediener.View.MoveCurrentToPosition(merkePos);
    }

    public string ZeitInString(TimeSpan Zeit)
    {
      return ((int)Zeit.TotalHours).ToString("D2") + ":" + Zeit.Minutes.ToString();
    }

    public void AuswertungBedienerTageErstellen(tabArbeitszeitAuswertung Auswertung)
    {
      if (Auswertung == null)
      {
        Auswertung = new tabArbeitszeitAuswertung()
        {
          Id = Guid.NewGuid(),
          eBediener = _Bediener,
          Jahr = _Jahr,
          Monat = _Monat,

          Urlaub = 0,
          Status = EnumStatusArbeitszeitAuswertung.Berechnet,
        };
        DbSichern.AbgleichEintragen(Auswertung.DatenAbgleich, EnumStatusDatenabgleich.Neu);
        _ListeAuswertungen.Add(Auswertung);
        _Db.tabArbeitszeitAuswertungSet.Add(Auswertung);
        _Db.SaveChanges();
      }


      var auswKumulativ = _Db.tabArbeitszeitAuswertungSet.Where(w => (w.fBediener == Auswertung.fBediener) && (w.Jahr == _Jahr) && (w.Monat < _Monat)).ToList();

      var ueberstunden = TimeSpan.Zero;
      foreach (var ausw in auswKumulativ)
      {
        var ds = ausw.Ueberstunden.Split(':');
        try
        {
          ueberstunden += new TimeSpan(Convert.ToInt32(ds[0]), Convert.ToInt32(ds[1]), 0);
        }
        catch { }
      }

      AuswertungKumulativ.UeberstundenAnzeige = ZeitInString(ueberstunden);
      AuswertungKumulativ.UrlaubAnzeige = Convert.ToInt16(auswKumulativ.Sum(s => s.Urlaub));
     
      var auswTage = _Db.tabArbeitszeitTagSet.Where(w => w.fArbeitszeitAuswertung == Auswertung.Id).ToList();
      var anzTage = DateTime.DaysInMonth(_Jahr, _Monat);
      var listeAnzeigeTage = new ObservableCollection<tabArbeitszeitTag>();

      var alleZeiten = _Db.tabArbeitszeitSet.Where(w => (w.fBediener == _Bediener.Id) && (w.Abmeldung != null) && (w.Anmeldung >= MonatErster) && (w.Anmeldung < MonatLetzter)).ToList();

      for (byte tag = 1; tag <= anzTage; tag++)
      {
        var auswTag = auswTage.FirstOrDefault(f => f.Tag == tag);
        

        if (auswTag == null)
        {
          auswTag = new tabArbeitszeitTag()
          {
            Id = Guid.NewGuid(),
            fArbeitszeitAuswertung = Auswertung.Id,
            Tag = tag
          };
          DbSichern.AbgleichEintragen(auswTag.DatenAbgleich, EnumStatusDatenabgleich.Neu);
          _Db.tabArbeitszeitTagSet.Add(auswTag);
        }
        auswTag.PropertyChanged += (sen, erg) =>
        {
          System.Windows.MessageBox.Show(erg.PropertyName);
        };

        listeAnzeigeTage.Add(auswTag);

        var aktDatum = new DateTime(_Jahr, _Monat, tag);
        auswTag.Wochentag = aktDatum.ToString("ddd");

        auswTag.IstSonnabend = aktDatum.DayOfWeek == DayOfWeek.Saturday;
        auswTag.IstSonntag = aktDatum.DayOfWeek == DayOfWeek.Sunday;
        auswTag.IstFeiertag = ListeFeiertage.FirstOrDefault(f => f.Datum == aktDatum) != null;

        var zeiten = alleZeiten.Where(w => w.Anmeldung.Day == tag).ToList();

        foreach (var zeit in zeiten)
        {
          // Kontrolle ob Zeiten an Tagesauswertung hängt
          if (zeit.eArbeitszeitAuswertung != auswTag)
            zeit.eArbeitszeitAuswertung = auswTag;

          auswTag.ZeitBerechnet += zeit.Dauer;
          auswTag.NachtschichtBerechnet += NachtZeitBerechnen(22, 0, 8, 0, zeit.Anmeldung, (DateTime)zeit.Abmeldung);
        }

        if (auswTag.DatenAbgleich.Status == EnumStatusDatenabgleich.Neu)
        {
          auswTag.Zeit = auswTag.ZeitBerechnet;
          auswTag.Nachtschicht = auswTag.NachtschichtBerechnet;
        }
      }

      _VsAuswertungTage.Source = listeAnzeigeTage;
      _Db.SaveChanges();
    }

    private void ListeAnzeigeTage_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      throw new NotImplementedException();
    }

    public static TimeSpan NachtZeitBerechnen(int NachtschichtStundeVon, int NachtschichtMinuteVon, int LaengeNachtschichtStunde, int LaengeNachtschichtMinute, DateTime DatumVon, DateTime DatumBis)
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
  }
}
