using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JgGlobalZeit.Fenster;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineGlobalZeit.Fenster
{
  public partial class FormOptionen : Window
  {
    private AnmeldungAuswertung _Auswertung;
    private JgEntityTab<tabArbeitszeitTerminal> _ListeTerminals;

    public FormOptionen(AnmeldungAuswertung Auswertung)
    {
      InitializeComponent();

      _Auswertung = Auswertung;
      tbJahr.Text = Auswertung.Jahr.ToString();

      for (byte i = 1; i <= 12; i++)
      {
        if (_Auswertung.ListeSollstundenJahr.Daten.FirstOrDefault(f => f.Monat == i) == null)
        {
          var neuSoll = new tabSollStunden()
          {
            Id = Guid.NewGuid(),
            Jahr = Auswertung.Jahr,
            Monat = i,
          };
          _Auswertung.Db.tabSollStundenSet.Add(neuSoll);
          _Auswertung.ListeSollstundenJahr.Add(neuSoll);
        };
      }

      var idisBediener = Auswertung.ListeBediener.Daten.Select(s => s.Id).ToArray();
      var auswertungenVorjahr = Auswertung.Db.tabArbeitszeitAuswertungSet.Where(w => (idisBediener.Contains(w.fBediener)) && (w.Jahr == Auswertung.Jahr) && (w.Monat == 0)).ToList();
      foreach (var bediener in _Auswertung.ListeBediener.Daten)
      {
        var auswVorjahr = auswertungenVorjahr.FirstOrDefault(f => f.fBediener == bediener.Id);
        if (auswVorjahr == null)
        {
          auswVorjahr = new tabArbeitszeitAuswertung()
          {
            Id = Guid.NewGuid(),
            Jahr = Auswertung.Jahr,
            Monat = 0,
            Ueberstunden = "00:00",
            Urlaub = 0,

            eBediener = bediener,
            Status = EnumStatusArbeitszeitAuswertung.Erledigt,
          };
          Auswertung.Db.tabArbeitszeitAuswertungSet.Add(auswVorjahr);
        }
        bediener.eArbeitszeitHelper = auswVorjahr;
      }

      _ListeTerminals = new JgEntityTab<tabArbeitszeitTerminal>(_Auswertung.Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsTerminals"),
        Tabellen = new DataGrid[] { gridTerminals }
      };
      _ListeTerminals.Daten = _Auswertung.Db.tabArbeitszeitTerminalSet.ToList();
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _Auswertung.ListeFeiertageJahr.ViewSource = (CollectionViewSource)(this.FindResource("vsFeiertage"));
      _Auswertung.ListeSollstundenJahr.ViewSource = (CollectionViewSource)(this.FindResource("vsSollStunden"));
      _Auswertung.ListePausen.ViewSource = (CollectionViewSource)(this.FindResource("vsPausen"));

      _Auswertung.ListeRundenJahr.ViewSource = (CollectionViewSource)(this.FindResource("vsRunden"));
      _Auswertung.ListeRundenJahr.ViewSource.GroupDescriptions.Add(new PropertyGroupDescription("eStandort.Bezeichnung"));

      var vsBediener = ((CollectionViewSource)(this.FindResource("vsBediener")));
      vsBediener.Source = _Auswertung.ListeBediener.Daten;
    }

    private void btnNeuerFeiertag_Click(object sender, RoutedEventArgs e)
    {
      var feiertag = new tabFeiertage()
      {
        Id = Guid.NewGuid(),
        Datum = DateTime.Now.Date
      };
      _Auswertung.Db.tabFeiertageSet.Add(feiertag);
      _Auswertung.ListeFeiertageJahr.Add(feiertag);
    }

    private void btnNeuePause_Click(object sender, RoutedEventArgs e)
    {
      var pause = new tabPausenzeit()
      {
        Id = Guid.NewGuid(),
        ZeitVon = new TimeSpan(5, 0, 0),
        ZeitBis = new TimeSpan(13, 0, 0),
        Pausenzeit = new TimeSpan(1, 0, 0)
      };
      _Auswertung.Db.tabPausenzeitSet.Add(pause);
      _Auswertung.ListePausen.Add(pause);
    }

    private void btnRundungswert_Click(object sender, RoutedEventArgs e)
    {
      var vorg = Convert.ToByte((sender as Button).Tag);  // 0 - Neu, 1 - Bearbeiten 
      var vsRunden = (CollectionViewSource)FindResource("vsRunden");
      var runden = (tabArbeitszeitRunden)vsRunden.View.CurrentItem;

      var fo = new FormArbeitszeitRunden(_Auswertung, runden, vorg == 0);
      if (!(fo.ShowDialog() ?? false))
        vsRunden.View.Refresh();
      vsRunden.View.MoveCurrentTo(fo.AktuellerWert);
    }

    private void btnTerminal_Click(object sender, RoutedEventArgs e)
    {
      var standorte = _Auswertung.Db.tabStandortSet.OrderBy(o => o.Bezeichnung).ToList();
      var fo = new FormTerminal(standorte, (sender == btnNeuesTermin) ? null : _ListeTerminals.Current);
      _ListeTerminals.ErgebnissFormular(fo.ShowDialog(), fo.IstNeu, fo.Terminal);
    }
  }
}
