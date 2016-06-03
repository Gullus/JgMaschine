using JgMaschineData;
using JgMaschineSetup.Commands;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Data.Entity;
using System.Threading.Tasks;

namespace JgMaschineSetup
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private JgModelContainer _Db;
 
    private JgMaschineLib.JgListe<JgMaschineData.tabMaschine> _ListeMaschinen;
    private JgMaschineLib.JgListe<JgMaschineData.tabStandort> _ListeStandorte;
    private JgMaschineLib.JgListe<JgMaschineData.tabBediener> _ListeBediener;

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _Db = new JgMaschineData.JgModelContainer();

      var standorte = await _Db.tabStandortSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.Bezeichnung).ToListAsync();
      var dv = (CollectionViewSource)this.FindResource("vsStandort");
      _ListeStandorte = new JgMaschineLib.JgListe<tabStandort>(_Db, standorte, dv, dgStandort);

      var bediener = await _Db.tabBedienerSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.NachName).ToListAsync();
      dv = (CollectionViewSource)this.FindResource("vsBediener");
      _ListeBediener = new JgMaschineLib.JgListe<tabBediener>(_Db, bediener, dv, dgBediener);

      var maschinen = await _Db.tabMaschineSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.MaschinenName).ToListAsync();
      dv = (CollectionViewSource)this.FindResource("vsMaschinen");
      _ListeMaschinen = new JgMaschineLib.JgListe<tabMaschine>(_Db, maschinen, dv, dgMaschine);

      cbStatusBediener.ItemsSource = Enum.GetValues(typeof(JgMaschineData.EnumStatusBediener));

      InitCommands();
    }

    public void InitCommands()
    {
      CommandBindings.Add(new CommandBinding(MyCommands.MaschineBearbeiten, (sen, erg) =>
      {
        var form = new FormMaschinenOptionen(_Db, _ListeMaschinen.AktDatensatz);
        if (!form.ShowDialog() ?? false)
          _ListeMaschinen.Reload(_Db);
      },
      (sen, erg) =>
      {
        erg.CanExecute = !_ListeMaschinen.IsEmpty;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.BedienerBeabeiten, (sen, erg) =>
      {
        var form = new JgMaschineSetup.Fenster.FormBediener(_Db, _ListeBediener.AktDatensatz);
        if (!form.ShowDialog() ?? false)
          _ListeBediener.Reload(_Db);
      }, (sen, erg) =>
      {
        erg.CanExecute = !_ListeBediener.IsEmpty;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.StandortBearbeiten, (sen, erg) =>
      {
        var form = new JgMaschineSetup.Fenster.FormStandort(_Db, _ListeStandorte.AktDatensatz);
        if (!form.ShowDialog() ?? false)
          _ListeStandorte.Reload(_Db);
      }
      , (sen, erg) =>
      {
        erg.CanExecute = !_ListeStandorte.IsEmpty;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.ProtokollBearbeiten, (sen, erg) =>
      {
        var form = new FormProtokollOptionen(_Db, _ListeMaschinen.AktDatensatz.eProtokoll);
        if (!form.ShowDialog() ?? false)
        {
          _Db.Entry(form.Protokoll).Reload();
          JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabProtokoll>(_Db, form.Protokoll, EnumStatusDatenabgleich.Geaendert);
        }
      }, (sen, erg) =>
      {
        erg.CanExecute = !_ListeMaschinen.IsEmpty;
      }));

      // CommandBindings.Add(new CommandBinding(MyCommands.ReportBenutzerBearbeiten, ReportBenutzerExecute));
    }

    private void ButtonNeuerStandort_Click(object sender, RoutedEventArgs e)
    {
      var form = new JgMaschineSetup.Fenster.FormStandort(_Db, null);
      if (form.ShowDialog() ?? false)
        _ListeStandorte.Add(form.Standort);
    }

    private void ButtonNeuerBediener_Click(object sender, RoutedEventArgs e)
    {
      var form = new JgMaschineSetup.Fenster.FormBediener(_Db, null);
      if (form.ShowDialog() ?? false)
        _ListeBediener.Add(form.Bediener);
    }

    private void ButtonNeueMaschine_Click(object sender, RoutedEventArgs e)
    {
      if (_Db.tabStandortSet.FirstOrDefault() == null)
      {
        MessageBox.Show("Es müssen Standorte vorhanden sein.", "Information !", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
      }

      var form = new FormMaschinenOptionen(_Db, null);
      if (form.ShowDialog() ?? false)
      {
        _ListeMaschinen.Add(form.Maschine);
        
        var prot = new tabProtokoll()
        { 
          Id = form.Maschine.Id,
          AuswertungStart = DateTime.Now.Date,
          AuswertungEnde = DateTime.Now.Date,
          LetzteDateiDatum = DateTime.Today.AddDays(-60),
          LetztePositionDatum = DateTime.Today.AddDays(-60),
          ProtokollText = "Protokoll erstellt",
          Status = JgMaschineData.EnumStatusProtkoll.Offen
        };
        JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabProtokoll>(_Db, prot, EnumStatusDatenabgleich.Neu);
      }
    }

    private void ButtonDatenAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      _Db.SaveChanges();
      foreach (var entity in _Db.ChangeTracker.Entries())
        entity.Reload();
      CollectionViewSource.GetDefaultView(dgMaschine.ItemsSource).Refresh();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      _Db.SaveChanges();
    }
  }
}
