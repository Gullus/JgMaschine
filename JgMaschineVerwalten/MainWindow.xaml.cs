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
using JgMaschineData;
using JgMaschineLib;
using JgMaschineLib.Zeit;
using JgMaschineVerwalten.Commands;

namespace JgMaschineVerwalten
{
  public partial class MainWindow : Window
  {
    private JgModelContainer _Db;
    private tabStandort _Standort;

    private EnumFilterAuswertung _Auswahl { get { return (EnumFilterAuswertung)Convert.ToByte((tcVerwaltung.SelectedItem as TabItem).Tag); } }

    #region Bediener

    private tabBediener _AuswahlBedienerArbeitszeit { get { return (tabBediener)(FindResource("vsBedienerArbeitszeit") as CollectionViewSource).View.CurrentItem; } }
    private tabBediener _AuswahlBedienerAnmeldung { get { return (tabBediener)(FindResource("vsBedienerAnmeldung") as CollectionViewSource).View.CurrentItem; } }

    #endregion

    private JgEntityAuto _DatenPool;
    private JgEntityTab<tabBediener> _JgBediener { get { return (JgEntityTab<tabBediener>)_DatenPool.Tabs[JgEntityAuto.TabArt.Bediener]; } }
    private JgEntityTab<tabMaschine> _JgMaschine { get { return (JgEntityTab<tabMaschine>)_DatenPool.Tabs[JgEntityAuto.TabArt.Maschine]; } }
    private JgEntityTab<tabArbeitszeit> _JgArbeitszeit { get { return (JgEntityTab<tabArbeitszeit>)_DatenPool.Tabs[JgEntityAuto.TabArt.Arbeitszeit]; } }
    private JgEntityTab<tabAnmeldungMaschine> _JgAnmeldung { get { return (JgEntityTab<tabAnmeldungMaschine>)_DatenPool.Tabs[JgEntityAuto.TabArt.Anmeldung]; } }
    private JgEntityTab<tabReparatur> _JgReparatur { get { return (JgEntityTab<tabReparatur>)_DatenPool.Tabs[JgEntityAuto.TabArt.Reparatur]; } }
    private JgEntityTab<tabAnmeldungReparatur> _JgRepAnmeldung { get { return (JgEntityTab<tabAnmeldungReparatur>)_DatenPool.Tabs[JgEntityAuto.TabArt.RepAnmeldung]; } }
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

    #region Report Ausertungen

    private JgEntityView<tabAuswertung> _ListeAuswertung;
    private CollectionViewSource _VsAuswertungArbeitszeit { get { return (CollectionViewSource)this.FindResource("vsAuswertungArbeitszeit"); } }
    private CollectionViewSource _VsAuswertungAnmeldung { get { return (CollectionViewSource)this.FindResource("vsAuswertungAnmeldung"); } }
    private CollectionViewSource _VsAuswertungBauteil { get { return (CollectionViewSource)this.FindResource("vsAuswertungBauteil"); } }
    private CollectionViewSource _VsAuswertungReparatur { get { return (CollectionViewSource)this.FindResource("vsAuswertungReparatur"); } }

    #endregion

    private JgEntityView<tabArbeitszeit> _ListeArbeitszeitAuswahl;
    private JgDatumZeit _DzArbeitszeitVon { get { return (JgDatumZeit)FindResource("dzArbeitszeitVon"); } }
    private JgDatumZeit _DzArbeitszeitBis { get { return (JgDatumZeit)FindResource("dzArbeitszeitBis"); } }

    private JgEntityView<tabAnmeldungMaschine> _ListeAnmeldungAuswahl;
    private JgDatumZeit _DzAnmeldungVon { get { return (JgDatumZeit)FindResource("dzAnmeldungVon"); } }
    private JgDatumZeit _DzAnmeldungBis { get { return (JgDatumZeit)FindResource("dzAnmeldungBis"); } }

    private JgEntityView<tabBauteil> _ListeBauteilAuswahl;
    private JgDatumZeit _DzBauteilVon { get { return (JgDatumZeit)FindResource("dzBauteilVon"); } }
    private JgDatumZeit _DzBauteilBis { get { return (JgDatumZeit)FindResource("dzBauteilBis"); } }

    private JgEntityView<tabReparatur> _ListeReparaturAuswahl;
    private JgDatumZeit _DzReparaturVon { get { return (JgDatumZeit)FindResource("dzReparaturVon"); } }
    private JgDatumZeit _DzReparaturBis { get { return (JgDatumZeit)FindResource("dzReparaturBis"); } }

    private FastReport.Report _Report;
    private tabAuswertung _AktAuswertung = null;
    private FastReport.EnvironmentSettings _ReportSettings = new FastReport.EnvironmentSettings();

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _Db = new JgModelContainer();
      _Standort = await _Db.tabStandortSet.FindAsync(Properties.Settings.Default.IdStandort);
      if (_Standort == null)
        _Standort = await _Db.tabStandortSet.FirstOrDefaultAsync();

      tblStandort.Text = _Standort.Bezeichnung;

      await InitListen();

      CommandBindings.Add(new CommandBinding(MyCommands.AnmeldungReparaturBediener, ExecuteAnmeldungBedienerReparatur, (sen, erg) =>
      {
        erg.CanExecute = _JgReparatur.Current != null;
      }));

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
          _ListeAuswertung.DsSave();
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
    }

    private async Task InitListen()
    {
      _DatenPool = new JgEntityAuto(Properties.Settings.Default.VerbindungsString, 5)
      {
        TimerAusgeloest = () =>
        {
          tblDatum.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
          TreeListMaschinRefresh();
        }
      };

      var con = _DatenPool.DbVerbindung();
      if (con == null)
        return;

      try
      {
        var eBediener = new JgEntityTab<tabBediener>(_DatenPool.Db)
        {
          AbfrageSqlString = () =>
          {
            if (_Standort == null)
              return JgEntityTab.IstNull;

            return "Select Id, DatenAbgleich_Datum "
              + "From tabBedienerSet "
              + "Where (fStandort = '" + _Standort.Id + "') AND (Status <> " + EnumStatusBediener.Stillgelegt.ToString("d") + ")";
          }
        };
        eBediener.SetIdis(con);
        if (eBediener.Idis != null)
          eBediener.Daten = await _DatenPool.Db.tabBedienerSet.Where(w => eBediener.Idis.Contains(w.Id)).ToListAsync();
        _DatenPool.Tabs.Add(JgEntityAuto.TabArt.Bediener, eBediener);

        (FindResource("vsBedienerArbeitszeit") as CollectionViewSource).Source = eBediener.Daten;
        (FindResource("vsBedienerAnmeldung") as CollectionViewSource).Source = eBediener.Daten;

        var eMaschine = new JgEntityTab<tabMaschine>(_DatenPool.Db)
        {
          ViewSource = (CollectionViewSource)FindResource("vsMaschine"),
          AbfrageSqlString = () =>
          {
            if (_Standort == null)
              return JgEntityTab.IstNull;

            return "SELECT Id, DatenAbgleich_Datum "
              + "FROM tabMaschineSet "
              + "WHERE (fStandort = '" + _Standort.Id + "') AND (Status <> " + EnumStatusMaschine.Stillgelegt.ToString("d") + ")";
          }
        };
        eMaschine.SetIdis(con);
        if (eMaschine.Idis != null)
          eMaschine.Daten = await _DatenPool.Db.tabMaschineSet.Where(w => eMaschine.Idis.Contains(w.Id)).ToListAsync();
        _DatenPool.Tabs.Add(JgEntityAuto.TabArt.Maschine, eMaschine);
        //(FindResource("vsMaschine") as CollectionViewSource).Source = eMaschine.Daten.ToList();

        var eArbeitszeit = new JgEntityTab<tabArbeitszeit>(_DatenPool.Db)
        {
          ViewSource = (CollectionViewSource)FindResource("vsArbeitszeitAktuell"),
          Tabellen = new DataGrid[] { dgArbeitszeitAktuell },
          AbfrageSqlString = () =>
          {
            if (_Standort == null)
              return JgEntityTab.IstNull;

            return "SELECT arbeitszeit.Id, arbeitszeit.DatenAbgleich_Datum "
              + "FROM tabBedienerSet AS bediener INNER JOIN "
              + "tabArbeitszeitSet AS arbeitszeit ON bediener.fAktivArbeitszeit = arbeitszeit.Id "
              + "WHERE(bediener.fStandort = '" + _Standort.Id + "')";
          }
        };
        eArbeitszeit.SetIdis(con);
        if (eArbeitszeit.Idis != null)
          eArbeitszeit.Daten = await _DatenPool.Db.tabArbeitszeitSet.Where(w => eArbeitszeit.Idis.Contains(w.Id)).ToListAsync();
        _DatenPool.Tabs.Add(JgEntityAuto.TabArt.Arbeitszeit, eArbeitszeit);

        var eAnmeldung = new JgEntityTab<tabAnmeldungMaschine>(_DatenPool.Db)
        {
          ViewSource = (CollectionViewSource)FindResource("vsAnmeldungAktuell"),
          Tabellen = new DataGrid[] { dgAnmeldungAktuell },
          AbfrageSqlString = () =>
          {
            if (_JgMaschine.Idis == null)
              return JgEntityTab.IstNull;
            else
              return "SELECT Id, DatenAbgleich_Datum "
                + "FROM tabAnmeldungMaschineSet "
                + "WHERE fAktivMaschine In (" + JgEntityTab.IdisInString(_JgMaschine.Idis) + ")";
          }
        };
        eAnmeldung.SetIdis(con);
        if (eAnmeldung.Idis != null)
          eAnmeldung.Daten = await _DatenPool.Db.tabAnmeldungMaschineSet.Where(w => eAnmeldung.Idis.Contains(w.Id)).ToListAsync();
        _DatenPool.Tabs.Add(JgEntityAuto.TabArt.Anmeldung, eAnmeldung);

        var eReparatur = new JgEntityTab<tabReparatur>(_DatenPool.Db)
        {
          ViewSource = (CollectionViewSource)FindResource("vsReparaturAktuell"),
          Tabellen = new DataGrid[] { dgReparaturAktuell },
          AbfrageSqlString = () =>
          {
            if (_JgMaschine.Idis != null)
            {
              var ids = _JgMaschine.Daten.Where(w => (w.fAktivReparatur != null)).Select(s => (Guid)s.fAktivReparatur).ToArray();
              if (ids.Length > 0)
                return "SELECT Id, DatenAbgleich_Datum "
                  + "FROM tabReparaturSet "
                  + "WHERE Id IN (" + JgEntityTab.IdisInString(ids) + ")";
            }
            return JgEntityTab.IstNull;
          }
        };
        eReparatur.SetIdis(con);
        if (eReparatur.Idis != null)
          eReparatur.Daten = await _DatenPool.Db.tabReparaturSet.Where(w => eReparatur.Idis.Contains(w.Id)).ToListAsync();
        _DatenPool.Tabs.Add(JgEntityAuto.TabArt.Reparatur, eReparatur);

        var eRepAnmeldung = new JgEntityTab<tabAnmeldungReparatur>(_DatenPool.Db, false)
        {
          ViewSource = (CollectionViewSource)FindResource("vsReparaturAktuellBediener"),
          AbfrageSqlString = () =>
          {
            if (_JgReparatur.Idis != null)
              return "SELECT Id, DatenAbgleich_Datum "
                + "FROM tabAnmeldungReparaturSet "
                + "WHERE fReparatur In (" + JgEntityTab.IdisInString(_JgReparatur.Idis) + ")";

            return JgEntityTab.IstNull;
          }
        };
        eRepAnmeldung.SetIdis(con);
        if (eRepAnmeldung.Idis != null)
          eRepAnmeldung.Daten = await _DatenPool.Db.tabAnmeldungReparaturSet.Where(w => eRepAnmeldung.Idis.Contains(w.Id)).ToListAsync();
        _DatenPool.Tabs.Add(JgEntityAuto.TabArt.RepAnmeldung, eRepAnmeldung);

      }
      catch (Exception f)
      {
        Helper.Protokoll($"Fehler beim erstellen des Datenpools !\nGrund: {f.Message}");
      }
      finally
      {
        con.Close();
      }

      var von = DateTime.Now.Date;
      var bis = new DateTime(von.Year, von.Month, von.Day, 23, 59, 59);

      _DzArbeitszeitVon.DatumZeit = von;
      _DzArbeitszeitBis.DatumZeit = bis;

      _ListeArbeitszeitAuswahl = new JgEntityView<tabArbeitszeit>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsArbeitszeitAuswahl"),
        Tabellen = new DataGrid[] { dgArbeitszeitAuswahl },
        DatenErstellen = (db) =>
        {
          return db.tabArbeitszeitSet.Where(w => (w.fStandort == _Standort.Id) && (w.Anmeldung >= _DzArbeitszeitVon.DatumZeit) && ((w.Anmeldung <= _DzArbeitszeitBis.DatumZeit)))
            .OrderBy(o => o.Anmeldung).ToList();
        }
      };

      _DzAnmeldungVon.DatumZeit = von;
      _DzAnmeldungBis.DatumZeit = bis;

      _ListeAnmeldungAuswahl = new JgEntityView<tabAnmeldungMaschine>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsAnmeldungAuswahl"),
        Tabellen = new DataGrid[] { dgAnmeldungAuswahl },
        DatenErstellen = (db) =>
        {
          var idis = (_DatenPool.Tabs[JgEntityAuto.TabArt.Maschine] as JgEntityTab<tabMaschine>).Idis;
          return db.tabAnmeldungMaschineSet.Where(w => idis.Contains(w.fMaschine) && (w.Anmeldung >= _DzAnmeldungVon.DatumZeit) && ((w.Anmeldung <= _DzAnmeldungBis.DatumZeit)))
            .OrderBy(o => o.Anmeldung).ToList();
        }
      };

      // Bauteile initialisieren ****************************

      _DzBauteilVon.DatumZeit = von;
      _DzBauteilBis.DatumZeit = bis;

      _ListeBauteilAuswahl = new JgEntityView<tabBauteil>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsBauteilAuswahl"),
        Tabellen = new DataGrid[] { dgBauteilAuswahl },
        DatenErstellen = (db) =>
        {
          if (_Maschine == null)
          {
            Helper.Protokoll("Bitte Maschine in linker Tabelle auswahlen !", Helper.ProtokollArt.Warnung);
            return null;
          }
          else
            return db.tabBauteilSet.Where(w => (w.fMaschine == _Maschine.Id) && (w.DatumStart >= _DzBauteilVon.DatumZeit) && (w.DatumStart <= _DzBauteilBis.DatumZeit))
            .Include(i => i.sBediener)
            .OrderBy(o => o.DatumStart).ToList();
        }
      };

      _DzReparaturVon.DatumZeit = von;
      _DzReparaturBis.DatumZeit = bis;

      _ListeReparaturAuswahl = new JgEntityView<tabReparatur>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsReparaturAuswahl"),
        Tabellen = new DataGrid[] { dgReparaturAuswahl },
        DatenErstellen = (db) =>
        {
          var idis = (_DatenPool.Tabs[JgEntityAuto.TabArt.Maschine] as JgEntityTab<tabMaschine>).Idis;
          return db.tabReparaturSet.Where(w => idis.Contains(w.fMaschine) && (w.VorgangBeginn >= _DzReparaturVon.DatumZeit) && (w.VorgangBeginn <= _DzReparaturBis.DatumZeit))
            .Include(i => i.sAnmeldungen).Include(i => i.sAnmeldungen.Select(s => s.eBediener))
            .OrderByDescending(o => o.VorgangBeginn);
        }
      };

      // Auswertung initialisieren ********************************

      _ListeAuswertung = new JgEntityView<tabAuswertung>();
      _ListeAuswertung.Daten = await _Db.tabAuswertungSet.Where(w => w.FilterAuswertung != EnumFilterAuswertung.Allgemein)
        .OrderBy(o => o.ReportName).ToListAsync();

      _VsAuswertungArbeitszeit.Source = _ListeAuswertung.Daten;
      _VsAuswertungArbeitszeit.Filter += (sen, erg) => erg.Accepted = (erg.Item as tabAuswertung).FilterAuswertung == EnumFilterAuswertung.Arbeitszeit;
      _VsAuswertungAnmeldung.Source = _ListeAuswertung.Daten;
      _VsAuswertungAnmeldung.Filter += (sen, erg) => erg.Accepted = (erg.Item as tabAuswertung).FilterAuswertung == EnumFilterAuswertung.Anmeldung;
      _VsAuswertungBauteil.Source = _ListeAuswertung.Daten;
      _VsAuswertungBauteil.Filter += (sen, erg) => erg.Accepted = (erg.Item as tabAuswertung).FilterAuswertung == EnumFilterAuswertung.Bauteil;
      _VsAuswertungReparatur.Source = _ListeAuswertung.Daten;
      _VsAuswertungReparatur.Filter += (sen, erg) => erg.Accepted = (erg.Item as tabAuswertung).FilterAuswertung == EnumFilterAuswertung.Reparatur;
    }

    private void TreeListMaschinRefresh()
    {
      Guid? idMaschine = null;
      Guid? idAnmeldung = null;
      if (_Maschine != null)
      {
        idMaschine = _Maschine.Id;
        if (treeViewMaschinen.SelectedItem is tabAnmeldungMaschine)
          idAnmeldung = (treeViewMaschinen.SelectedItem as tabAnmeldungMaschine).Id;
      }

      _JgMaschine.ViewSource?.View?.Refresh();

      if (idMaschine != null)
      {
        if (idAnmeldung != null) // Als ersttes nach Benutzer suchen
        {
          foreach (var dsMaschine in treeViewMaschinen.Items)
          {
            var itemMaschine = (TreeViewItem)treeViewMaschinen.ItemContainerGenerator.ContainerFromItem(dsMaschine);
            foreach (var dsAnmeldung in itemMaschine.Items)
            {
              if ((dsAnmeldung as tabAnmeldungMaschine).Id == idAnmeldung)
              {
                (itemMaschine.ItemContainerGenerator.ContainerFromItem(dsAnmeldung) as TreeViewItem).IsSelected = true;
                return;
              }
            }
          }
        }
        foreach (var dsMaschine in treeViewMaschinen.Items)
        {
          if ((dsMaschine as tabMaschine).Id == idMaschine)
          {
            (treeViewMaschinen.ItemContainerGenerator.ContainerFromItem(dsMaschine) as TreeViewItem).IsSelected = true;
            return;
          }
        }
      }
    }

    #region Reparaturen ************************************************************

    private void NeueReparaturErstellen_Click(object sender, RoutedEventArgs e)
    {
      if (_Maschine == null)
      {
        Helper.Protokoll("Bitte Maschine in linker Tabelle auswahlen !", Helper.ProtokollArt.Warnung);
        return;
      }

      if (_Maschine.fAktivReparatur != null)
      {
        MessageBox.Show($"Die Maschine {_Maschine.MaschinenName} ist bereits im Reparaturmodus.", "Information !", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
      }

      var form = new Fenster.FormReparatur(null, _JgBediener.Daten, _Maschine);
      if (form.ShowDialog() ?? false)
      {
        _JgReparatur.Add(form.Reparatur, false);
        _Maschine.fAktivReparatur = form.Reparatur.Id;
        DbSichern.AbgleichEintragen(_Maschine.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);

        foreach (var anmeldungMaschine in _Maschine.sAktiveAnmeldungen)
        {
          var anmledRep = new tabAnmeldungReparatur()
          {
            Id = Guid.NewGuid(),
            Anmeldung = form.Reparatur.VorgangBeginn,
            fReparatur = form.Reparatur.Id,
            fBediener = anmeldungMaschine.fBediener
          };
          _JgRepAnmeldung.Add(anmledRep, false);
        }

        _JgReparatur.GeheZuDatensatz(form.Reparatur, true);
        TreeListMaschinRefresh();
      }
    }

    private void ReparaturBearbeitenAktuell_Click(object sender, RoutedEventArgs e)
    {
      Fenster.FormReparatur form = new Fenster.FormReparatur(_JgReparatur.Current, _JgBediener.Daten);
      if (form.ShowDialog() ?? false)
        _JgReparatur.DsSave();
      else
        _JgReparatur.Reload();
    }

    private void ReparaturBeenden_Click(object sender, RoutedEventArgs e)
    {
      var reparatur = _JgReparatur.Current;
      var anzeigeText = $"Maschine {reparatur.eMaschine.MaschinenName} mit Ereignis {reparatur.Ereignis} abmelden?";

      JgMaschineLib.Zeit.FormAuswahlDatumZeit form = new FormAuswahlDatumZeit("Abmeldung", anzeigeText, DateTime.Now);
      if (form.ShowDialog() ?? false)
      {
        var repBedienerAustragen = reparatur.sAnmeldungen.Where(w => w.IstAktiv).ToList();
        foreach (var bediener in repBedienerAustragen)
        {
          bediener.Abmeldung = form.DatumZeit;
          DbSichern.AbgleichEintragen(bediener.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
        reparatur.VorgangEnde = form.DatumZeit;
        reparatur.eMaschine.fAktivReparatur = null;
        DbSichern.AbgleichEintragen(reparatur.eMaschine.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        _JgReparatur.DsSave();
        _JgReparatur.Remove();

        TreeListMaschinRefresh();
      }
    }

    private void ReparaturBearbeitenAuswahl_Click(object sender, RoutedEventArgs e)
    {
      Fenster.FormReparatur form = new Fenster.FormReparatur(_ListeReparaturAuswahl.Current, _JgBediener.Daten);
      if (form.ShowDialog() ?? false)
        _ListeReparaturAuswahl.DsSave();
      else
        _ListeReparaturAuswahl.Reload(form.Reparatur);
    }

    private void ExecuteAnmeldungBedienerReparatur(object sender, ExecutedRoutedEventArgs e)
    {
      var beschäaeftigt = _JgReparatur.Daten.SelectMany(s => s.sAnmeldungen).Where(w => w.IstAktiv).Select(s => s.eBediener.Id).ToList();

      var form = new Fenster.FormNeuerBedienerReparatur(_JgBediener.Daten.Where(w => !beschäaeftigt.Contains(w.Id)));
      if (form.ShowDialog() ?? false)
      {
        var anmeldungReparatur = form.Anmeldung;
        anmeldungReparatur.eReparatur = _JgReparatur.Current;
        _JgRepAnmeldung.Add(anmeldungReparatur);
        _JgRepAnmeldung.Refresh();
      }
    }

    private void ReparaturAnmeldungAbmeldung_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = _JgRepAnmeldung.Current;
      string anzeigeText = $"Möchten Sie den Bediener {anmeldung.eBediener.Name} von der Reparatur an Maschine {anmeldung.eReparatur.eMaschine.MaschinenName} abmelden ?";

      var form = new JgFormDatumZeit();
      if (form.Anzeigen("Abmeldung", anzeigeText, DateTime.Now))
      {
        anmeldung.Abmeldung = form.Datum;
        _JgRepAnmeldung.DsSave(anmeldung);
        _JgRepAnmeldung.Refresh();
      }
    }

    private void ReparaturAnmeldungAuswahlBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var colView = (CollectionViewSource)FindResource("vsReparaturAuswahlBediener");
      var anmeldung = (tabAnmeldungReparatur)colView.View.CurrentItem;
      string anzeigeText = $"Reparaturzeiten für den Bediener {anmeldung.eBediener.Name} an Maschin {anmeldung.eReparatur.eMaschine.MaschinenName} bearbeiten.";

      var form = new FormAuswahlDatumVonBis("Anmeldung Maschine bearbeiten", anzeigeText, anmeldung.Anmeldung, anmeldung.Abmeldung ?? DateTime.Now);
      if (form.ShowDialog() ?? false)
      {
        anmeldung.Anmeldung = form.DatumVon;
        anmeldung.Abmeldung = form.DatumBis;
        DbSichern.AbgleichEintragen(anmeldung.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        _ListeReparaturAuswahl.Db.SaveChanges();
        colView.View.Refresh();
        colView.View.MoveCurrentTo(anmeldung);
      }
    }

    #endregion

    #region Anmeldungen Maschine ***************************************************

    private void NeueAnmeldungMaschine_Click(object sender, RoutedEventArgs e)
    {
      if (_Maschine == null)
      {
        Helper.Protokoll("Bitte Maschine in linker Tabelle auswahlen !", Helper.ProtokollArt.Warnung);
        return;
      }

      var bedienerAusgewaehlt = (tabBediener)(FindResource("vsBedienerAnmeldung") as CollectionViewSource).View.CurrentItem;
      var anmeldung = _JgMaschine.Daten.SelectMany(s => s.sAktiveAnmeldungen).FirstOrDefault(w => w.fBediener == bedienerAusgewaehlt.Id);

      if (anmeldung != null)
        MessageBox.Show($"{bedienerAusgewaehlt.Name} ist bereits an Maschine: '{anmeldung.eMaschine.MaschinenName}' angemeldet! Bitte erst an dieser Maschine abmelden.", "Information !", MessageBoxButton.OK, MessageBoxImage.Warning);
      else
      {
        JgFormDatumZeit form = new JgFormDatumZeit();
        if (form.Anzeigen("Anmeldung", $"Geben Sie die Zeit an, in welcher sich der '{_AuswahlBedienerAnmeldung.Name}' angemeldet hat.", DateTime.Now))
        {
          anmeldung = new tabAnmeldungMaschine()
          {
            Id = Guid.NewGuid(),
            eBediener = bedienerAusgewaehlt,
            eMaschine = _Maschine,
            Anmeldung = form.Datum,
            ManuelleAnmeldung = true,
            ManuelleAbmeldung = true,
            fAktivMaschine = _Maschine.Id
          };
          var datAnmeldung = (JgEntityTab<tabAnmeldungMaschine>)_DatenPool.Tabs[JgEntityAuto.TabArt.Anmeldung];
          datAnmeldung.Add(anmeldung);

          TreeListMaschinRefresh();
        }
      }
    }

    private void BanutzerMaschineAbmelden_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = _JgAnmeldung.Current;
      string anzeigeText = $"Möchten Sie den Bediener {anmeldung.eBediener.Name} von der Maschine {anmeldung.eMaschine.MaschinenName} abmelden ?";

      JgFormDatumZeit form = new JgFormDatumZeit();
      if (form.Anzeigen("Abmeldung", anzeigeText, DateTime.Now))
      {
        anmeldung.Abmeldung = form.Datum;
        anmeldung.ManuelleAbmeldung = true;
        anmeldung.eAktivMaschine = null;
        _JgAnmeldung.Remove(anmeldung);

        tabAnmeldungReparatur rep = _JgReparatur.Daten.SelectMany(s => s.sAnmeldungen).Where(w => w.IstAktiv).FirstOrDefault(f => f.fBediener == anmeldung.fBediener);
        if (rep != null)
        {
          rep.Abmeldung = form.Datum;
          DbSichern.AbgleichEintragen(rep.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }

        _JgAnmeldung.DsSave(anmeldung);

        TreeListMaschinRefresh();
      }
    }

    private void AnmeldungBenutzerBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = _ListeAnmeldungAuswahl.Current;
      var anz = $"Korrektur der Arbeitszeit für den Mitarbeiter {anmeldung.eBediener.Name}.";
      var form = new FormAuswahlDatumVonBis("Berichtigung Arbeitszeit", anz, anmeldung.Anmeldung, anmeldung.Abmeldung ?? DateTime.Now);
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
        _ListeAnmeldungAuswahl.DsSave();
        _ListeAnmeldungAuswahl.Refresh();
      }
    }

    #endregion

    #region Arbeitszeit verwalten **************************************************

    private void ArbeitszeitErstellen_Click(object sender, RoutedEventArgs e)
    {
      var bediener = (tabBediener)(FindResource("vsBedienerArbeitszeit") as CollectionViewSource).View.CurrentItem;

      if (bediener.eAktivArbeitszeit != null)
        Helper.Protokoll($"Bediener {bediener.Name} bereits angemeldet !", Helper.ProtokollArt.Warnung);
      else
      {
        string anzeigeText = string.Format("Mitarbeiter {0} im Betrieb anmelden ?", _AuswahlBedienerArbeitszeit.Name);
        FormAuswahlDatumZeit form = new FormAuswahlDatumZeit("Anmeldung", anzeigeText, DateTime.Now);
        if (form.ShowDialog() ?? false)
        {
          var arbeitszeit = new JgMaschineData.tabArbeitszeit()
          {
            Id = Guid.NewGuid(),
            fBediener = _AuswahlBedienerArbeitszeit.Id,
            fStandort = _Standort.Id,
            Anmeldung = form.DatumZeit,
            ManuelleAnmeldung = true,
            ManuelleAbmeldung = true,
          };

          _JgArbeitszeit.Add(arbeitszeit);

          bediener.fAktivArbeitszeit = arbeitszeit.Id;
          _JgBediener.DsSave(bediener);
        }
      }
    }

    private void ArbeitszeitAbmeldung_Click(object sender, RoutedEventArgs e)
    {
      JgFormDatumZeit form = new JgFormDatumZeit();
      if (form.Anzeigen("Abmeldung Bediener", $"Möchten Sie '{_JgArbeitszeit.Current.eBediener.Name}' abmelden ?", DateTime.Now))
      {
        var abmeldung = _JgArbeitszeit.Current;
        abmeldung.eBediener.fAktivArbeitszeit = null;
        abmeldung.Abmeldung = form.Datum;
        abmeldung.ManuelleAbmeldung = true;
        _JgArbeitszeit.DsSave(abmeldung);
        _JgArbeitszeit.Remove(abmeldung);
      }
    }

    private void ArbeitszeitBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var anz = $"Korrektur der Arbeitszeit für den Mitarbeiter {_ListeArbeitszeitAuswahl.Current.eBediener.Name}.";

      var form = new FormAuswahlDatumVonBis("Berichtigung Arbeitszeit", anz, _ListeArbeitszeitAuswahl.Current.Anmeldung, _ListeArbeitszeitAuswahl.Current.Abmeldung ?? DateTime.Now);
      if (form.ShowDialog() ?? false)
      {
        if (form.DatumVon != _ListeArbeitszeitAuswahl.Current.Anmeldung)
        {
          _ListeArbeitszeitAuswahl.Current.Anmeldung = form.DatumVon;
          _ListeArbeitszeitAuswahl.Current.ManuelleAnmeldung = true;
        }
        if (form.DatumBis != _ListeArbeitszeitAuswahl.Current.Abmeldung)
        {
          _ListeArbeitszeitAuswahl.Current.Abmeldung = form.DatumBis;
          _ListeArbeitszeitAuswahl.Current.ManuelleAbmeldung = true;
        }
        _ListeArbeitszeitAuswahl.DsSave();
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
        await InitListen();
      }
    }
    private void Window_Closed(object sender, EventArgs e)
    {
      Properties.Settings.Default.Save();
    }

    private void btnAuswahlAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      switch ((sender as Button).Tag.ToString())
      {
        case "1": _ListeArbeitszeitAuswahl.DatenAktualisieren(); break;
        case "2": _ListeAnmeldungAuswahl.DatenAktualisieren(); break;
        case "3": _ListeBauteilAuswahl.DatenAktualisieren(); break;
        case "4": _ListeReparaturAuswahl.DatenAktualisieren(); break;
      }
    }

    private void btnDrucken_Click(object sender, RoutedEventArgs e)
    {
      var vorgang = Convert.ToInt32((sender as Button).Tag);  // 1 - Anzeigen, 2 - Drucken, 3 - Design, 4 - Neuer Report

      if (vorgang < 4)
      {
        switch (_Auswahl)
        {
          case EnumFilterAuswertung.Arbeitszeit: _AktAuswertung = (tabAuswertung)_VsAuswertungArbeitszeit.View.CurrentItem; break;
          case EnumFilterAuswertung.Anmeldung: _AktAuswertung = (tabAuswertung)_VsAuswertungAnmeldung.View.CurrentItem; break;
          case EnumFilterAuswertung.Bauteil: _AktAuswertung = (tabAuswertung)_VsAuswertungBauteil.View.CurrentItem; break;
          case EnumFilterAuswertung.Reparatur: _AktAuswertung = (tabAuswertung)_VsAuswertungReparatur.View.CurrentItem; break;
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

      switch (_Auswahl)
      {
        case JgMaschineData.EnumFilterAuswertung.Arbeitszeit:
          if (tcArbeitszeit.SelectedIndex == 0)
            _Report.RegisterData(_JgArbeitszeit.Daten, "Daten");
          else
          {
            _Report.RegisterData(_ListeArbeitszeitAuswahl.Daten, "Daten");
            _Report.SetParameterValue("DatumVon", _DzArbeitszeitVon);
            _Report.SetParameterValue("DatumBis", _DzArbeitszeitBis);
          }
          _Report.SetParameterValue("IstAktuell", tcArbeitszeit.SelectedIndex == 0);
          break;
        case JgMaschineData.EnumFilterAuswertung.Anmeldung:
          if (tcAnmeldung.SelectedIndex == 0)
            _Report.RegisterData(_JgAnmeldung.Daten, "Daten");
          else
          {
            _Report.RegisterData(_ListeAnmeldungAuswahl.Daten, "Daten");
            _Report.SetParameterValue("DatumVon", _DzAnmeldungVon);
            _Report.SetParameterValue("DatumBis", _DzAnmeldungBis);
          }
          _Report.SetParameterValue("IstAktuell", tcAnmeldung.SelectedIndex == 0);
          break;
        case EnumFilterAuswertung.Bauteil:
          _Report.RegisterData(_ListeBauteilAuswahl.Daten, "Daten");
          _Report.SetParameterValue("DatumVon", _DzBauteilVon);
          _Report.SetParameterValue("DatumBis", _DzBauteilBis);
          break;
        case EnumFilterAuswertung.Reparatur:
          if (tcReparatur.SelectedIndex == 0)
            _Report.RegisterData(_JgReparatur.Daten, "Daten");
          else
          {
            _Report.RegisterData(_ListeReparaturAuswahl.Daten, "Daten");
            _Report.SetParameterValue("DatumVon", _DzReparaturVon);
            _Report.SetParameterValue("DatumBis", _DzReparaturBis);
          }
          _Report.SetParameterValue("IstAktuell", tcReparatur.SelectedIndex == 0);
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
            FilterAuswertung = _Auswahl,
            ReportName = form.ReportName,
            ErstelltDatum = DateTime.Now,
            ErstelltName = username,
            GeaendertDatum = DateTime.Now,
            GeaendertName = username
          };
          _ListeAuswertung.Add(_AktAuswertung);

          switch (_Auswahl)
          {
            case EnumFilterAuswertung.Arbeitszeit: _VsAuswertungArbeitszeit.View.MoveCurrentTo(_AktAuswertung); break;
            case EnumFilterAuswertung.Anmeldung: _VsAuswertungAnmeldung.View.MoveCurrentTo(_AktAuswertung); break;
            case EnumFilterAuswertung.Bauteil: _VsAuswertungBauteil.View.MoveCurrentTo(_AktAuswertung); break;
            case EnumFilterAuswertung.Reparatur: _VsAuswertungReparatur.View.MoveCurrentTo(_AktAuswertung); break;
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

    private void TabelleAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      _DatenPool.DatenAktualisieren();
    }

    private void button_Click(object sender, RoutedEventArgs e)
    {
      _JgReparatur.ViewSource.View.Refresh();

    }
  }
}
