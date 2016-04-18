using JgMaschineData;
using JgMaschineSetup.Commands;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JgMaschineSetup
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private JgModelContainer _Db;
    private CollectionViewSource _VsMaschine { get { return (CollectionViewSource)this.FindResource("vsMaschinen"); } }
    private CollectionViewSource _VsBediener { get { return (CollectionViewSource)this.FindResource("vsBediener"); } }
    private CollectionViewSource _VsDaten { get { return (CollectionViewSource)this.FindResource("vsDaten"); } }
    private CollectionViewSource _VsStandort { get { return (CollectionViewSource)this.FindResource("vsStandort"); } }

    public MainWindow()
    {
      InitializeComponent();
      dtpAuswertungVon.SelectedDate = DateTime.Today.AddDays(-7);
      dtpAuswertungBis.SelectedDate = DateTime.Today;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _Db = new JgMaschineData.JgModelContainer();

      _VsMaschine.Source = _Db.tabMaschineSet.ToList();
      _VsMaschine.View.CurrentChanged += ViewMaschine_CurrentChanged;
      ViewMaschine_CurrentChanged(null, null);

      _VsBediener.Source = _Db.tabBedienerSet.ToList();
      _VsStandort.Source = _Db.tabStandortSet.ToList();

      cbStatusBediener.ItemsSource = Enum.GetValues(typeof(JgMaschineData.EnumStatusBediener));

      InitCommands();
    }

    void ViewMaschine_CurrentChanged(object sender, EventArgs e)
    {
      if (_VsMaschine.View.CurrentItem != null)
      {
        var idMaschine = (_VsMaschine.View.CurrentItem as JgMaschineData.tabMaschine).Id;
        DateTime datVom = (DateTime)dtpAuswertungVon.SelectedDate, datBis = ((DateTime)(dtpAuswertungBis.SelectedDate)).AddDays(1);
        _VsDaten.Source = _Db.tabDatenSet.Where(w => (w.fMaschine == idMaschine) && (w.DatumStart >= datVom) && (w.DatumStart < datBis)).OrderBy(o => o.DatumStart).ToList();
      }
    }

    public void InitCommands()
    {
      CommandBindings.Add(new CommandBinding(MyCommands.MaschineBearbeiten, (sen, erg) =>
      {
        var form = new FormMaschinenOptionen(_Db, (tabMaschine)_VsMaschine.View.CurrentItem);
        if (!form.ShowDialog() ?? false)
        {
          _Db.Entry(form.Maschine).Reload();
          _VsMaschine.View.Refresh();
        }
        _Db.SaveChanges();
      },
      (sen, erg) =>
      {
        erg.CanExecute = !_VsMaschine.View.IsEmpty;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.BedienerBeabeiten, (sen, erg) =>
      {
        var form = new JgMaschineSetup.Fenster.FormBediener(_Db, (tabBediener)_VsBediener.View.CurrentItem);
        if (!form.ShowDialog() ?? false)
        {
          _Db.Entry(form.Bediener).Reload();
          _VsBediener.View.Refresh();
        }
        _Db.SaveChanges();
      }, (sen, erg) =>
      {
        erg.CanExecute = !_VsBediener.View.IsEmpty;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.StandortBearbeiten, (sen, erg) =>
      {
        var form = new JgMaschineSetup.Fenster.FormStandort(_Db, (tabStandort)_VsStandort.View.CurrentItem);
        if (!form.ShowDialog() ?? false)
        {
          _Db.Entry(form.Standort).Reload();
          _VsStandort.View.Refresh();
        }
        _Db.SaveChanges();
      }
      , (sen, erg) =>
      {
        erg.CanExecute = !_VsStandort.View.IsEmpty;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.ProtokollBearbeiten, (sen, erg) =>
      {
        var maschine = (tabMaschine)_VsMaschine.View.CurrentItem;
        var form = new FormProtokollOptionen(_Db, maschine.eProtokoll);
        if (!form.ShowDialog() ?? false)
          _Db.Entry(form.Protokoll).Reload();

        _Db.SaveChanges();
      }, (sen, erg) =>
      {
        erg.CanExecute = !_VsMaschine.View.IsEmpty && ((_VsMaschine.View.CurrentItem as JgMaschineData.tabMaschine).ProtokollName != EnumProtokollName.Handbiegung);
      }));

      // CommandBindings.Add(new CommandBinding(MyCommands.ReportBenutzerBearbeiten, ReportBenutzerExecute));
    }

    private void ButtonNeuerStandort_Click(object sender, RoutedEventArgs e)
    {
      var form = new JgMaschineSetup.Fenster.FormStandort(_Db, null);
      if (form.ShowDialog() ?? false)
      {
        _Db.tabStandortSet.Add(form.Standort);
        _Db.SaveChanges();
        _VsStandort.Source = _Db.tabStandortSet.ToList();
        _VsStandort.View.MoveCurrentTo(form.Standort);
      }
    }

    private void ButtonNeuerBediener_Click(object sender, RoutedEventArgs e)
    {
      var form = new JgMaschineSetup.Fenster.FormBediener(_Db, null);
      if (form.ShowDialog() ?? false)
      {
        _Db.tabBedienerSet.Add(form.Bediener);
        _Db.SaveChanges();
        _VsBediener.Source = _Db.tabBedienerSet.ToList();
        _VsBediener.View.MoveCurrentTo(form.Bediener);
      }
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
        _Db.tabMaschineSet.Add(form.Maschine);
        _Db.SaveChanges();

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
        _Db.tabProtokollSet.Add(prot);
        _Db.SaveChanges();

        _VsMaschine.Source = _Db.tabMaschineSet.ToList();
        _VsMaschine.View.MoveCurrentTo(form.Maschine);
      }
    }

    private void ButtonDatenAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      _Db.SaveChanges();
      foreach (var entity in _Db.ChangeTracker.Entries())
        entity.Reload();
      CollectionViewSource.GetDefaultView(gridMaschinen.ItemsSource).Refresh();
    }

    private void btnAuswertungStarten_Click(object sender, RoutedEventArgs e)
    {
      ViewMaschine_CurrentChanged(null, null);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      _Db.SaveChanges();
    }
  }
}
