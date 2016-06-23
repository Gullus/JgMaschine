using JgMaschineData;
using JgMaschineSetup.Commands;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JgMaschineSetup
{
  public partial class MainWindow : Window
  {
    private JgMaschineLib.JgList<JgMaschineData.tabMaschine> _ListeMaschinen;
    private JgMaschineLib.JgList<JgMaschineData.tabStandort> _ListeStandorte;
    private JgMaschineLib.JgList<JgMaschineData.tabBediener> _ListeBediener;

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _ListeMaschinen = new JgMaschineLib.JgList<tabMaschine>((CollectionViewSource)this.FindResource("vsMaschinen"));
      _ListeMaschinen.MyQuery = _ListeMaschinen.Db.tabMaschineSet.Where(w => !w.DatenAbgleich.Geloescht).Include(i => i.eProtokoll).OrderBy(o => o.MaschinenName);
      _ListeMaschinen.ListeTabellen = new DataGrid[] { dgMaschine };
      await _ListeMaschinen.DatenGenerierenAsync();

      _ListeStandorte = new JgMaschineLib.JgList<tabStandort>((CollectionViewSource)this.FindResource("vsStandort"));
      _ListeStandorte.MyQuery = _ListeStandorte.Db.tabStandortSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.Bezeichnung);
      _ListeStandorte.ListeTabellen = new DataGrid[] { dgStandort };
      await _ListeStandorte.DatenGenerierenAsync();

      _ListeBediener = new JgMaschineLib.JgList<tabBediener>((CollectionViewSource)this.FindResource("vsBediener"));
      _ListeBediener.MyQuery = _ListeBediener.Db.tabBedienerSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.NachName);
      _ListeBediener.ListeTabellen = new DataGrid[] { dgBediener };
      await _ListeBediener.DatenGenerierenAsync();

      cbStatusBediener.ItemsSource = Enum.GetValues(typeof(JgMaschineData.EnumStatusBediener));

      InitCommands();
    }

    public void InitCommands()
    {
      CommandBindings.Add(new CommandBinding(MyCommands.MaschineBearbeiten, (sen, erg) =>
      {
        var form = new Fenster.FormMaschinenOptionen(_ListeMaschinen.AktDatensatz, _ListeStandorte);
        if (form.ShowDialog() ?? false)
          _ListeMaschinen.AktSichern(EnumStatusDatenabgleich.Geaendert);
        else
          _ListeMaschinen.Reload(form.Maschine);
      },
      (sen, erg) =>
      {
        erg.CanExecute = !_ListeMaschinen.IsEmpty;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.BedienerBeabeiten, (sen, erg) =>
      {
        var form = new Fenster.FormBediener(_ListeBediener.AktDatensatz, _ListeStandorte);
        if (form.ShowDialog() ?? false)
          _ListeBediener.AktSichern(EnumStatusDatenabgleich.Geaendert);
        else
          _ListeBediener.Reload(form.Bediener);
      }, (sen, erg) =>
      {
        erg.CanExecute = !_ListeBediener.IsEmpty;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.StandortBearbeiten, (sen, erg) =>
      {
        var form = new Fenster.FormStandort(_ListeStandorte.AktDatensatz);
        if (form.ShowDialog() ?? false)
          _ListeStandorte.AktSichern(EnumStatusDatenabgleich.Geaendert);
        else
          _ListeStandorte.Reload(form.Standort);
      }
      , (sen, erg) =>
      {
        erg.CanExecute = !_ListeStandorte.IsEmpty;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.ProtokollBearbeiten, (sen, erg) =>
      {
        var prot = _ListeMaschinen.AktDatensatz.eProtokoll;
        if (prot == null)
          prot = ProtokollErstellen(_ListeMaschinen.AktDatensatz);
        var form = new FormProtokollOptionen(_ListeMaschinen.AktDatensatz.eProtokoll);
        if (form.ShowDialog() ?? false)
          _ListeMaschinen.DsSichern(form.Protokoll, EnumStatusDatenabgleich.Geaendert);
        else
          _ListeMaschinen.DsReload<tabProtokoll>(form.Protokoll);
      }, (sen, erg) =>
      {
        erg.CanExecute = !_ListeMaschinen.IsEmpty;
      }));
    }

    private void ButtonNeuerStandort_Click(object sender, RoutedEventArgs e)
    {
      var form = new JgMaschineSetup.Fenster.FormStandort(null);
      if (form.ShowDialog() ?? false)
        _ListeStandorte.Add(form.Standort);
    }

    private void ButtonNeuerBediener_Click(object sender, RoutedEventArgs e)
    {
      if (!KontrolleStandorte())
        return;

      var form = new JgMaschineSetup.Fenster.FormBediener(null, _ListeStandorte);
      if (form.ShowDialog() ?? false)
        _ListeBediener.Add(form.Bediener);
    }

    private bool KontrolleStandorte()
    {
      if (_ListeStandorte.IsEmpty)
      {
        MessageBox.Show("Es müssen Standorte vorhanden sein.", "Information !", MessageBoxButton.OK, MessageBoxImage.Information);
        return false;
      }
      return true; 
    }

    private tabProtokoll ProtokollErstellen(tabMaschine ZuMaschine)
    {
      var prot = new tabProtokoll()
      {
        Id = ZuMaschine.Id,
        AuswertungStart = DateTime.Now.Date,
        AuswertungEnde = DateTime.Now.Date,
        LetzteDateiDatum = DateTime.Today.AddDays(-60),
        LetztePositionDatum = DateTime.Today.AddDays(-60),
        ProtokollText = "Protokoll erstellt",
        Status = JgMaschineData.EnumStatusProtkoll.Offen
      };
      _ListeMaschinen.DsSichern(prot, EnumStatusDatenabgleich.Neu);

      return prot;
    }

    private void ButtonNeueMaschine_Click(object sender, RoutedEventArgs e)
    {
      if (! KontrolleStandorte())
        return;
 
      var form = new Fenster.FormMaschinenOptionen(null, _ListeStandorte);
      if (form.ShowDialog() ?? false)
      {
        _ListeMaschinen.Add(form.Maschine);
        ProtokollErstellen(form.Maschine);
      }
    }

    private async void ButtonDatenAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      await _ListeMaschinen.DatenGenerierenAsync();
    }
  }
}
