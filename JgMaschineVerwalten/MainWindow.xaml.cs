using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineVerwalten.Commands;
using JgMaschineVerwalten.Fenster;
using JgZeitHelper;
using Microsoft.Win32;

namespace JgMaschineVerwalten
{
  public partial class MainWindow : Window
  {
    private JgModelContainer _Db;
    private tabStandort _Standort;

    private EnumFilterAuswertung _Auswahl { get { return (EnumFilterAuswertung)Convert.ToByte((tcVerwaltung.SelectedItem as TabItem).Tag); } }

    private JgEntityList<tabBediener> _ListeBediener = null;
    private JgEntityList<tabMaschine> _ListeMaschinen = null;
    private JgEntityList<tabAnmeldungMaschine> _ListeAnmeldungen = null;
    private JgEntityList<tabReparatur> _ListeReparaturen = null;

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
    private Guid[] _IdisMaschinen { get { return _ListeMaschinen.Daten.Select(s => s.Id).ToArray(); } }

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

    private JgEntityList<tabAuswertung> _ListeReporteAnmeldung;
    private JgEntityList<tabAuswertung> _ListeReporteBauteil;
    private JgEntityList<tabAuswertung> _ListeReporteReparatur;

    public MainWindow()
    {
      InitializeComponent();
      Helper.FensterEinstellung(this, Properties.Settings.Default);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _Db = new JgModelContainer();
      _Standort = await _Db.tabStandortSet.FirstOrDefaultAsync(f => f.Id == Properties.Settings.Default.IdStandort);
      if (_Standort == null)
        _Standort = await _Db.tabStandortSet.FirstOrDefaultAsync();

      tblStandort.Text = _Standort.Bezeichnung;

      InitListen();

      CommandBindings.Add(new CommandBinding(MyCommands.AnmeldungReparaturBediener, (sen, erg) =>
      {
        var idisBedienerRep = _ListeReparaturen.Current.sAnmeldungen.Select(s => s.eBediener.Id).ToArray();
        var offeneBediener = _ListeBediener.Daten.Where(w => !idisBedienerRep.Contains(w.Id)).OrderBy(o => o.Name).ToList();

        var form = new FormNeuerBedienerReparatur(offeneBediener);
        if (form.ShowDialog() ?? false)
        {
          _Db.tabAnmeldungReparaturSet.Add(form.Anmeldung);
          form.Anmeldung.eReparatur = _ListeReparaturen.Current;
          _Db.SaveChanges();
          ((CollectionViewSource)FindResource("vsReparaturAktuellBediener")).View.Refresh();
          ((CollectionViewSource)FindResource("vsReparaturAktuellBediener")).View.MoveCurrentTo(form.Anmeldung);
        }
      }, (sen, erg) =>
      {
        erg.CanExecute = _ListeReparaturen?.Current != null;
      }));

      _Report = new FastReport.Report()
      {
        FileName = "Datenbank"
      };
      _ReportSettings.CustomSaveReport += (obj, repEvent) =>
      {
        MemoryStream memStr = new MemoryStream();
        try
        {
          repEvent.Report.Save(memStr);
          _Db.tabAuswertungSet.Attach(_AktAuswertung);
          _AktAuswertung.Report = memStr.ToArray();
          _AktAuswertung.GeaendertDatum = DateTime.Now;
          _AktAuswertung.GeaendertName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
          _Db.SaveChanges();
        }
        catch (Exception f)
        {
          Helper.InfoBox("Fehler beim speichern des Reports!", f);
        }
        finally
        {
          memStr.Dispose();
        }
      };
    }

    private void InitListen()
    {
      var von = DateTime.Now.Date;
      var bis = von.AddDays(1).AddSeconds(-1);

      _ListeBediener = new JgEntityList<tabBediener>(_Db)
      {
        OnDatenLaden = (d, p) =>
        {
          var idStandort = (Guid)p.Params["idStandort"];
          return d.tabBedienerSet.Where(w => (w.Status != EnumStatusBediener.Stillgelegt)).ToList();
        }
      };
      _ListeBediener.Parameter["idStandort"] = _Standort.Id;
      _ListeBediener.DatenLaden();

      _ListeMaschinen = new JgEntityList<tabMaschine>(_Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsMaschine"),
        OnDatenLaden = (d, p) =>
        {
          var idStandort = (Guid)p.Params["idStandort"];
          return  d.tabMaschineSet.Where(w => (w.fStandort == idStandort) && (!w.DatenAbgleich.Geloescht) && (w.Status != EnumStatusMaschine.Stillgelegt))
            .OrderBy(o => o.MaschinenName).ToList();
        }
      };
      _ListeMaschinen.Parameter["idStandort"] = _Standort.Id;    
      _ListeMaschinen.DatenLaden();

      // Aktuelle Anmeldungen an Maschine *****************************************************

      _ListeAnmeldungen = new JgEntityList<tabAnmeldungMaschine>(_Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsAnmeldungAktuell"),
        Tabellen = new DataGrid[] { dgAnmeldungAktuell },
        OnDatenLaden = (d, p) =>
        {
          var idisMaschinen = (Guid[])p.Params["IdisMaschinen"];
          var dat = d.tabAnmeldungMaschineSet.Where(w => (w.fAktivMaschine != null) && idisMaschinen.Contains((Guid)w.fAktivMaschine));

          if (!p.IstSortiert)
            dat = dat.OrderBy(o => o.Anmeldung);

          return dat.ToList();
        }
      };
      _ListeAnmeldungen.Parameter["IdisMaschinen"] = _IdisMaschinen;
      _ListeAnmeldungen.DatenLaden();

      // Auswahl Anmeldungenan an Maschinen ********************************************************

      _DzAnmeldungVon.AnzeigeDatumZeit = von;
      _DzAnmeldungBis.AnzeigeDatumZeit = bis;

      _ListeAnmeldungAuswahl = new JgEntityList<tabAnmeldungMaschine>(_Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsAnmeldungAuswahl"),
        Tabellen = new DataGrid[] { dgAnmeldungAuswahl },
        OnDatenLaden = (d, p) =>
        {
          var idisMaschinen = (Guid[])p.Params["IdisMaschinen"];
          var datVon = (DateTime)p.Params["DatumVon"];
          var datBis = (DateTime)p.Params["DatumBis"];

          var dat = d.tabAnmeldungMaschineSet.Where(w => idisMaschinen.Contains(w.fMaschine) && (w.Anmeldung >= datVon) && ((w.Anmeldung <= datBis)));

          if (!p.IstSortiert)
            dat = dat.OrderBy(o => o.Anmeldung);
          
          return dat.ToList();
        }
      };

      // Auswahl Bauteile initialisieren ****************************

      _DzBauteilVon.AnzeigeDatumZeit = von;
      _DzBauteilBis.AnzeigeDatumZeit = bis;

      _ListeBauteilAuswahl = new JgEntityList<tabBauteil>(_Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsBauteilAuswahl"),
        Tabellen = new DataGrid[] { dgBauteilAuswahl },
        OnDatenLaden = (d, p) =>
        {
          var idMaschine = (Guid)p.Params["IdMaschine"];
          var datVon = (DateTime)p.Params["DatumVon"];
          var datBis = (DateTime)p.Params["DatumBis"];

          var dat = d.tabBauteilSet.Where(w => (w.fMaschine == idMaschine) && (w.DatumStart >= datVon) && (w.DatumStart <= datBis))
            .Include(i => i.sBediener);

          if (!p.IstSortiert)
            dat = dat.OrderBy(o => o.DatumStart);
                    
          return dat.ToList();
        }
      };

      // Aktuelle Reparaturen initialisieren ********************************************************

      _ListeReparaturen = new JgEntityList<tabReparatur>(_Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsReparaturAktuell"),
        Tabellen = new DataGrid[] { dgReparaturAktuell },
        OnDatenLaden = (d, p) =>
        {
          var idisReparaturen = (Guid[])p.Params["IdisReparaturen"];

          var dat = d.tabReparaturSet.Where(w => idisReparaturen.Contains(w.Id))
            .Include(i => i.sAnmeldungen).Include(i => i.sAnmeldungen.Select(s => s.eBediener));

          if (!p.IstSortiert)
            dat = dat.OrderBy(o => o.VorgangBeginn);

          return dat.ToList();
        }
      };

      // Auswahl Reparaturen initialisieren ********************************************************

      _DzReparaturVon.AnzeigeDatumZeit = von;
      _DzReparaturBis.AnzeigeDatumZeit = bis;

      _ListeReparaturAuswahl = new JgEntityList<tabReparatur>(_Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsReparaturAuswahl"),
        Tabellen = new DataGrid[] { dgReparaturAuswahl },
        OnDatenLaden = (d, p) =>
        {
          var idisMaschinen = (Guid[])p.Params["IdisMaschinen"];
          var datVon = (DateTime)p.Params["DatumVon"];
          var datBis = (DateTime)p.Params["DatumBis"];

          var dat = d.tabReparaturSet.Where(w => idisMaschinen.Contains(w.fMaschine) && (w.VorgangBeginn >= datVon) && (w.VorgangBeginn <= datBis))
            .Include(i => i.sAnmeldungen).Include(i => i.sAnmeldungen.Select(s => s.eBediener));

          if (!p.IstSortiert)
            dat = dat.OrderBy(o => o.VorgangBeginn);

          return dat.ToList();
        }
      };

      // Auswertung initialisieren ********************************

      var filterAuswertungen = new EnumFilterAuswertung[] { EnumFilterAuswertung.Anmeldung, EnumFilterAuswertung.Bauteil, EnumFilterAuswertung.Reparatur };
      var reporte = _Db.tabAuswertungSet.Where(w => filterAuswertungen.Contains(w.FilterAuswertung) && (!w.DatenAbgleich.Geloescht)).ToList();

      _ListeReporteAnmeldung = new JgEntityList<tabAuswertung>(_Db)
      {
        ViewSource = (CollectionViewSource)this.FindResource("vsReporteAnmeldung"),
        Daten = reporte.Where(w => w.FilterAuswertung == EnumFilterAuswertung.Anmeldung).OrderBy(o => o.AnzeigeReportname).ToList()
      };

      _ListeReporteBauteil = new JgEntityList<tabAuswertung>(_Db)
      {
        ViewSource = (CollectionViewSource)this.FindResource("vsReporteBauteil"),
        Daten = reporte.Where(w => w.FilterAuswertung == EnumFilterAuswertung.Bauteil).OrderBy(o => o.AnzeigeReportname).ToList()
      };

      _ListeReporteReparatur = new JgEntityList<tabAuswertung>(_Db)
      {
        ViewSource = (CollectionViewSource)this.FindResource("vsReporteReparatur"),
        Daten = reporte.Where(w => w.FilterAuswertung == EnumFilterAuswertung.Reparatur).OrderBy(o => o.AnzeigeReportname).ToList()
      };
    }

    private void TreeViewMaschinenAktualisieren()
    {
      var maschine = _Maschine;
      _ListeMaschinen.Refresh();
      GeheZu(maschine?.Id);
    }

    private void GeheZu(Guid? Id)
    {
      if (Id == null)
        return;

      try
      {
        foreach (var dsMaschine in treeViewMaschinen.Items)
        {
          if ((dsMaschine as tabMaschine).Id == Id)
          {
            (treeViewMaschinen.ItemContainerGenerator.ContainerFromItem(dsMaschine) as TreeViewItem).IsSelected = true;
            return;
          }
          var itemMaschine = (TreeViewItem)treeViewMaschinen.ItemContainerGenerator.ContainerFromItem(dsMaschine);
          foreach (var dsAnmeldung in itemMaschine.Items)
          {
            if ((dsAnmeldung as tabAnmeldungMaschine).Id == Id)
            {
              (itemMaschine.ItemContainerGenerator.ContainerFromItem(dsAnmeldung) as TreeViewItem).IsSelected = true;
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
        Helper.InfoBox("Bitte Maschine in linker Tabelle auswahlen !", Helper.ProtokollArt.Warnung);
      else if (_Maschine.fAktivReparatur != null)
        MessageBox.Show($"Die Maschine {_Maschine.MaschinenName} ist bereits im Reparaturmodus.", "Information !", MessageBoxButton.OK, MessageBoxImage.Information);
      else
      {
        var form = new Fenster.FormReparatur(null, _ListeBediener.Daten, _Maschine);
        if (_ListeReparaturen.ErgebnissFormular(form.ShowDialog(), true, form.Reparatur))
        {
          _Maschine.eAktivReparatur = form.Reparatur;

          foreach (var anmeldungMaschine in _Maschine.sAktiveAnmeldungen)
          {
            var anmledRep = new tabAnmeldungReparatur()
            {
              Id = Guid.NewGuid(),
              Anmeldung = form.Reparatur.VorgangBeginn,
              eReparatur = form.Reparatur,
              eBediener = anmeldungMaschine.eBediener
            };
            _Db.tabAnmeldungReparaturSet.Add(anmledRep);
          }

          TreeViewMaschinenAktualisieren();
          ((CollectionViewSource)FindResource("vsReparaturAktuellBediener")).View.Refresh();
          _Db.SaveChanges();
        }
      }
    }

    private void ReparaturBearbeitenAktuell_Click(object sender, RoutedEventArgs e)
    {
      Fenster.FormReparatur form = new Fenster.FormReparatur(_ListeReparaturen.Current, _ListeBediener.Daten);

      if (form.ShowDialog() ?? false)
        _ListeReparaturen.DsSave();
      else
        _ListeReparaturen.Reload();
    }

    private void ReparaturBeenden_Click(object sender, RoutedEventArgs e)
    {
      var reparatur = _ListeReparaturen.Current;
      var anzeigeText = $"Maschine {reparatur.eMaschine.MaschinenName} mit Vorgang {reparatur.Vorgang} abmelden?";
      var ergZeitAbfrage = (DateTime?)DateTime.Now;

      if (JgZeit.AbfrageZeit(anzeigeText, "Abmeldung", ref ergZeitAbfrage, false))
      {
        var repBedienerAustragen = reparatur.sAnmeldungen.Where(w => w.IstAktiv).ToList();
        foreach (var bediener in repBedienerAustragen)
          bediener.AnzeigeAbmeldung = ergZeitAbfrage;

        reparatur.AnzeigeVorgangEnde = ergZeitAbfrage;
        reparatur.eMaschine.fAktivReparatur = null;
        _ListeReparaturen.DsSave();
        _ListeReparaturen.Remove();

        TreeViewMaschinenAktualisieren();
      }
    }

    private void ReparaturBearbeitenAuswahl_Click(object sender, RoutedEventArgs e)
    {
      var form = new FormReparatur(_ListeReparaturAuswahl.Current, _ListeBediener.Daten);
      _ListeReparaturAuswahl.ErgebnissFormular(form.ShowDialog(), false, form.Reparatur);
    }

    private void ReparaturAnmeldungAbmeldung_Click(object sender, RoutedEventArgs e)
    {
      var cvsRepBenutzer = (CollectionViewSource)FindResource("vsReparaturAktuellBediener");
      var dsRepBenutzer = (tabAnmeldungReparatur)cvsRepBenutzer.View.CurrentItem;

      string anzeigeText = $"Möchten Sie den Bediener {dsRepBenutzer.eBediener.Name} von der Reparatur an Maschine {dsRepBenutzer.eReparatur.eMaschine.MaschinenName} abmelden ?";
      var zeitAbmeldung = (DateTime?)(dsRepBenutzer.Abmeldung ?? DateTime.Now);

      if (JgZeit.AbfrageZeit(anzeigeText, "Abmeldung", ref zeitAbmeldung, false))
      {
        dsRepBenutzer.Abmeldung = zeitAbmeldung;
        _Db.SaveChanges();
      }
    }

    private void ReparaturAnmeldungAuswahlBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var colView = (CollectionViewSource)FindResource("vsReparaturAuswahlBediener");
      var anmeldung = (tabAnmeldungReparatur)colView.View.CurrentItem;
      string anzeigeText = $"Reparaturzeiten für den Bediener {anmeldung.eBediener.Name} an Maschin {anmeldung.eReparatur.eMaschine.MaschinenName} bearbeiten.";

      var zeitAnmeldung = (DateTime?)anmeldung.Anmeldung;
      var zeitAbmeldung = anmeldung.Abmeldung;

      if (JgZeit.AbfrageZeit(anzeigeText, "Anmeldung Maschine bearbeiten", ref zeitAnmeldung, false, ref zeitAbmeldung, true))
      {
        var safe = false;
        if (anmeldung.Anmeldung != zeitAnmeldung)
        {
          anmeldung.Anmeldung = zeitAnmeldung.Value;
          safe = true;
        }
        if (anmeldung.Abmeldung != zeitAbmeldung)
        {
          anmeldung.Abmeldung = zeitAbmeldung;
          safe = true;
        }
        if (safe)
        {
          _Db.SaveChanges();
          colView.View.Refresh();
          colView.View.MoveCurrentTo(anmeldung);
        }
      }
    }

    #endregion

    #region Anmeldungen Maschine ***************************************************

    private void NeueAnmeldungMaschine_Click(object sender, RoutedEventArgs e)
    {
      var form = new FormNeueAnmeldung(_ListeBediener.Daten, _ListeMaschinen.Daten, _Maschine);
      if (_ListeAnmeldungen.ErgebnissFormular(form.ShowDialog(), true, form.AnmeldungMaschine))
      {
        _ListeMaschinen.Refresh();
        GeheZu(form.AnmeldungMaschine.fMaschine);
      }
    }

    private void BanutzerMaschineAbmelden_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = _ListeAnmeldungen.Current;
      var msg = $"Möchten Sie den Bediener {anmeldung.eBediener.Name} von der Maschine {anmeldung.eMaschine.MaschinenName} abmelden ?";
      var zeitAbmeldung = (DateTime?)(anmeldung.Abmeldung ?? DateTime.Now);

      if (JgZeit.AbfrageZeit(msg, "Abmeldung", ref zeitAbmeldung, false))
      {
        anmeldung.AnzeigeAbmeldung = zeitAbmeldung;
        anmeldung.ManuelleAbmeldung = true;
        anmeldung.eAktivMaschine = null;
        _ListeAnmeldungen.Remove(anmeldung);

        // Von eventuellen Reparaturen abmelden
        var rep = _ListeReparaturen.Daten.SelectMany(s => s.sAnmeldungen).Where(w => w.IstAktiv).FirstOrDefault(f => f.fBediener == anmeldung.fBediener);
        if (rep != null)
          rep.Abmeldung = zeitAbmeldung;

        _ListeAnmeldungen.DsSave(anmeldung);

        GeheZu(_Maschine?.Id);
        _ListeMaschinen.Refresh();
      }
    }

    private void AnmeldungBenutzerBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = _ListeAnmeldungAuswahl.Current;
      var msg = $"Korrektur der Arbeitszeit für den Mitarbeiter {anmeldung.eBediener.Name}.";
      var zeitAnmeldung = (DateTime?)anmeldung.Anmeldung;
      var zeitAbmeldung = anmeldung.Abmeldung;

      if (JgZeit.AbfrageZeit(msg, "Berichtigung Arbeitszeit", ref zeitAnmeldung, false, ref zeitAbmeldung, true))
      {
        bool safe = false;
        if (zeitAnmeldung != anmeldung.Anmeldung)
        {
          anmeldung.AnzeigeAnmeldung = zeitAnmeldung.Value;
          anmeldung.ManuelleAnmeldung = true;
          safe = true;
        }
        if (zeitAbmeldung != anmeldung.Abmeldung)
        {
          anmeldung.AnzeigeAbmeldung = zeitAbmeldung;
          anmeldung.ManuelleAbmeldung = true;
          safe = true;
        }
        if (safe)
          _ListeAnmeldungAuswahl.DsSave();
      } 
    }

    #endregion

    private void ButtonOptionen_Click(object sender, RoutedEventArgs e)
    {
      FormOptionen form = new FormOptionen(_Db);
      if (form.ShowDialog() ?? false)
      {
        _Standort = form.StandOrt;
        tblStandort.Text = _Standort.Bezeichnung;
        _ListeMaschinen.Parameter["idStandort"] = _Standort.Id;
        _ListeMaschinen.DatenAktualisieren();
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      Properties.Settings.Default.Save();
    }

    private void BtnAuswahlAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      switch ((sender as Button).Tag.ToString())
      {
        case "0":
          _ListeAnmeldungAuswahl.Parameter["IdisMaschinen"] = _IdisMaschinen;
          _ListeAnmeldungAuswahl.Parameter["DatumVon"] = _DzAnmeldungVon.AnzeigeDatumZeit;
          _ListeAnmeldungAuswahl.Parameter["DatumBis"] = _DzAnmeldungBis.AnzeigeDatumZeit;
          _ListeAnmeldungAuswahl.DatenAktualisieren();
          break;
        case "1":
          if (_Maschine == null)
          {
            Helper.InfoBox("Bitte wählen Sie eine Maschine aus.", Helper.ProtokollArt.Info);
            return;
          }

          _ListeBauteilAuswahl.Parameter["IdMaschine"] = _Maschine.Id;
          _ListeBauteilAuswahl.Parameter["DatumVon"] = _DzBauteilVon.AnzeigeDatumZeit;
          _ListeBauteilAuswahl.Parameter["DatumBis"] = _DzBauteilBis.AnzeigeDatumZeit;
          _ListeBauteilAuswahl.DatenAktualisieren();

          foreach(var bt in _ListeBauteilAuswahl.Daten)
          {
            if (bt.BvbsDaten == null)
              bt.LoadBvbsDaten(false);
          }

          break;
        case "2":
          _ListeReparaturAuswahl.Parameter["IdisMaschinen"] = _IdisMaschinen;
          _ListeReparaturAuswahl.Parameter["DatumVon"] = _DzReparaturVon.AnzeigeDatumZeit;
          _ListeReparaturAuswahl.Parameter["DatumBis"] = _DzReparaturBis.AnzeigeDatumZeit;
          _ListeReparaturAuswahl.DatenAktualisieren();
          break;
      }
    }

    private void BtnDrucken_Click(object sender, RoutedEventArgs e)
    {
      var vorgang = Convert.ToInt32((sender as Button).Tag);  // 1 - Anzeigen, 2 - Drucken, 3 - Design, 4 - Neuer Report, 5 - Report Exportieren, 6 - Löschen

      if (vorgang != 4)
      {
        switch (_Auswahl)
        {
          case EnumFilterAuswertung.Anmeldung: _AktAuswertung = (tabAuswertung)_ListeReporteAnmeldung.Current; break;
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
            SaveFileDialog dia = new SaveFileDialog()
            {
              Filter = "Fastreport (*.frx)|*.frx|Alle Dateien (*.*)|*.*",
              FilterIndex = 1
            };
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
                case EnumFilterAuswertung.Anmeldung: _ListeReporteAnmeldung.Delete(_AktAuswertung); break;
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
        case JgMaschineData.EnumFilterAuswertung.Anmeldung:
          if (tcAnmeldung.SelectedIndex == 0)
            _Report.RegisterData(_ListeAnmeldungen.Daten, "Daten");
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
            _Report.RegisterData(_ListeReparaturen.Daten, "Daten");
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
          case EnumFilterAuswertung.Anmeldung: _ListeReporteAnmeldung.Add(_AktAuswertung); break;
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

    private void BtnFastCube_Click(object sender, RoutedEventArgs e)
    {
      var dat = Helper.StartVerzeichnis() + @"\jgFastCube";
      if (!Directory.Exists(dat))
        Directory.CreateDirectory(dat);

      var prop = Properties.Settings.Default;
      JgMaschineLib.Imports.JgFastCube.JgFastCubeStart(prop.JgCubeVerbindungsString, prop.JgCubeSqlText, dat);
    }

    private Guid[] _IdisMaschinenInReparatur
    {
      get { return _ListeMaschinen.Daten.Where(w => w.eAktivReparatur != null).Select(s => (Guid)s.fAktivReparatur).ToArray(); }
    }

    private void BtnAktuellAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      switch ((sender as Button).Tag.ToString())
      {
        case "0": // Treeview aktualisieren
          TreeViewMaschinenAktualisieren();
          break;
        case "1": // Anweldung aktualisieren
          _ListeAnmeldungen.Parameter["IdisMaschinen"] = _IdisMaschinen;
          _ListeAnmeldungen.DatenAktualisieren ();
          break;
        case "2": // Reparaturen aktualisieren
          _ListeReparaturen.Parameter["IdisReparaturen"] = _IdisMaschinenInReparatur;
          _ListeReparaturen.DatenAktualisieren();
          break;
      }
    }

    private void TabItemFocusReparatur(object sender, RoutedEventArgs e)
    {
      if (_ListeReparaturen.ErsterDurchlauf)
      {
        _ListeReparaturen.Parameter["IdisReparaturen"] = _IdisMaschinenInReparatur;
        _ListeReparaturen.DatenLaden();
      }
    }
  }
}
