using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using JgMaschineData;
using JgMaschineLib;
using JgZeitHelper;

namespace JgMaschineGlobalZeit
{
  public class AnmeldungAuswertung
  {
    private JgModelContainer _Db;
    public JgModelContainer Db { get { return _Db; } }

    private ComboBox _CmbJahr;
    private ComboBox _CmbMonat;

    public JgEntityList<tabBediener> ListeBediener;

    // Wegen Optionen wird immer gleich das Ganze Jahr geladen

    public JgEntityList<tabPausenzeit> ListePausen;
    public JgEntityList<tabSollStunden> ListeSollstundenJahr;
    public JgEntityList<tabFeiertage> ListeFeiertageJahr;
    public JgEntityList<tabArbeitszeitRunden> ListeRundenJahr;

    public TimeSpan SollStundenMonat = TimeSpan.Zero;
    public List<tabFeiertage> ListeFeiertageMonat;
    public List<tabArbeitszeitRunden> ListeRundenMonat;

    public ArbeitszeitBediener AzBediener = null;
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

    public tabBediener AktuellerBediener { get { return ListeBediener.Current; } }

    public AnmeldungAuswertung(JgModelContainer Db, ComboBox CmbJahr, ComboBox CmbMonat, CollectionViewSource VsBediener, 
      ArbeitszeitBediener NeuAzBediener, CollectionViewSource VsAnzeigeTage)
    {
      _Db = Db;
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
      _CmbMonat.ItemsSource = Enum.GetValues(typeof(JgZeit.Monate));
      _CmbMonat.SelectedIndex = _CmbMonat.Items.IndexOf((JgZeit.Monate)heute.Month);

      _CmbMonat.SelectionChanged += (sen, erg) =>
      {
        if ((sen as ComboBox).IsKeyboardFocusWithin)
          MonatGeandert();
      };

      ListeBediener = new JgEntityList<tabBediener>(_Db)
      {
        Daten = _Db.tabBedienerSet.Where(w => (w.Status == EnumStatusBediener.Aktiv) && !w.DatenAbgleich.Geloescht).OrderBy(o => o.NachName).ToList(),
        ViewSource = VsBediener
      };
      VsBediener.GroupDescriptions.Add(new PropertyGroupDescription("eStandort.Bezeichnung"));
      VsBediener.View.CurrentChanged += (sen, erg) =>
      {
        BenutzerGeaendert();
      };

      AzBediener = NeuAzBediener;
      AzBediener.SetDb(_Db);

      VsAnzeigeTage.Source = AzBediener.ListeTage;
      JahrGeandert();
    }

    public void JahrGeandert()
    {
      var pausen =
      ListePausen = new JgEntityList<tabPausenzeit>(_Db)
      {
        Daten = _Db.tabPausenzeitSet.Where(w => (!w.DatenAbgleich.Geloescht)).ToList()
      };

      var jahr = Jahr;

      ListeSollstundenJahr = new JgEntityList<tabSollStunden>(_Db)
      {
        Daten = _Db.tabSollStundenSet.Where(f => (f.Jahr == jahr)).ToList()
      };

      var ersterJahr = new DateTime(jahr, 1, 1);
      var letzterJahr = new DateTime(jahr, 12, 31, 23, 59, 59);

      ListeFeiertageJahr = new JgEntityList<tabFeiertage>(_Db)
      {
        Daten = _Db.tabFeiertageSet.Where(w => (!w.DatenAbgleich.Geloescht) && (w.Datum >= ersterJahr) && (w.Datum <= letzterJahr)).ToList()
      };

      ListeRundenJahr = new JgEntityList<tabArbeitszeitRunden>(_Db)
      {
        Daten = _Db.tabArbeitszeitRundenSet.Where(w => (w.Jahr == jahr)).ToList()
      };

      MonatGeandert();
    }

    public void MonatGeandert()
    {
      var monat = Monat;

      var sollStunde = ListeSollstundenJahr.Daten.FirstOrDefault(f => (f.Monat == monat) && (!f.DatenAbgleich.Geloescht));
      SollStundenMonat = (sollStunde == null) ? TimeSpan.Zero : JgZeit.StringInZeit(sollStunde.SollStunden);

      ListeFeiertageMonat = ListeFeiertageJahr.Daten.Where(w => (w.Datum.Month == monat) && (!w.DatenAbgleich.Geloescht)).ToList();
      ListeRundenMonat = _Db.tabArbeitszeitRundenSet.Where(w => (w.Monat == monat) && (!w.DatenAbgleich.Geloescht)).ToList();

      var idisBediener = ListeBediener.Daten.Select(s => s.Id).ToArray();
      var listAuswertungen = _Db.tabArbeitszeitAuswertungSet.Where(w => (idisBediener.Contains(w.fBediener) && (w.Jahr == Jahr) && (w.Monat == monat))).ToList();

      foreach (var bediener in ListeBediener.Daten)
        bediener.eArbeitszeitHelper = listAuswertungen.FirstOrDefault(f => f.eBediener == bediener);

      BenutzerGeaendert();
    }

    public void BenutzerGeaendert()
    {
      Mouse.OverrideCursor = Cursors.Wait;
      AzBediener.BedienerBerechnen(AktuellerBediener, Jahr, Monat, SollStundenMonat, ListeRundenMonat, ListeFeiertageMonat, ListePausen.Daten);
      Mouse.OverrideCursor = null;
    }

    public void DatensatzAutomatischBerechnenGeaendert()
    {
      var dsAuswahlTag = (tabArbeitszeitTag)_VsAnzeigeTage.View.CurrentItem;
      dsAuswahlTag.IstManuellGeaendert = !dsAuswahlTag.IstManuellGeaendert;
      dsAuswahlTag.NotifyPropertyChanged("IstManuellGeaendert");
      _Db.SaveChanges();

      if (!dsAuswahlTag.IstManuellGeaendert)
        BenutzerGeaendert();
    }
  }
}

