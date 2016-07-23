using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineLib.Zeit;
using JgMaschineVerwalten.Commands;

namespace JgMaschineVerwalten
{
  public partial class MainWindow : Window
  {
    private tabStandort _Standort;
    private JgList<tabBediener> _ListeBediener;
    private CollectionViewSource _VsBediener { get { return (CollectionViewSource)this.FindResource("vsBedienerArbeitszeit"); } }
    private CollectionViewSource _VsBedienerAnmeldung { get { return (CollectionViewSource)this.FindResource("vsBedienerAnmeldung"); } }
    private List<tabMaschine> _ListeMaschinen { get { return (List<tabMaschine>)_VsMaschine.Source; } }
    private CollectionViewSource _VsMaschine { get { return (CollectionViewSource)FindResource("vsMaschine"); } }
    private tabMaschine _Maschine
    {
      get
      {
        if (treeViewMaschinen.SelectedItem == null)
          return null;

        if (treeViewMaschinen.SelectedItem is tabMaschine)
          return (tabMaschine)treeViewMaschinen.SelectedItem;
        else
          return (treeViewMaschinen.SelectedItem as tabAnmeldungMaschine).eMaschine;
      }
    }

    private JgList<tabAuswertung> _ListeAuswertung;

    private CollectionViewSource _VsAuswertungArbeitszeit { get { return (CollectionViewSource)this.FindResource("vsAuswertungArbeitszeit"); } }
    private CollectionViewSource _VsAuswertungAnmeldung { get { return (CollectionViewSource)this.FindResource("vsAuswertungAnmeldung"); } }
    private CollectionViewSource _VsAuswertungBauteil { get { return (CollectionViewSource)this.FindResource("vsAuswertungBauteil"); } }
    private CollectionViewSource _VsAuswertungReparatur { get { return (CollectionViewSource)this.FindResource("vsAuswertungReparatur"); } }

    private JgList<tabArbeitszeit> _ListeArbeitszeitAktuell;
    private JgList<tabArbeitszeit> _ListeArbeitszeitAuswahl;
    private JgDatumZeit _DzArbeitszeitVon { get { return (JgDatumZeit)this.FindResource("dzArbeitszeitVon"); } }
    private JgDatumZeit _DzArbeitszeitBis { get { return (JgDatumZeit)this.FindResource("dzArbeitszeitBis"); } }

    private JgList<tabAnmeldungMaschine> _ListeAnmeldungAktuell;
    private JgList<tabAnmeldungMaschine> _ListeAnmeldungAuswahl;
    private JgDatumZeit _DzAnmeldungVon { get { return (JgDatumZeit)this.FindResource("dzAnmeldungVon"); } }
    private JgDatumZeit _DzAnmeldungBis { get { return (JgDatumZeit)this.FindResource("dzAnmeldungBis"); } }

    private JgDatumZeit _DzBauteilVon { get { return (JgDatumZeit)this.FindResource("dzBauteilVon"); } }
    private JgDatumZeit _DzBauteilBis { get { return (JgDatumZeit)this.FindResource("dzBauteilBis"); } }
    private JgList<tabBauteil> _ListeBauteilAuswahl;

    private JgDatumZeit _DzReparaturVon { get { return (JgDatumZeit)this.FindResource("dzReparaturVon"); } }
    private JgDatumZeit _DzReparaturBis { get { return (JgDatumZeit)this.FindResource("dzReparaturBis"); } }
    private JgList<tabReparatur> _ListeReparaturAktuell;
    private JgList<tabReparatur> _ListeReparaturAuswahl;

    private DispatcherTimer _AktualisierungsTimer;

    private FastReport.Report _Report;
    private FastReport.EnvironmentSettings _ReportSettings = new FastReport.EnvironmentSettings();
    private tabAuswertung _AktAuswertung = null;

    private Guid[] _IdisAktivArbeitszeit { get { return _ListeBediener.Where(w => (w.fAktivArbeitszeit != null)).Select(s => (Guid)s.fAktivArbeitszeit).ToArray(); } }
    private Guid[] _IdisMaschine { get { return _ListeMaschinen.Select(s => s.Id).ToArray(); } }
    private Guid[] _IdisAktivReparatur { get { return _ListeMaschinen.Where(w => (w.fAktivReparatur != null)).Select(s => (Guid)s.fAktivReparatur).ToArray(); } }

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      tblDatum.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
      var von = DateTime.Now.Date;
      var bis = new DateTime(von.Year, von.Month, von.Day, 23, 59, 59);

      // Standort initialieren

      using (var db = new JgMaschineData.JgModelContainer())
      {
        _Standort = await db.tabStandortSet.FindAsync(Properties.Settings.Default.IdStandort);
        if (_Standort == null)
          _Standort = await db.tabStandortSet.FirstOrDefaultAsync();
      }
      tblStandort.Text = _Standort.Bezeichnung;

      await InitListen(von, bis);

      InitCommands();

      _Report = new FastReport.Report();
      _Report.FileName = "Datenbank";
      _ReportSettings.CustomSaveReport += (obj, repEvent) =>
      {
        MemoryStream memStr = new MemoryStream();
        try
        {
          repEvent.Report.Save(memStr);
          _AktAuswertung.Report = memStr.ToArray();
          _AktAuswertung.GeaendertDatum = DateTime.Now;
          _AktAuswertung.GeaendertName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
          _ListeAuswertung.DsSichern(_AktAuswertung, EnumStatusDatenabgleich.Geaendert);
        }
        catch (Exception f)
        {
          System.Windows.MessageBox.Show("Fehler beim speichern des Reports !\r\nGrund: " + f.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
          memStr.Dispose();
        }
      };

      //_AktualisierungsTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = new TimeSpan(0, 0, 10) };
      //_AktualisierungsTimer.Tick += _AktualisierungsTimer_Tick;
      //_AktualisierungsTimer.Start();
    }

    private async Task InitListen(DateTime von, DateTime bis)
    {
      // Bediener initialisieren

      _ListeBediener = new JgList<tabBediener>(_VsBediener);
      _ListeBediener.MyQuery = _ListeBediener.Db.tabBedienerSet.Where(w => (w.fStandort == _Standort.Id) && (w.Status != EnumStatusBediener.Stillgelegt))
        .Include(i => i.eAktivArbeitszeit).Include(i => i.eAktivArbeitszeit.eBediener).OrderBy(o => o.NachName);
      await _ListeBediener.DatenGenerierenAsync();
      _VsBedienerAnmeldung.Source = _ListeBediener;

      using (var db = new JgMaschineData.JgModelContainer())
      {
        _VsMaschine.Source = db.tabMaschineSet.Where(w => (w.fStandort == _Standort.Id) && (w.Status != JgMaschineData.EnumStatusMaschine.Stillgelegt))
          .Include(i => i.sAktiveAnmeldungen).Include(i => i.sAktiveAnmeldungen.Select(s => s.eBediener)).ToList();
      }
      treeViewMaschinen.UpdateLayout();
      if (treeViewMaschinen.Items.Count > 0)
        (treeViewMaschinen.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem).IsSelected = true;

      // Arbeitszeit Initialisieren

      _ListeArbeitszeitAktuell = new JgList<tabArbeitszeit>((CollectionViewSource)FindResource("vsArbeitszeitAktuell"));
      _ListeArbeitszeitAktuell.MyQuery = _ListeArbeitszeitAktuell.Db.tabArbeitszeitSet.Where(w => _IdisAktivArbeitszeit.Contains(w.Id))
        .OrderBy(o => o.Anmeldung);
      _ListeArbeitszeitAktuell.ListeTabellen = new DataGrid[] { dgArbeitszeitAktuell };
      await _ListeArbeitszeitAktuell.DatenGenerierenAsync();
      await _ListeArbeitszeitAktuell.Db.tabBedienerSet.Where(w => w.fStandort == _Standort.Id).ToListAsync();

      _DzArbeitszeitVon.DatumZeit = von;
      _DzArbeitszeitBis.DatumZeit = bis;

      _ListeArbeitszeitAuswahl = new JgList<tabArbeitszeit>((CollectionViewSource)FindResource("vsArbeitszeitAuswahl"));
      _ListeArbeitszeitAuswahl.MyQuery = _ListeArbeitszeitAuswahl.Db.tabArbeitszeitSet.Where(w => (w.fStandort == _Standort.Id) && (w.Anmeldung >= _DzArbeitszeitVon.DatumZeit) && ((w.Anmeldung <= _DzArbeitszeitBis.DatumZeit)))
        .OrderBy(o => o.Anmeldung);
      _ListeArbeitszeitAuswahl.ListeTabellen = new DataGrid[] { dgArbeitszeitAuswahl };

      // Anmeldungen initialisieren

      _ListeAnmeldungAktuell = new JgList<tabAnmeldungMaschine>((CollectionViewSource)FindResource("vsAnmeldungAktuell"));
      _ListeAnmeldungAktuell.MyQuery = _ListeAnmeldungAktuell.Db.tabAnmeldungMaschineSet.Where(w => _IdisMaschine.Contains(w.fAktivMaschine ?? Guid.Empty))
        .Include(i => i.eBediener).Include(i => i.eMaschine)
        .OrderBy(o => o.Anmeldung);
      _ListeAnmeldungAktuell.ListeTabellen = new DataGrid[] { dgAnmeldungAktuell };
      await _ListeAnmeldungAktuell.DatenGenerierenAsync();
      await _ListeAnmeldungAktuell.Db.tabBedienerSet.Where(w => w.fStandort == _Standort.Id).ToListAsync();
      await _ListeAnmeldungAktuell.Db.tabMaschineSet.Where(w => w.fStandort == _Standort.Id).ToListAsync();

      _DzAnmeldungVon.DatumZeit = von;
      _DzAnmeldungBis.DatumZeit = bis;

      _ListeAnmeldungAuswahl = new JgList<tabAnmeldungMaschine>((CollectionViewSource)FindResource("vsAnmeldungAuswahl"));
      _ListeAnmeldungAuswahl.MyQuery = _ListeAnmeldungAuswahl.Db.tabAnmeldungMaschineSet.Where(w => _IdisMaschine.Contains(w.fMaschine) && (w.Anmeldung >= _DzAnmeldungVon.DatumZeit) && ((w.Anmeldung <= _DzAnmeldungBis.DatumZeit)))
        .OrderBy(o => o.Anmeldung);
      _ListeAnmeldungAuswahl.ListeTabellen = new DataGrid[] { dgAnmeldungAuswahl };

      // Bauteile initialisieren

      _DzBauteilVon.DatumZeit = von;
      _DzBauteilBis.DatumZeit = bis;

      _ListeBauteilAuswahl = new JgList<tabBauteil>((CollectionViewSource)FindResource("vsBauteilAuswahl"));
      _ListeBauteilAuswahl.MyQuery = _ListeBauteilAuswahl.Db.tabBauteilSet.Where(w => (w.fMaschine == _Maschine.Id) && (w.DatumStart >= _DzBauteilVon.DatumZeit) && (w.DatumStart <= _DzBauteilBis.DatumZeit)).OrderBy(o => o.DatumStart).Include(i => i.sBediener);
      _ListeBauteilAuswahl.ListeTabellen = new DataGrid[] { dgBauteilAuswahl };

      // Reparaturen initialisieren 

      _ListeReparaturAktuell = new JgList<tabReparatur>((CollectionViewSource)FindResource("vsReparaturAktuell"));
      _ListeReparaturAktuell.MyQuery = _ListeReparaturAktuell.Db.tabReparaturSet.Where(w => _IdisAktivReparatur.Contains(w.Id))
        .Include(i => i.eMaschine).Include(i => i.eProtokollant).Include(i => i.eVerursacher).Include(i => i.sAnmeldungen)
        .OrderByDescending(o => o.VorgangBeginn);
      _ListeReparaturAktuell.ListeTabellen = new DataGrid[] { dgReparaturAktuell };
      await _ListeReparaturAktuell.DatenGenerierenAsync();
      await _ListeReparaturAktuell.Db.tabBedienerSet.Where(w => w.fStandort == _Standort.Id).ToListAsync();
      await _ListeReparaturAktuell.Db.tabMaschineSet.Where(w => w.fStandort == _Standort.Id).ToListAsync();
      var sortBenutzerReparatur = new System.ComponentModel.SortDescription("Anmeldung", System.ComponentModel.ListSortDirection.Descending);
      (FindResource("vsReparaturAktuellBediener") as CollectionViewSource).SortDescriptions.Add(sortBenutzerReparatur);

      _DzReparaturVon.DatumZeit = von;
      _DzReparaturBis.DatumZeit = bis;

      _ListeReparaturAuswahl = new JgList<tabReparatur>((CollectionViewSource)FindResource("vsReparaturAuswahl"));
      _ListeReparaturAuswahl.MyQuery = _ListeReparaturAuswahl.Db.tabReparaturSet.Where(w => _IdisMaschine.Contains(w.fMaschine) && (w.VorgangBeginn >= _DzReparaturVon.DatumZeit) && (w.VorgangBeginn <= _DzReparaturBis.DatumZeit))
        .Include(i => i.sAnmeldungen).Include(i => i.sAnmeldungen.Select(s => s.eBediener))
        .OrderByDescending(o => o.VorgangBeginn);
      _ListeReparaturAuswahl.ListeTabellen = new DataGrid[] { dgReparaturAuswahl };
      (FindResource("vsReparaturAuswahlBediener") as CollectionViewSource).SortDescriptions.Add(sortBenutzerReparatur);

      // Auswertung initialisieren

      _ListeAuswertung = new JgList<tabAuswertung>(_VsAuswertungArbeitszeit);
      _VsAuswertungArbeitszeit.Filter += (sen, erg) => erg.Accepted = (erg.Item as tabAuswertung).FilterAuswertung == EnumFilterAuswertung.Arbeitszeit;
      _ListeAuswertung.MyQuery = _ListeAuswertung.Db.tabAuswertungSet.Where(w => w.FilterAuswertung != EnumFilterAuswertung.Allgemein).OrderBy(o => o.ReportName);
      await _ListeAuswertung.DatenGenerierenAsync();

      _VsAuswertungAnmeldung.Source = _ListeAuswertung;
      _VsAuswertungAnmeldung.Filter += (sen, erg) => erg.Accepted = (erg.Item as tabAuswertung).FilterAuswertung == EnumFilterAuswertung.Anmeldung;
      _VsAuswertungBauteil.Source = _ListeAuswertung;
      _VsAuswertungBauteil.Filter += (sen, erg) => erg.Accepted = (erg.Item as tabAuswertung).FilterAuswertung == EnumFilterAuswertung.Bauteil;
      _VsAuswertungReparatur.Source = _ListeAuswertung;
      _VsAuswertungReparatur.Filter += (sen, erg) => erg.Accepted = (erg.Item as tabAuswertung).FilterAuswertung == EnumFilterAuswertung.Reparatur;
    }

    private void InitCommands()
    {
      CommandBindings.Add(new CommandBinding(MyCommands.AnmeldungBediener, ExecuteAnmeldungBenutzerAnmeldung, (sen, erg) =>
      {
        erg.CanExecute = (_Maschine != null);
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.ReparaturNeu, ExecuteReperaturNeu, (sen, erg) =>
      {
        erg.CanExecute = _Maschine != null;
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.AnmeldungReparaturBediener, (sen, erg) =>
      {
        var reparatur = _ListeReparaturAktuell.AktDatensatz;
        var breitsAngemeldet = reparatur.sAnmeldungen.Where(w => w.IstAktiv).Select(s => s.fBediener).ToArray();
        Dictionary<Guid, string> bediener = _ListeBediener.Where(w => !breitsAngemeldet.Contains(w.Id)).ToDictionary(s => s.Id, s => s.Name);

        var form = new Fenster.FormNeuerBedienerReparatur(bediener);
        if (form.ShowDialog() ?? false)
        {
          form.Anmeldung.eReparatur = reparatur;
          DbSichern.AbgleichEintragen(form.Anmeldung.DatenAbgleich, EnumStatusDatenabgleich.Neu);
          _ListeReparaturAktuell.Db.tabAnmeldungReparaturSet.Add(form.Anmeldung);
          _ListeReparaturAktuell.Db.SaveChanges();
          (FindResource("vsReparaturAktuellBediener") as CollectionViewSource).View.Refresh();
        }
      }, (sen, erg) =>
     {
       erg.CanExecute = _ListeReparaturAktuell.AktDatensatz != null;
     }));
    }

    private async Task TreeListAktualisieren()
    {
      _AktualisierungsTimer.Stop();

      var idMaschine = _Maschine.Id;
      Guid? idBediener = null;
      if (treeViewMaschinen.SelectedItem is tabBediener)
        idBediener = (treeViewMaschinen.SelectedItem as tabBediener).Id;

      using (var db = new JgMaschineData.JgModelContainer())
      {
        _VsMaschine.Source = await db.tabMaschineSet.Where(w => (w.fStandort == _Standort.Id) && (w.Status != JgMaschineData.EnumStatusMaschine.Stillgelegt))
          .Include(i => i.sAktiveAnmeldungen).Include(i => i.sAktiveAnmeldungen.Select(s => s.eBediener)).ToListAsync();
      }
      treeViewMaschinen.UpdateLayout();

      if (idBediener != null)
      {
        foreach (var dsMaschine in treeViewMaschinen.Items)
        {
          var itemMaschine = (TreeViewItem)treeViewMaschinen.ItemContainerGenerator.ContainerFromItem(dsMaschine);
          foreach (var dsBenutzer in itemMaschine.Items)
          {
            if ((dsBenutzer as tabBediener).Id == idBediener)
            {
              var itemBenutzer = (itemMaschine.ItemContainerGenerator.ContainerFromItem(dsBenutzer) as TreeViewItem).IsSelected = true;
              return;
            }
          }
        }
      }

      // Wenn kein Benutzer gefunden wird

      foreach (var dsMaschine in treeViewMaschinen.Items)
      {
        if ((dsMaschine as tabMaschine).Id == idMaschine)
        {
          (treeViewMaschinen.ItemContainerGenerator.ContainerFromItem(dsMaschine) as TreeViewItem).IsSelected = true;
          break;
        }
      }

      _AktualisierungsTimer.Start();
    }

    private async void _AktualisierungsTimer_Tick(object sender, EventArgs e)
    {
      tblDatum.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
      await TreeListAktualisieren();
      await _ListeArbeitszeitAktuell.DatenGenerierenAsync();
      await _ListeAnmeldungAktuell.DatenGenerierenAsync();
      await _ListeReparaturAktuell.DatenGenerierenAsync();
    }

    #region Formular Reparatur *****************************************************

    private async void ExecuteReperaturNeu(object sender, ExecutedRoutedEventArgs e)
    {
      if (_Maschine.fAktivReparatur != null)
      {
        MessageBox.Show($"Die Maschine {_Maschine.MaschinenName} ist bereits im Reparaturmodus.", "Information !", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
      }

      var form = new Fenster.FormReparatur(null, _ListeBediener, _Maschine);
      if (form.ShowDialog() ?? false)
      {
        _ListeReparaturAktuell.Add(form.Reparatur);
        form.Reparatur.eAktivMaschine.Add(await _ListeReparaturAktuell.DsAttachAsync<tabMaschine>(_Maschine.Id));

        if (_Maschine.sAktiveAnmeldungen.Count > 0)
        {
          foreach (var anmeldungMaschine in _Maschine.sAktiveAnmeldungen)
          {
            var anmeldungReparatur = new tabAnmeldungReparatur()
            {
              Id = Guid.NewGuid(),
              Anmeldung = form.Reparatur.VorgangBeginn,
              fReparatur = form.Reparatur.Id,
              fBediener = anmeldungMaschine.fBediener
            };
            DbSichern.AbgleichEintragen(anmeldungReparatur.DatenAbgleich, EnumStatusDatenabgleich.Neu);
            _ListeReparaturAktuell.Db.tabAnmeldungReparaturSet.Add(anmeldungReparatur);
          }
          (FindResource("vsReparaturAktuellBediener") as CollectionViewSource).View.Refresh();
        }
        _ListeReparaturAktuell.Db.SaveChanges();

        await TreeListAktualisieren();
      }
    }

    private void ReparaturBearbeitenAktuell_Click(object sender, RoutedEventArgs e)
    {
      Fenster.FormReparatur form = new Fenster.FormReparatur(_ListeReparaturAktuell.AktDatensatz, _ListeBediener);
      if (form.ShowDialog() ?? false)
        _ListeReparaturAktuell.AktSichern(EnumStatusDatenabgleich.Geaendert);
      else
        _ListeReparaturAktuell.Reload(form.Reparatur);
    }

    private void ReparaturBearbeitenAuswahl_Click(object sender, RoutedEventArgs e)
    {
      Fenster.FormReparatur form = new Fenster.FormReparatur(_ListeReparaturAuswahl.AktDatensatz, _ListeBediener);
      if (form.ShowDialog() ?? false)
        _ListeReparaturAuswahl.AktSichern(EnumStatusDatenabgleich.Geaendert);
      else
        _ListeReparaturAuswahl.Reload(form.Reparatur);
    }

    private async void ReparaturBeenden_Click(object sender, RoutedEventArgs e)
    {
      var reparatur = _ListeReparaturAktuell.AktDatensatz;
      var anzeigeText = $"Maschine {reparatur.eMaschine.MaschinenName} mit Ereigniss {reparatur.Ereigniss} abmelden?";

      JgMaschineLib.Zeit.FormAuswahlDatumZeit form = new FormAuswahlDatumZeit("Abmeldung", anzeigeText, DateTime.Now);
      if (form.ShowDialog() ?? false)
      {
        var repAustragen = reparatur.sAnmeldungen.Where(w => w.IstAktiv).ToList();
        foreach (var bediener in repAustragen)
        {
          bediener.Abmeldung = form.DatumZeit;
          DbSichern.AbgleichEintragen(bediener.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
        reparatur.VorgangEnde = form.DatumZeit;
        reparatur.eMaschine.fAktivReparatur = null;
        _ListeReparaturAktuell.AktSichern(EnumStatusDatenabgleich.Geaendert);
        _ListeReparaturAktuell.Remove(reparatur);

        await TreeListAktualisieren();
      }
    }

    private void ReparaturAnmeldungBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var cvs = (CollectionViewSource)FindResource("vsReparaturAuswahlBediener");
      var anmeldung = (tabAnmeldungReparatur)cvs.View.CurrentItem;
      string anzeigeText = $"Reparaturzeiten für den Bediener {anmeldung.eBediener.Name} bearbeiten.";

      var form = new JgMaschineLib.Zeit.FormAuswahlDatumVonBis("Anmeldung Maschine bearbeiten", anzeigeText, anmeldung.Anmeldung, anmeldung.Abmeldung ?? DateTime.Now);
      if (form.ShowDialog() ?? false)
      {
        anmeldung.Anmeldung = form.DatumVon;
        anmeldung.Abmeldung = form.DatumBis;
        DbSichern.AbgleichEintragen(anmeldung.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        _ListeReparaturAuswahl.Db.SaveChanges();
        cvs.View.Refresh();
      }
    }

    private void ReparaturAnmeldungAbmeldung_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = (tabAnmeldungReparatur)(FindResource("vsReparaturAktuellBediener") as CollectionViewSource).View.CurrentItem;
      string anzeigeText = $"Möchten Sie den Bediener {anmeldung.eBediener.Name} von der Reparatur abmelden ?";

      var form = new JgFormDatumZeit();
      if (form.Anzeigen("Abmeldung", anzeigeText, DateTime.Now))
        ReparaturBedienerAbmelden(anmeldung, form.Datum);
    }

    private void ReparaturBedienerAbmelden(tabAnmeldungReparatur AnmeldungReparatur, DateTime ZeitAbmeldung)
    {
      if (AnmeldungReparatur != null)
      {
        AnmeldungReparatur.Abmeldung = ZeitAbmeldung;
        DbSichern.AbgleichEintragen(AnmeldungReparatur.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        _ListeReparaturAktuell.Db.SaveChanges();
      }
    }

    #endregion

    #region Anmeldungen Maschine

    private async void ExecuteAnmeldungBenutzerAnmeldung(object sender, ExecutedRoutedEventArgs e)
    {
      var bediener = (tabBediener)_VsBedienerAnmeldung.View.CurrentItem;
      var kontrAngemeldet = _Maschine.sAktiveAnmeldungen.FirstOrDefault(f => (f.fBediener == bediener.Id));

      if (kontrAngemeldet != null)
        MessageBox.Show($"{bediener.Name} ist bereits an Maschine: '{kontrAngemeldet.eMaschine.MaschinenName}' angemeldet! Bitte erst an dieser Maschine abmelden.", "Information !", MessageBoxButton.OK, MessageBoxImage.Warning);
      else
      {
        JgFormDatumZeit form = new JgFormDatumZeit();
        if (form.Anzeigen("Anmeldung", $"Geben Sie die Zeit an, in welcher sich der '{bediener.Name}' angemeldet hat.", DateTime.Now))
        {
          var anmeldung = new tabAnmeldungMaschine()
          {
            Id = Guid.NewGuid(),
            fBediener = bediener.Id,
            fMaschine = _Maschine.Id,
            Anmeldung = form.Datum,
            ManuelleAnmeldung = true,
            ManuelleAbmeldung = true,
            fAktivMaschine = _Maschine.Id
          };
          _ListeAnmeldungAktuell.Add(anmeldung);

          if (_Maschine.fAktivReparatur != null)
          {
            var anmeldungReparatur = new tabAnmeldungReparatur()
            {
              Id = Guid.NewGuid(),
              fReparatur = (Guid)_Maschine.fAktivReparatur,
              fBediener = anmeldung.fBediener,
              Anmeldung = DateTime.Now,
            };
            DbSichern.AbgleichEintragen(anmeldungReparatur.DatenAbgleich, EnumStatusDatenabgleich.Neu);
            _ListeReparaturAktuell.Db.tabAnmeldungReparaturSet.Add(anmeldungReparatur);
            _ListeReparaturAktuell.Db.SaveChanges();
          }

          await TreeListAktualisieren();
        }
      }
    }

    private async void AnmeldungBenutzerAbmelden_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = _ListeAnmeldungAktuell.AktDatensatz;
      string anzeigeText = $"Möchten Sie den Bediener {anmeldung.eBediener.Name} von der Maschine {anmeldung.eMaschine.MaschinenName} abmelden ?";

      JgFormDatumZeit form = new JgFormDatumZeit();
      if (form.Anzeigen("Abmeldung", anzeigeText, DateTime.Now))
      {
        anmeldung.Abmeldung = form.Datum;
        anmeldung.ManuelleAbmeldung = true;
        anmeldung.eAktivMaschine = null;
        _ListeAnmeldungAktuell.AktSichern(EnumStatusDatenabgleich.Geaendert);
        _ListeAnmeldungAktuell.Remove(anmeldung);

        tabAnmeldungReparatur rep = _ListeReparaturAktuell.SelectMany(s => s.sAnmeldungen).FirstOrDefault(f => f.fBediener == anmeldung.fBediener);
        if (rep != null)
        {
          rep.Abmeldung = form.Datum;
          DbSichern.AbgleichEintragen(rep.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
          _ListeReparaturAktuell.Db.SaveChanges();
        }

        await TreeListAktualisieren();
      }
    }

    private void AnmeldungBenutzerBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = _ListeAnmeldungAuswahl.AktDatensatz;
      var anz = $"Korrektur der Arbeitszeit für den Mitarbeiter {anmeldung.eBediener.Name}.";
      var form = new FormAuswahlDatumVonBis("Berichtigung Arbeitszeit", anz, anmeldung.Anmeldung, (DateTime)anmeldung.Abmeldung);
      if (form.ShowDialog() ?? false)
      {
        if (form.DatumVon != anmeldung.Anmeldung)
        {
          anmeldung.Anmeldung = form.DatumVon;
          anmeldung.ManuelleAnmeldung = true;
        }
        if (form.DatumBis != anmeldung.Abmeldung)
        {
          anmeldung.Abmeldung = form.DatumBis;
          anmeldung.ManuelleAbmeldung = true;
        }
        _ListeAnmeldungAuswahl.AktSichern(EnumStatusDatenabgleich.Geaendert);
        _ListeAnmeldungAuswahl.Refresh();
      }
    }

    private void BenutzerAbmelden(tabAnmeldungMaschine Anmeldung, DateTime AbmeldeZeit)
    {
      if (Anmeldung != null)
      {
        var anmeldung = _ListeAnmeldungAktuell.FirstOrDefault(f => f.Id == Anmeldung.Id);
        anmeldung.Abmeldung = AbmeldeZeit;
        anmeldung.ManuelleAbmeldung = true;
        anmeldung.eAktivMaschine = null;
        DbSichern.AbgleichEintragen(anmeldung.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        _ListeAnmeldungAktuell.Db.SaveChanges();

        var anmeldungReparatur = _ListeReparaturAktuell.SelectMany(m => m.sAnmeldungen).FirstOrDefault(f => f.fBediener == anmeldung.fBediener);
        ReparaturBedienerAbmelden(anmeldungReparatur, AbmeldeZeit);
      }
    }

    #endregion

    #region Arbeitszeit verwalten

    private void ArbeitszeitAnmeldung_Click(object sender, RoutedEventArgs e)
    {
      var bediener = (tabBediener)(FindResource("vsBedienerArbeitszeit") as CollectionViewSource).View.CurrentItem;

      if (bediener.eAktivArbeitszeit != null)
        MessageBox.Show($"Bediener {bediener.Name} bereits angemeldet !", "Informaation !", MessageBoxButton.OK, MessageBoxImage.Warning);
      else
      {
        string anzeigeText = string.Format("Mitarbeiter {0} im Betrieb anmelden ?", bediener.Name);
        FormAuswahlDatumZeit form = new FormAuswahlDatumZeit("Anmeldung", anzeigeText, DateTime.Now);
        if (form.ShowDialog() ?? false)
        {
          var arbeitszeit = new JgMaschineData.tabArbeitszeit()
          {
            Id = Guid.NewGuid(),
            fBediener = bediener.Id,
            fStandort = _Standort.Id,
            Anmeldung = form.DatumZeit,
            ManuelleAnmeldung = true,
            ManuelleAbmeldung = true,
          };
          _ListeArbeitszeitAktuell.Add(arbeitszeit);

          bediener.fAktivArbeitszeit = arbeitszeit.Id;
          _ListeBediener.DsSichern(bediener, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    private async void ArbeitszeitAbmeldung_Click(object sender, RoutedEventArgs e)
    {
      var arbeitszeit = _ListeArbeitszeitAktuell.AktDatensatz;

      JgFormDatumZeit form = new JgFormDatumZeit();
      if (form.Anzeigen("Abmeldung Bediener", $"Möchten Sie '{arbeitszeit.eBediener.Name}' abmelden ?", DateTime.Now))
      {
        arbeitszeit.Abmeldung = form.Datum;
        arbeitszeit.ManuelleAbmeldung = true;

        arbeitszeit.eBediener.fAktivArbeitszeit = null;
        DbSichern.AbgleichEintragen(arbeitszeit.eBediener.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);

        _ListeArbeitszeitAktuell.AktSichern(EnumStatusDatenabgleich.Geaendert);
        _ListeArbeitszeitAktuell.Remove(arbeitszeit);

        var anmeldungAktuell = _ListeAnmeldungAktuell.FirstOrDefault(f => f.fBediener == arbeitszeit.fBediener);
        if (anmeldungAktuell != null)
        {
          var anzeigeText = $"{arbeitszeit.eBediener.Name} ist noch an Maschine '{anmeldungAktuell.eMaschine.MaschinenName}' angemeldet. Von dieser Maschine abmelden?";
          var erg = MessageBox.Show(anzeigeText, "Abfrage Abmeldung", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
          if (erg == MessageBoxResult.Yes)
            BenutzerAbmelden(anmeldungAktuell, form.Datum);
          await TreeListAktualisieren();
        }
      }
    }

    private void ArbeitszeitBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var arbeitszeit = _ListeArbeitszeitAuswahl.AktDatensatz;
      var anz = $"Korrektur der Arbeitszeit für den Mitarbeiter {arbeitszeit.eBediener.Name}.";
      var form = new FormAuswahlDatumVonBis("Berichtigung Arbeitszeit", anz, arbeitszeit.Anmeldung, arbeitszeit.Abmeldung ?? DateTime.Now);
      if (form.ShowDialog() ?? false)
      {
        if (form.DatumVon != arbeitszeit.Anmeldung)
        {
          arbeitszeit.Anmeldung = form.DatumVon;
          arbeitszeit.ManuelleAnmeldung = true;
        }
        if (form.DatumBis != arbeitszeit.Abmeldung)
        {
          arbeitszeit.Abmeldung = form.DatumBis;
          arbeitszeit.ManuelleAbmeldung = true;
        }
        _ListeArbeitszeitAuswahl.AktSichern(EnumStatusDatenabgleich.Geaendert);
        _ListeArbeitszeitAuswahl.Refresh();
      }
    }

    #endregion

    private async void ButtonOptionen_Click(object sender, RoutedEventArgs e)
    {
      List<tabStandort> standorte;
      using (var db = new JgModelContainer())
      {
        standorte = await db.tabStandortSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.Bezeichnung).ToListAsync();
      }

      Fenster.FormOptionen form = new Fenster.FormOptionen(standorte);
      if (form.ShowDialog() ?? false)
      {
        _Standort = form.StandOrt;
        using (var db = new JgMaschineData.JgModelContainer())
        {
          _VsMaschine.Source = db.tabMaschineSet.Where(w => (w.fStandort == _Standort.Id) && (w.Status != JgMaschineData.EnumStatusMaschine.Stillgelegt)).ToList();
        }
        treeViewMaschinen.UpdateLayout();
        if (treeViewMaschinen.Items.Count > 0)
          (treeViewMaschinen.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem).IsSelected = true;

        await _ListeArbeitszeitAktuell.DatenGenerierenAsync();
        await _ListeArbeitszeitAktuell.Db.tabBedienerSet.Where(w => w.fStandort == _Standort.Id).ToListAsync();
        await _ListeAnmeldungAktuell.DatenGenerierenAsync();
        await _ListeAnmeldungAktuell.Db.tabBedienerSet.Where(w => w.fStandort == _Standort.Id).ToListAsync();
        await _ListeAnmeldungAktuell.Db.tabMaschineSet.Where(w => w.fStandort == _Standort.Id).ToListAsync();
        await _ListeReparaturAktuell.DatenGenerierenAsync();
        await _ListeReparaturAktuell.Db.tabBedienerSet.Where(w => w.fStandort == _Standort.Id).ToListAsync();
        await _ListeReparaturAktuell.Db.tabMaschineSet.Where(w => w.fStandort == _Standort.Id).ToListAsync();
      }
    }
    private void Window_Closed(object sender, EventArgs e)
    {
      Properties.Settings.Default.Save();
    }
    private async void btnAuswahlAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      switch ((sender as Button).Tag.ToString())
      {
        case "1": await _ListeArbeitszeitAuswahl.DatenGenerierenAsync(); break;
        case "2": await _ListeAnmeldungAuswahl.DatenGenerierenAsync(); break;
        case "3": await _ListeBauteilAuswahl.DatenGenerierenAsync(); break;
        case "4": await _ListeReparaturAuswahl.DatenGenerierenAsync(); break;
      }
    }
    private void btnDrucken_Click(object sender, RoutedEventArgs e)
    {
      var auswahl = (EnumFilterAuswertung)Enum.Parse(typeof(EnumFilterAuswertung), (sender as Button).Tag.ToString()[0].ToString());
      var vorgang = Convert.ToInt32((sender as Button).Tag.ToString().Substring(1));  // 1 - Anzeigen, 2 - Drucken, 3 - Design, 4 - Neuer Report

      if (vorgang < 4)
      {
        switch (auswahl)
        {
          case JgMaschineData.EnumFilterAuswertung.Arbeitszeit: _AktAuswertung = (tabAuswertung)_VsAuswertungArbeitszeit.View.CurrentItem; break;
          case JgMaschineData.EnumFilterAuswertung.Anmeldung: _AktAuswertung = (tabAuswertung)_VsAuswertungAnmeldung.View.CurrentItem; break;
          case JgMaschineData.EnumFilterAuswertung.Bauteil: _AktAuswertung = (tabAuswertung)_VsAuswertungBauteil.View.CurrentItem; break;
          case JgMaschineData.EnumFilterAuswertung.Reparatur: _AktAuswertung = (tabAuswertung)_VsAuswertungReparatur.View.CurrentItem; break;
        }

        if (_AktAuswertung == null)
        {
          MessageBox.Show("Es wurde kein Report ausgewählt.", "Fehler !", MessageBoxButton.OK, MessageBoxImage.Information);
          return;
        }

        _Report.Clear();
        if (_AktAuswertung.Report == null)
          vorgang = 3;
        else
        {
          var mem = new MemoryStream(_AktAuswertung.Report);
          _Report.Load(mem);
        }
      }

      switch (auswahl)
      {
        case JgMaschineData.EnumFilterAuswertung.Arbeitszeit:
          if (tcArbeitszeit.SelectedIndex == 0)
            _Report.RegisterData(_ListeArbeitszeitAktuell, "Daten");
          else
          {
            _Report.RegisterData(_ListeArbeitszeitAuswahl, "Daten");
            _Report.SetParameterValue("DatumVon", _DzArbeitszeitVon);
            _Report.SetParameterValue("DatumBis", _DzArbeitszeitBis);
          }
          _Report.SetParameterValue("IstAktuell", tcArbeitszeit.SelectedIndex == 0);
          break;
        case JgMaschineData.EnumFilterAuswertung.Anmeldung:
          if (tcAnmeldungen.SelectedIndex == 0)
            _Report.RegisterData(_ListeAnmeldungAktuell, "Daten");
          else
          {
            _Report.RegisterData(_ListeAnmeldungAuswahl, "Daten");
            _Report.SetParameterValue("DatumVon", _DzAnmeldungVon);
            _Report.SetParameterValue("DatumBis", _DzAnmeldungBis);
          }
          _Report.SetParameterValue("IstAktuell", tcAnmeldungen.SelectedIndex == 0);
          break;
        case JgMaschineData.EnumFilterAuswertung.Bauteil:
          _Report.RegisterData(_ListeBauteilAuswahl, "Daten");
          _Report.SetParameterValue("DatumVon", _DzBauteilVon);
          _Report.SetParameterValue("DatumBis", _DzBauteilBis);
          break;
        case JgMaschineData.EnumFilterAuswertung.Reparatur:
          if (tcReparaturen.SelectedIndex == 0)
            _Report.RegisterData(_ListeReparaturAktuell, "Daten");
          else
          {
            _Report.RegisterData(_ListeReparaturAuswahl, "Daten");
            _Report.SetParameterValue("DatumVon", _DzReparaturVon);
            _Report.SetParameterValue("DatumBis", _DzReparaturBis);
          }
          _Report.SetParameterValue("IstAktuell", tcReparaturen.SelectedIndex == 0);
          break;
        default:
          break;
      }

      if (vorgang == 4)
      {
        Fenster.FormNeuerReport form = new Fenster.FormNeuerReport();
        if (form.ShowDialog() ?? false)
        {
          string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
          _AktAuswertung = new JgMaschineData.tabAuswertung()
          {
            Id = Guid.NewGuid(),
            FilterAuswertung = auswahl,
            ReportName = form.ReportName,
            ErstelltDatum = DateTime.Now,
            ErstelltName = username,
            GeaendertDatum = DateTime.Now,
            GeaendertName = username
          };
          _ListeAuswertung.Add(_AktAuswertung);

          switch (auswahl)
          {
            case JgMaschineData.EnumFilterAuswertung.Arbeitszeit: _VsAuswertungArbeitszeit.View.MoveCurrentTo(_AktAuswertung); break;
            case JgMaschineData.EnumFilterAuswertung.Anmeldung: _VsAuswertungAnmeldung.View.MoveCurrentTo(_AktAuswertung); break;
            case JgMaschineData.EnumFilterAuswertung.Bauteil: _VsAuswertungBauteil.View.MoveCurrentTo(_AktAuswertung); break;
            case JgMaschineData.EnumFilterAuswertung.Reparatur: _VsAuswertungReparatur.View.MoveCurrentTo(_AktAuswertung); break;
          }

          _Report.Design();
        }
      }
      else
        switch (vorgang)
        {
          case 1: _Report.Show(); break;
          case 2: _Report.Print(); break;
          case 3: _Report.Design(); break;
        }
    }
    private async void TabelleAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      switch ((sender as Button).Tag.ToString())
      {
        case "0": await TreeListAktualisieren(); break;
        case "1": await 
            _ListeArbeitszeitAktuell.DatenGenerierenAsync();

          break;
        case "2": await _ListeAnmeldungAktuell.DatenGenerierenAsync(); break;
        case "4": await _ListeReparaturAktuell.DatenGenerierenAsync(); break;
      }
    }

    private void button_Click(object sender, RoutedEventArgs e)
    {
      (FindResource("vsReparaturAktuell") as CollectionViewSource).View.Refresh();
    }
  }
}
