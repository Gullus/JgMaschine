using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineSetup.Commands;

namespace JgMaschineSetup
{
  public partial class MainWindow : Window
  {
    private JgEntityView<tabMaschine> _ListeMaschinen;
    private JgEntityView<tabStandort> _ListeStandorte;
    private JgEntityView<tabBediener> _ListeBediener;

    public MainWindow()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _ListeStandorte = new JgEntityView<tabStandort>()
      {
        ViewSource = (CollectionViewSource)this.FindResource("vsStandort"),
        Tabellen = new DataGrid[] { dgStandort },
        DatenErstellen = (db) =>
        {
          return db.tabStandortSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.Bezeichnung).ToList();
        }
      };
      _ListeStandorte.DatenAktualisieren();
      tbDatenbankverbinudng.Text = _ListeStandorte.Db.Database.Connection.ConnectionString;

     _ListeMaschinen = new JgEntityView<tabMaschine>()
      {
        ViewSource = (CollectionViewSource)this.FindResource("vsMaschinen"),
        Tabellen = new DataGrid[] { dgMaschine },
        DatenErstellen = (db) =>
        {
          return db.tabMaschineSet.Where(w => !w.DatenAbgleich.Geloescht).Include(i => i.eProtokoll).OrderBy(o => o.MaschinenName).ToList();
        }
      };
      _ListeMaschinen.DatenAktualisieren();

      _ListeBediener = new JgEntityView<tabBediener>()
      {
        ViewSource = (CollectionViewSource)this.FindResource("vsBediener"),
        Tabellen = new DataGrid[] { dgBediener },
        DatenErstellen = (db) =>
        {
          return db.tabBedienerSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.NachName).ToList();
        }
      };
      _ListeBediener.DatenAktualisieren();

      cbStatusBediener.ItemsSource = Enum.GetValues(typeof(JgMaschineData.EnumStatusBediener));
      InitCommands();

      tbNetversion.Text = Helper.GetNetversion();
    }

    public void InitCommands()
    {
      CommandBindings.Add(new CommandBinding(MyCommands.MaschineBearbeiten, (sen, erg) =>
      {
        var form = new Fenster.FormMaschinenOptionen(_ListeMaschinen.Current, _ListeStandorte.Daten);
        if (form.ShowDialog() ?? false)
          _ListeMaschinen.DsSave();
        else
          _ListeMaschinen.Reload();
      },
      (sen, erg) =>
      {
        erg.CanExecute = _ListeMaschinen.Current != null;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.BedienerBeabeiten, (sen, erg) =>
      {
        var form = new Fenster.FormBediener(_ListeBediener.Current, _ListeStandorte.Daten);
        if (form.ShowDialog() ?? false)
          _ListeBediener.DsSave();
        else
          _ListeBediener.Reload();
      }, (sen, erg) =>
      {
        erg.CanExecute = _ListeBediener.Current != null;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.StandortBearbeiten, (sen, erg) =>
      {
        var form = new Fenster.FormStandort(_ListeStandorte.Current);
        if (form.ShowDialog() ?? false)
          _ListeStandorte.DsSave();
        else
          _ListeStandorte.Reload();
      }
      , (sen, erg) =>
      {
        erg.CanExecute = _ListeStandorte.Current != null;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.ProtokollBearbeiten, (sen, erg) =>
      {
        var prot = _ListeMaschinen.Current.eProtokoll;
        if (prot == null)
          prot = ProtokollErstellen(_ListeMaschinen.Current);
        var form = new FormProtokollOptionen(_ListeMaschinen.Current.eProtokoll);
        if (form.ShowDialog() ?? false)
        {
          DbSichern.AbgleichEintragen(form.Protokoll.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
          _ListeMaschinen.Db.SaveChanges();
        }
        else
          _ListeMaschinen.Db.Entry(form.Protokoll);
      }, (sen, erg) =>
      {
        erg.CanExecute = _ListeMaschinen.Current != null;
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

      var form = new JgMaschineSetup.Fenster.FormBediener(null, _ListeStandorte.Daten);
      if (form.ShowDialog() ?? false)
        _ListeBediener.Add(form.Bediener);
    }

    private bool KontrolleStandorte()
    {
      if (_ListeStandorte.Current == null)
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
        LetzteZeile = 0,
        ProtokollText = "Protokoll erstellt",
      };
      JgMaschineLib.DbSichern.AbgleichEintragen(prot.DatenAbgleich, EnumStatusDatenabgleich.Neu);
      _ListeMaschinen.Db.tabProtokollSet.Add(prot);
      return prot;
    }

    private void ButtonNeueMaschine_Click(object sender, RoutedEventArgs e)
    {
      if (!KontrolleStandorte())
        return;

      var form = new Fenster.FormMaschinenOptionen(null, _ListeStandorte.Daten);
      if (form.ShowDialog() ?? false)
      {
        _ListeMaschinen.Add(form.Maschine);
        ProtokollErstellen(form.Maschine);
      }
    }

    private void ButtonDatenAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      switch ((sender as Button).Tag.ToString())
      {
        case "Maschine": _ListeMaschinen.DatenAktualisieren(); break;
        case "Standort": _ListeStandorte.DatenAktualisieren(); break;
        case "Bediener": _ListeBediener.DatenAktualisieren(); break;
      }
    }

    private void btnExportBedienerDatafox_Click(object sender, RoutedEventArgs e)
    {
      var msg = "Benutzer in den Terminals aktualisieren ?";
      var erg = MessageBox.Show(msg, "Information", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
      if (erg == MessageBoxResult.OK)
      {
        foreach (var standort in _ListeStandorte.Daten)
        {
          standort.UpdateBedienerDatafox = true;
          DbSichern.AbgleichEintragen(standort.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
        _ListeStandorte.Db.SaveChanges();
      }
    }

    private void PfadEreignissanzeigeEinrichten_Click(object sender, RoutedEventArgs e)
    {
      //Todo Pfade für WIndows Log setzen 

      //Proto.PfadeInWindowsEreignissAnzeigeSetzten();
    }
  }
}
