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
using JgMaschineVerwalten.Commands;
using JgZeitHelper;
using Microsoft.Win32;

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

    private JgEntityListAuto _DatenPool;
    private JgEntityList<tabBediener> _JgBediener { get { return (JgEntityList<tabBediener>)_DatenPool.Tabs[JgEntityListAuto.TabArt.Bediener]; } }
    private JgEntityList<tabMaschine> _JgMaschine { get { return (JgEntityList<tabMaschine>)_DatenPool.Tabs[JgEntityListAuto.TabArt.Maschine]; } }
    private JgEntityList<tabArbeitszeit> _JgArbeitszeit { get { return (JgEntityList<tabArbeitszeit>)_DatenPool.Tabs[JgEntityListAuto.TabArt.Arbeitszeit]; } }
    private JgEntityList<tabAnmeldungMaschine> _JgAnmeldung { get { return (JgEntityList<tabAnmeldungMaschine>)_DatenPool.Tabs[JgEntityListAuto.TabArt.Anmeldung]; } }
    private JgEntityList<tabReparatur> _JgReparatur { get { return (JgEntityList<tabReparatur>)_DatenPool.Tabs[JgEntityListAuto.TabArt.Reparatur]; } }
    private JgEntityList<tabAnmeldungReparatur> _JgRepAnmeldung { get { return (JgEntityList<tabAnmeldungReparatur>)_DatenPool.Tabs[JgEntityListAuto.TabArt.RepAnmeldung]; } }
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
    private Guid[] _IdisMaschinen { get { return _JgMaschine.Daten.Select(s => s.Id).ToArray(); } }

    private JgEntityList<tabArbeitszeit> _ListeArbeitszeitAuswahl;
    private JgZeit _DzArbeitszeitVon { get { return (JgZeit)FindResource("dzArbeitszeitVon"); } }
    private JgZeit _DzArbeitszeitBis { get { return (JgZeit)FindResource("dzArbeitszeitBis"); } }

    private JgEntityList<tabAnmeldungMaschine> _ListeAnmeldungAuswahl;
    private JgZeit _DzAnmeldungVon { get { return (JgZeit)FindResource("dzAnmeldungVon"); } }
    private JgZeit _DzAnmeldungBis { get { return (JgZeit)FindResource("dzAnmeldungBis"); } }

    private JgEntityList<tabBauteil> _ListeBauteilAuswahl;
    private JgZeit _DzBauteilVon { get { return (JgZeit)FindResource("dzBauteilVon"); } }
    private JgZeit _DzBauteilBis { get { return (JgZeit)FindResource("dzBauteilBis"); } }

    private JgEntityList<tabReparatur> _ListeReparaturAuswahl;
    private JgZeit _DzReparaturVon { get { return (JgZeit)FindResource("dzReparaturVon"); } }
    private JgZeit _DzReparaturBis { get { return (JgZeit)FindResource("dzReparaturBis"); } }

    private FastReport.Report _Report;
    private tabAuswertung _AktAuswertung = null;
    private FastReport.EnvironmentSettings _ReportSettings = new FastReport.EnvironmentSettings();

    private JgEntityList<tabAuswertung> _ListeReporteArbeitszeit;
    private JgEntityList<tabAuswertung> _ListeReporteAnmedlung;
    private JgEntityList<tabAuswertung> _ListeReporteBauteil;
    private JgEntityList<tabAuswertung> _ListeReporteReparatur;

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _Db = new JgModelContainer();
      _Standort = await _Db.tabStandortSet.FirstOrDefaultAsync(f => f.Id == Properties.Settings.Default.IdStandort);
      if (_Standort == null)
        _Standort = await _Db.tabStandortSet.FirstOrDefaultAsync();

      tblStandort.Text = _Standort.Bezeichnung;

      await InitListen();

      CommandBindings.Add(new CommandBinding(MyCommands.AnmeldungReparaturBediener, ExecuteAnmeldungBedienerReparatur, (sen, erg) =>
      {
        erg.CanExecute = _JgReparatur?.Current != null;
      }));

      _Report = new FastReport.Report();
      _Report.FileName = "Datenbank";
      _ReportSettings.CustomSaveReport += (obj, repEvent) =>
      {
        MemoryStream memStr = new MemoryStream();
        try
        {
          repEvent.Report.Save(memStr);
          _ListeReporteArbeitszeit.Db.tabAuswertungSet.Attach(_AktAuswertung);
          _AktAuswertung.Report = memStr.ToArray();
          _AktAuswertung.GeaendertDatum = DateTime.Now;
          _AktAuswertung.GeaendertName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
          _ListeReporteArbeitszeit.Db.SaveChanges();
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
      _DatenPool = new JgEntityListAuto(Properties.Settings.Default.VerbindungsString, 5)
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
        var eBediener = new JgEntityList<tabBediener>(_DatenPool.Db)
        {
          AbfrageSqlString = () =>
          {
            if (_Standort == null)
              return JgEntityList.IstNull;

            return "Select Id, DatenAbgleich_Datum "
              + "From tabBedienerSet "
              + "Where (fStandort = '" + _Standort.Id + "') AND (Status <> " + EnumStatusBediener.Stillgelegt.ToString("d") + ")";
          }
        };
        eBediener.SetIdis(con);
        if (eBediener.Idis != null)
          eBediener.Daten = await _DatenPool.Db.tabBedienerSet.Where(w => eBediener.Idis.Contains(w.Id)).ToListAsync();
        _DatenPool.Tabs.Add(JgEntityListAuto.TabArt.Bediener, eBediener);

        (FindResource("vsBedienerArbeitszeit") as CollectionViewSource).Source = eBediener.Daten;
        (FindResource("vsBedienerAnmeldung") as CollectionViewSource).Source = eBediener.Daten;

        var eMaschine = new JgEntityList<tabMaschine>(_DatenPool.Db)
        {
          ViewSource = (CollectionViewSource)FindResource("vsMaschine"),
          AbfrageSqlString = () =>
          {
            if (_Standort == null)
              return JgEntityList.IstNull;

            return "SELECT Id, DatenAbgleich_Datum "
              + "FROM tabMaschineSet "
              + "WHERE (fStandort = '" + _Standort.Id + "') AND (Status <> " + EnumStatusMaschine.Stillgelegt.ToString("d") + ")";
          }
        };
        eMaschine.SetIdis(con);
        if (eMaschine.Idis != null)
          eMaschine.Daten = await _DatenPool.Db.tabMaschineSet.Where(w => eMaschine.Idis.Contains(w.Id)).ToListAsync();
        _DatenPool.Tabs.Add(JgEntityListAuto.TabArt.Maschine, eMaschine);

        var eArbeitszeit = new JgEntityList<tabArbeitszeit>(_DatenPool.Db)
        {
          ViewSource = (CollectionViewSource)FindResource("vsArbeitszeitAktuell"),
          Tabellen = new DataGrid[] { dgArbeitszeitAktuell },
          AbfrageSqlString = () =>
          {
            if (_Standort == null)
              return JgEntityList.IstNull;

            return "SELECT arbeitszeit.Id, arbeitszeit.DatenAbgleich_Datum "
              + "FROM tabBedienerSet AS bediener INNER JOIN "
              + "tabArbeitszeitSet AS arbeitszeit ON bediener.fAktivArbeitszeit = arbeitszeit.Id "
              + "WHERE(bediener.fStandort = '" + _Standort.Id + "')";
          }
        };
        eArbeitszeit.SetIdis(con);
        if (eArbeitszeit.Idis != null)
          eArbeitszeit.Daten = await _DatenPool.Db.tabArbeitszeitSet.Where(w => eArbeitszeit.Idis.Contains(w.Id)).ToListAsync();
        _DatenPool.Tabs.Add(JgEntityListAuto.TabArt.Arbeitszeit, eArbeitszeit);

        var eAnmeldung = new JgEntityList<tabAnmeldungMaschine>(_DatenPool.Db)
        {
          ViewSource = (CollectionViewSource)FindResource("vsAnmeldungAktuell"),
          Tabellen = new DataGrid[] { dgAnmeldungAktuell },
          AbfrageSqlString = () =>
          {
            if (_JgMaschine.Idis == null)
              return JgEntityList.IstNull;
            else
              return "SELECT Id, DatenAbgleich_Datum "
                + "FROM tabAnmeldungMaschineSet "
                + "WHERE fAktivMaschine In (" + JgEntityList.IdisInString(_JgMaschine.Idis) + ")";
          }
        };
        eAnmeldung.SetIdis(con);
        if (eAnmeldung.Idis != null)
          eAnmeldung.Daten = await _DatenPool.Db.tabAnmeldungMaschineSet.Where(w => eAnmeldung.Idis.Contains(w.Id)).ToListAsync();
        _DatenPool.Tabs.Add(JgEntityListAuto.TabArt.Anmeldung, eAnmeldung);

        var eReparatur = new JgEntityList<tabReparatur>(_DatenPool.Db)
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
                  + "WHERE Id IN (" + JgEntityList.IdisInString(ids) + ")";
            }
            return JgEntityList.IstNull;
          }
        };
        eReparatur.SetIdis(con);
        if (eReparatur.Idis != null)
          eReparatur.Daten = _DatenPool.Db.tabReparaturSet.Where(w => eReparatur.Idis.Contains(w.Id)).ToList();
        _DatenPool.Tabs.Add(JgEntityListAuto.TabArt.Reparatur, eReparatur);

        var eRepAnmeldung = new JgEntityList<tabAnmeldungReparatur>(_DatenPool.Db, false)
        {
          ViewSource = (CollectionViewSource)FindResource("vsReparaturAktuellBediener"),
          AbfrageSqlString = () =>
          {
            if (_JgReparatur.Idis != null)
              return "SELECT Id, DatenAbgleich_Datum "
                + "FROM tabAnmeldungReparaturSet "
                + "WHERE fReparatur In (" + JgEntityList.IdisInString(_JgReparatur.Idis) + ")";

            return JgEntityList.IstNull;
          }
        };
        eRepAnmeldung.SetIdis(con);
        if (eRepAnmeldung.Idis != null)
          eRepAnmeldung.Daten = await _DatenPool.Db.tabAnmeldungReparaturSet.Where(w => eRepAnmeldung.Idis.Contains(w.Id)).ToListAsync();
        _DatenPool.Tabs.Add(JgEntityListAuto.TabArt.RepAnmeldung, eRepAnmeldung);

        // Bei Benutzer zusätzliche Viewsorce refreshen

        _JgBediener.ViewSorceAuchAktualisieren.AddRange(new CollectionViewSource[] { _JgArbeitszeit.ViewSource, _JgAnmeldung.ViewSource, _JgReparatur.ViewSource, _JgRepAnmeldung.ViewSource });
      }
      catch (Exception f)
      {
        Helper.Protokoll("Fehler beim erstellen des Datenpools !", f);
      }
      finally
      {
        con.Close();
      }

      var von = DateTime.Now.Date;
      var bis = new DateTime(von.Year, von.Month, von.Day, 23, 59, 59);

      // Arbeitszeiten ********************************************************

      _DzArbeitszeitVon.AnzeigeDatumZeit = von;
      _DzArbeitszeitBis.AnzeigeDatumZeit = bis;

      _ListeArbeitszeitAuswahl = new JgEntityList<tabArbeitszeit>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsArbeitszeitAuswahl"),
        Tabellen = new DataGrid[] { dgArbeitszeitAuswahl },
        OnDatenLaden = (d, p) =>
        {
          var idStandort = (Guid)p["Standort"];
          var datVon = (DateTime)p["DatumVon"];
          var datBis = (DateTime)p["DatumBis"];
          return d.tabArbeitszeitSet.Where(w => (w.fStandort == idStandort) && (w.Anmeldung >= datVon) && ((w.Anmeldung <= datBis)))
            .OrderBy(o => o.Anmeldung).ToList();
        }
      };
      _ListeArbeitszeitAuswahl.Parameter = new Dictionary<string, object>()
      {
        { "Standort", _Standort.Id },
        { "DatumVon", _DzArbeitszeitVon.AnzeigeDatumZeit },
        { "DatumBis", _DzArbeitszeitBis.AnzeigeDatumZeit }
      };

      // Anmeldungenan Maschinen ********************************************************

      _DzAnmeldungVon.AnzeigeDatumZeit = von;
      _DzAnmeldungBis.AnzeigeDatumZeit = bis;

      _ListeAnmeldungAuswahl = new JgEntityList<tabAnmeldungMaschine>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsAnmeldungAuswahl"),
        Tabellen = new DataGrid[] { dgAnmeldungAuswahl },
        OnDatenLaden = (d, p) =>
        {
          var idisMaschinen = (Guid[])p["IdisMaschinen"];
          var datVon = (DateTime)p["DatumVon"];
          var datBis = (DateTime)p["DatumBis"];
          return d.tabAnmeldungMaschineSet.Where(w => idisMaschinen.Contains(w.fMaschine) && (w.Anmeldung >= datVon) && ((w.Anmeldung <= datBis)))
            .OrderBy(o => o.Anmeldung).ToList();
        }
      };
      _ListeAnmeldungAuswahl.Parameter = new Dictionary<string, object>()
      {
        { "IdisMaschinen", _IdisMaschinen },
        { "DatumVon", _DzAnmeldungVon.AnzeigeDatumZeit },
        { "DatumBis", _DzAnmeldungBis.AnzeigeDatumZeit }
      };

      // Bauteile initialisieren ****************************

      _DzBauteilVon.AnzeigeDatumZeit = von;
      _DzBauteilBis.AnzeigeDatumZeit = bis;

      _ListeBauteilAuswahl = new JgEntityList<tabBauteil>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsBauteilAuswahl"),
        Tabellen = new DataGrid[] { dgBauteilAuswahl },
        OnDatenLaden = (d, p) =>
        {
          var maschine = (tabMaschine)p["Maschine"];
          var datVon = (DateTime)p["DatumVon"];
          var datBis = (DateTime)p["DatumBis"];

          if (maschine == null)
          {
            Helper.Protokoll("Bitte Maschine in linker Tabelle auswahlen !", Helper.ProtokollArt.Warnung);
            return null;
          }
          else
            return d.tabBauteilSet.Where(w => (w.fMaschine == maschine.Id) && (w.DatumStart >= datVon) && (w.DatumStart <= datBis))
            .Include(i => i.sBediener)
            .OrderBy(o => o.DatumStart).ToList();
        }
      };
      _ListeAnmeldungAuswahl.Parameter = new Dictionary<string, object>()
      {
        { "Maschine", _Maschine },
        { "DatumVon", _DzBauteilVon.AnzeigeDatumZeit },
        { "DatumBis", _DzBauteilBis.AnzeigeDatumZeit }
      };

      // Reparaturen initialisieren ********************************************************

      _DzReparaturVon.AnzeigeDatumZeit = von;
      _DzReparaturBis.AnzeigeDatumZeit = bis;

      _ListeReparaturAuswahl = new JgEntityList<tabReparatur>
      {
        ViewSource = (CollectionViewSource)FindResource("vsReparaturAuswahl"),
        Tabellen = new DataGrid[] { dgReparaturAuswahl },
        OnDatenLaden = (d, p) =>
        {
          var idisMaschinen = (Guid[])p["IdisMaschinen"];
          var datVon = (DateTime)p["DatumVon"];
          var datBis = (DateTime)p["DatumBis"];
          return d.tabReparaturSet.Where(w => idisMaschinen.Contains(w.fMaschine) && (w.VorgangBeginn >= datVon) && (w.VorgangBeginn <= datBis))
            .Include(i => i.sAnmeldungen).Include(i => i.sAnmeldungen.Select(s => s.eBediener))
            .OrderByDescending(o => o.VorgangBeginn);
        }
      };
      _ListeReparaturAuswahl.Parameter = new Dictionary<string, object>()
      {
        { "IdisMaschinen", _IdisMaschinen },
        { "DatumVon", _DzReparaturVon.AnzeigeDatumZeit },
        { "DatumBis", _DzReparaturBis.AnzeigeDatumZeit }
      };


      // Auswertung initialisieren ********************************

      var db = new JgModelContainer();
      var reporte = db.tabAuswertungSet.Where(w => (w.FilterAuswertung != EnumFilterAuswertung.Allgemein) && (!w.DatenAbgleich.Geloescht)).ToList();

      _ListeReporteArbeitszeit = new JgEntityList<tabAuswertung>(db)
      {
        ViewSource = (CollectionViewSource)this.FindResource("vsAuswertungArbeitszeit"),
        Daten = reporte.Where(w => w.FilterAuswertung == EnumFilterAuswertung.Arbeitszeit).OrderBy(o => o.AnzeigeReportname).ToList()
      };

      _ListeReporteAnmedlung = new JgEntityList<tabAuswertung>(db)
      {
        ViewSource = (CollectionViewSource)this.FindResource("vsAuswertungAnmeldung"),
        Daten = reporte.Where(w => w.FilterAuswertung == EnumFilterAuswertung.Anmeldung).OrderBy(o => o.AnzeigeReportname).ToList()
      };

      _ListeReporteBauteil = new JgEntityList<tabAuswertung>(db)
      {
        ViewSource = (CollectionViewSource)this.FindResource("vsAuswertungBauteil"),
        Daten = reporte.Where(w => w.FilterAuswertung == EnumFilterAuswertung.Bauteil).OrderBy(o => o.AnzeigeReportname).ToList()
      };

      _ListeReporteReparatur = new JgEntityList<tabAuswertung>(db)
      {
        ViewSource = (CollectionViewSource)this.FindResource("vsAuswertungReparatur"),
        Daten = reporte.Where(w => w.FilterAuswertung == EnumFilterAuswertung.Reparatur).OrderBy(o => o.AnzeigeReportname).ToList()
      };
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

      try
      {
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
      catch { }
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

        _JgReparatur.GeheZuDatensatz(form.Reparatur);
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
      var anzeigeText = $"Maschine {reparatur.eMaschine.MaschinenName} mit Vorgang {reparatur.Vorgang} abmelden?";
      var ergZeitAbfrage = DateTime.Now;

      if (JgZeit.AbfrageZeit(anzeigeText, "Abmeldung", ref ergZeitAbfrage))
      {
        var repBedienerAustragen = reparatur.sAnmeldungen.Where(w => w.IstAktiv).ToList();
        foreach (var bediener in repBedienerAustragen)
          bediener.Abmeldung = ergZeitAbfrage;

        reparatur.VorgangEnde = ergZeitAbfrage;
        reparatur.eMaschine.fAktivReparatur = null;
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
      var zeitAbmeldung = anmeldung.Abmeldung ?? DateTime.Now;

      if (JgZeit.AbfrageZeit(anzeigeText, "Abmeldung", ref zeitAbmeldung))
      {
        anmeldung.Abmeldung = zeitAbmeldung;
        _JgRepAnmeldung.DsSave(anmeldung);
        _JgRepAnmeldung.Refresh();
      }
    }

    private void ReparaturAnmeldungAuswahlBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var colView = (CollectionViewSource)FindResource("vsReparaturAuswahlBediener");
      var anmeldung = (tabAnmeldungReparatur)colView.View.CurrentItem;
      string anzeigeText = $"Reparaturzeiten für den Bediener {anmeldung.eBediener.Name} an Maschin {anmeldung.eReparatur.eMaschine.MaschinenName} bearbeiten.";

      var zeitAnmeldung = anmeldung.Anmeldung;
      var zeitAbmeldung = anmeldung.Abmeldung ?? DateTime.Now;

      if (JgZeit.AbfrageZeit(anzeigeText, "Anmeldung Maschine bearbeiten", ref zeitAnmeldung, ref zeitAbmeldung))
      {
        anmeldung.Anmeldung = zeitAnmeldung;
        anmeldung.Abmeldung = zeitAbmeldung;
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
        var msg = $"Geben Sie die Zeit an, in welcher sich der '{_AuswahlBedienerAnmeldung.Name}' angemeldet hat.";
        var zeitAnmeldung = DateTime.Now;

        if (JgZeit.AbfrageZeit(msg, "Anmeldung", ref zeitAnmeldung))
        {
          anmeldung = new tabAnmeldungMaschine()
          {
            Id = Guid.NewGuid(),
            eBediener = bedienerAusgewaehlt,
            eMaschine = _Maschine,
            Anmeldung = zeitAnmeldung,
            ManuelleAnmeldung = true,
            ManuelleAbmeldung = true,
            fAktivMaschine = _Maschine.Id
          };
          var datAnmeldung = (JgEntityList<tabAnmeldungMaschine>)_DatenPool.Tabs[JgEntityListAuto.TabArt.Anmeldung];
          datAnmeldung.Add(anmeldung);

          TreeListMaschinRefresh();
        }
      }
    }

    private void BanutzerMaschineAbmelden_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = _JgAnmeldung.Current;
      string anzeigeText = $"Möchten Sie den Bediener {anmeldung.eBediener.Name} von der Maschine {anmeldung.eMaschine.MaschinenName} abmelden ?";
      var zeitAbmeldung = anmeldung.Abmeldung ?? DateTime.Now;

      if (JgZeit.AbfrageZeit(anzeigeText, "Abmeldung", ref zeitAbmeldung))
      {
        anmeldung.Abmeldung = zeitAbmeldung;
        anmeldung.ManuelleAbmeldung = true;
        anmeldung.eAktivMaschine = null;
        _JgAnmeldung.Remove(anmeldung);

        tabAnmeldungReparatur rep = _JgReparatur.Daten.SelectMany(s => s.sAnmeldungen).Where(w => w.IstAktiv).FirstOrDefault(f => f.fBediener == anmeldung.fBediener);
        if (rep != null)
          rep.Abmeldung = zeitAbmeldung;

        _JgAnmeldung.DsSave(anmeldung);

        TreeListMaschinRefresh();
      }
    }

    private void AnmeldungBenutzerBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = _ListeAnmeldungAuswahl.Current;
      var msg = $"Korrektur der Arbeitszeit für den Mitarbeiter {anmeldung.eBediener.Name}.";
      var zeitAnmeldung = anmeldung.Anmeldung;
      var zeitAbmeldung = anmeldung.Abmeldung ?? DateTime.Now;

      if (JgZeit.AbfrageZeit(msg, "Berichtigung Arbeitszeit", ref zeitAnmeldung, ref zeitAbmeldung))
      {
        if (zeitAnmeldung != anmeldung.Anmeldung)
        {
          anmeldung.Anmeldung = zeitAnmeldung;
          anmeldung.ManuelleAnmeldung = true;
        }
        if (zeitAbmeldung != anmeldung.Abmeldung)
        {
          anmeldung.Abmeldung = zeitAbmeldung;
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
        var ergZeitAbfrage = DateTime.Now;

        if (JgZeit.AbfrageZeit(anzeigeText, "Anmeldung", ref ergZeitAbfrage))
        {
          var arbeitszeit = new JgMaschineData.tabArbeitszeit()
          {
            Id = Guid.NewGuid(),
            fBediener = _AuswahlBedienerArbeitszeit.Id,
            fStandort = _Standort.Id,
            Anmeldung = ergZeitAbfrage,
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
      var msg = $"Möchten Sie '{_JgArbeitszeit.Current.eBediener.Name}' abmelden ?";
      var ergZeitAbfrage = DateTime.Now;
      if (JgZeit.AbfrageZeit(msg, "Abmeldung Bediener", ref ergZeitAbfrage))
      {
        var abmeldung = _JgArbeitszeit.Current;
        abmeldung.eBediener.fAktivArbeitszeit = null;
        abmeldung.Abmeldung = ergZeitAbfrage;
        abmeldung.ManuelleAbmeldung = true;
        _JgArbeitszeit.DsSave(abmeldung);
        _JgArbeitszeit.Remove(abmeldung);
      }
    }

    private void ArbeitszeitBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var msg = $"Korrektur der Arbeitszeit für den Mitarbeiter {_ListeArbeitszeitAuswahl.Current.eBediener.Name}.";
      var zeitAnmeldung = _ListeArbeitszeitAuswahl.Current.Anmeldung ?? DateTime.Now;
      var zeitAbmeldung = _ListeArbeitszeitAuswahl.Current.Abmeldung ?? DateTime.Now;

      if (JgZeit.AbfrageZeit(msg, "Berichtigung Arbeitszeit", ref zeitAnmeldung, ref zeitAbmeldung))
      {
        if (zeitAnmeldung != _ListeArbeitszeitAuswahl.Current.Anmeldung)
        {
          _ListeArbeitszeitAuswahl.Current.Anmeldung = zeitAnmeldung;
          _ListeArbeitszeitAuswahl.Current.ManuelleAnmeldung = true;
        }
        if (zeitAnmeldung != _ListeArbeitszeitAuswahl.Current.Abmeldung)
        {
          _ListeArbeitszeitAuswahl.Current.Abmeldung = zeitAbmeldung;
          _ListeArbeitszeitAuswahl.Current.ManuelleAbmeldung = true;
        }
        _ListeArbeitszeitAuswahl.DsSave();
      }
    }

    #endregion

    private async void ButtonOptionen_Click(object sender, RoutedEventArgs e)
    {
      _DatenPool.JgTimer.Stop();
      List<tabStandort> standorte;
      using (var db = new JgModelContainer())
      {
        standorte = await db.tabStandortSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.Bezeichnung).ToListAsync();
      }

      Fenster.FormOptionen form = new Fenster.FormOptionen(standorte);
      if (form.ShowDialog() ?? false)
      {
        _Standort = form.StandOrt;
        tblStandort.Text = _Standort.Bezeichnung;
        await InitListen();
      }
      else
        _DatenPool.JgTimer.Start();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      Properties.Settings.Default.Save();
    }

    private void btnAuswahlAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      switch ((sender as Button).Tag.ToString())
      {
        case "1":
          _ListeArbeitszeitAuswahl.Parameter["DatumVon"] = _DzArbeitszeitVon.AnzeigeDatumZeit;
          _ListeArbeitszeitAuswahl.Parameter["DatumBis"] = _DzArbeitszeitBis.AnzeigeDatumZeit;
          _ListeArbeitszeitAuswahl.DatenNeuLaden();
          break;
        case "2":
          _ListeAnmeldungAuswahl.Parameter["IdisMaschinen"] = _IdisMaschinen;
          _ListeAnmeldungAuswahl.Parameter["DatumVon"] = _DzAnmeldungVon.AnzeigeDatumZeit;
          _ListeAnmeldungAuswahl.Parameter["DatumBis"] = _DzAnmeldungBis.AnzeigeDatumZeit;
          _ListeAnmeldungAuswahl.DatenNeuLaden();
          break;
        case "3":
          _ListeBauteilAuswahl.Parameter["Maschine"] = _Maschine;
          _ListeBauteilAuswahl.Parameter["DatumVon"] = _DzBauteilVon.AnzeigeDatumZeit;
          _ListeBauteilAuswahl.Parameter["DatumBis"] = _DzBauteilBis.AnzeigeDatumZeit;
          _ListeBauteilAuswahl.DatenNeuLaden();
          break;
        case "4":
          _ListeReparaturAuswahl.Parameter["IdisMaschinen"] = _IdisMaschinen;
          _ListeReparaturAuswahl.Parameter["DatumVon"] = _DzReparaturVon.AnzeigeDatumZeit;
          _ListeReparaturAuswahl.Parameter["DatumBis"] = _DzReparaturBis.AnzeigeDatumZeit;
          _ListeReparaturAuswahl.DatenNeuLaden();
          break;
      }
    }

    private void btnDrucken_Click(object sender, RoutedEventArgs e)
    {
      var vorgang = Convert.ToInt32((sender as Button).Tag);  // 1 - Anzeigen, 2 - Drucken, 3 - Design, 4 - Neuer Report, 5 - Report Exportieren, 6 - Löschen

      if (vorgang != 4)
      {
        switch (_Auswahl)
        {
          case EnumFilterAuswertung.Arbeitszeit: _AktAuswertung = (tabAuswertung)_ListeReporteArbeitszeit.Current; break;
          case EnumFilterAuswertung.Anmeldung: _AktAuswertung = (tabAuswertung)_ListeReporteAnmedlung.Current; break;
          case EnumFilterAuswertung.Bauteil: _AktAuswertung = (tabAuswertung)_ListeReporteBauteil.Current; break;
          case EnumFilterAuswertung.Reparatur: _AktAuswertung = (tabAuswertung)_ListeReporteReparatur.Current; break;
        }

        if (_AktAuswertung == null)
        {
          MessageBox.Show("Es wurde kein Report ausgewählt.", "Fehler !", MessageBoxButton.OK, MessageBoxImage.Information);
          return;
        }

        switch (vorgang)
        {
          case 5: // Exportieren
            SaveFileDialog dia = new SaveFileDialog();
            dia.Filter = "Fastreport (*.frx)|*.frx|Alle Dateien (*.*)|*.*";
            dia.FilterIndex = 1;
            if (dia.ShowDialog() ?? false)
            {
              _Report.Save(dia.FileName);
              MemoryStream mem;
              mem = new MemoryStream(_AktAuswertung.Report);
              using (Stream f = File.Create(dia.FileName))
              {
                mem.CopyTo(f);
              }
            }
            return;
          case 6:  // Report löschen
            var mb = MessageBox.Show($"Report {_AktAuswertung.ReportName} löschen ?", "Löschabfrage", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None);
            if (mb == MessageBoxResult.Yes)
            {
              switch (_Auswahl)
              {
                case EnumFilterAuswertung.Arbeitszeit: _ListeReporteArbeitszeit.Delete(_AktAuswertung); break;
                case EnumFilterAuswertung.Anmeldung: _ListeReporteAnmedlung.Delete(_AktAuswertung); break;
                case EnumFilterAuswertung.Bauteil: _ListeReporteBauteil.Delete(_AktAuswertung); break;
                case EnumFilterAuswertung.Reparatur: _ListeReporteReparatur.Delete(_AktAuswertung); break;
              }
            }
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
            _Report.SetParameterValue("DatumVon", _DzArbeitszeitVon.AnzeigeDatumZeit);
            _Report.SetParameterValue("DatumBis", _DzArbeitszeitBis.AnzeigeDatumZeit);
          }
          _Report.SetParameterValue("IstAktuell", tcArbeitszeit.SelectedIndex == 0);
          break;
        case JgMaschineData.EnumFilterAuswertung.Anmeldung:
          if (tcAnmeldung.SelectedIndex == 0)
            _Report.RegisterData(_JgAnmeldung.Daten, "Daten");
          else
          {
            _Report.RegisterData(_ListeAnmeldungAuswahl.Daten, "Daten");
            _Report.SetParameterValue("DatumVon", _DzAnmeldungVon.AnzeigeDatumZeit);
            _Report.SetParameterValue("DatumBis", _DzAnmeldungBis.AnzeigeDatumZeit);
          }
          _Report.SetParameterValue("IstAktuell", tcAnmeldung.SelectedIndex == 0);
          break;
        case EnumFilterAuswertung.Bauteil:
          if (_Maschine == null)
          {
            MessageBox.Show("Wählen Sie eine Maschine aus.");
            return;
          }

          _Report.RegisterData(_ListeBauteilAuswahl.Daten, "Daten");
          _Report.SetParameterValue("MaschinenName", _Maschine.MaschinenName);
          _Report.SetParameterValue("DatumVon", _DzBauteilVon.AnzeigeDatumZeit);
          _Report.SetParameterValue("DatumBis", _DzBauteilBis.AnzeigeDatumZeit);
          break;
        case EnumFilterAuswertung.Reparatur:
          if (tcReparatur.SelectedIndex == 0)
            _Report.RegisterData(_JgReparatur.Daten, "Daten");
          else
          {
            _Report.RegisterData(_ListeReparaturAuswahl.Daten, "Daten");
            _Report.SetParameterValue("DatumVon", _DzReparaturVon.AnzeigeDatumZeit);
            _Report.SetParameterValue("DatumBis", _DzReparaturBis.AnzeigeDatumZeit);
          }
          _Report.SetParameterValue("IstAktuell", tcReparatur.SelectedIndex == 0);
          break;
        default:
          break;
      }

      if (vorgang == 4) // Neuer Report
      {
        var repName = "";

        var formNeu = new Fenster.FormNeuerReport();
        if (!formNeu.ShowDialog() ?? false)
          return;
        repName = formNeu.ReportName;

        string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        _AktAuswertung = new tabAuswertung()
        {
          Id = Guid.NewGuid(),
          FilterAuswertung = _Auswahl,
          ReportName = repName,
          ErstelltDatum = DateTime.Now,
          ErstelltName = username,
          GeaendertDatum = DateTime.Now,
          GeaendertName = username
        };

        switch (_Auswahl)
        {
          case EnumFilterAuswertung.Arbeitszeit: _ListeReporteArbeitszeit.Add(_AktAuswertung); break;
          case EnumFilterAuswertung.Anmeldung: _ListeReporteAnmedlung.Add(_AktAuswertung); break;
          case EnumFilterAuswertung.Bauteil: _ListeReporteBauteil.Add(_AktAuswertung); break;
          case EnumFilterAuswertung.Reparatur: _ListeReporteReparatur.Add(_AktAuswertung); break;
        }

        _Report.Design();
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

    private void btnFastCube_Click(object sender, RoutedEventArgs e)
    {
      var dat = Helper.StartVerzeichnis() + @"\jgFastCube";
      if (!Directory.Exists(dat))
        Directory.CreateDirectory(dat);

      var prop = Properties.Settings.Default;
      JgMaschineLib.Imports.JgFastCube.JgFastCubeStart(prop.JgCubeVerbindungsString, prop.JgCubeSqlText, dat);
    }
  }
}
