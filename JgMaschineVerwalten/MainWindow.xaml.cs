using JgMaschineLib.Zeit;
using JgMaschineVerwalten.Commands;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JgMaschineVerwalten
{
  public partial class MainWindow : Window
  {
    JgMaschineData.JgModelContainer _Db;

    private JgMaschineData.tabStandort _Standort;
    private JgMaschineLib.JgListe<JgMaschineData.tabMaschine> _ListeMaschinen;

    private JgMaschineData.tabMaschine _Maschine { get { return _ListeMaschinen.AktDatensatz; } }

    private JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit> _ListeArbeitszeitAktuell;
    private JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit> _ListeArbeitszeitAuswahl;
    private JgDatumZeit _DzArbeitszeitVon { get { return (JgDatumZeit)this.FindResource("dzArbeitszeitVon"); } }
    private JgDatumZeit _DzArbeitszeitBis { get { return (JgDatumZeit)this.FindResource("dzArbeitszeitBis"); } }


    private JgMaschineLib.JgListe<JgMaschineData.tabAnmeldungMaschine> _ListeAnmeldungAktuell;
    private JgMaschineLib.JgListe<JgMaschineData.tabAnmeldungMaschine> _ListeAnmeldungAuswahl;
    private JgDatumZeit _DzAnmeldungVon { get { return (JgDatumZeit)this.FindResource("dzAnmeldungVon"); } }
    private JgDatumZeit _DzAnmeldungBis { get { return (JgDatumZeit)this.FindResource("dzAnmeldungBis"); } }
    private CollectionViewSource _VsBenutzerAnmeldung { get { return (CollectionViewSource)FindResource("vsAnmeldungBenutzer"); } }

    private JgDatumZeit _DzBauteilVon { get { return (JgDatumZeit)this.FindResource("dzReparaturVon"); } }
    private JgDatumZeit _DzBauteilBis { get { return (JgDatumZeit)this.FindResource("dzReparaturBis"); } }
    private JgMaschineLib.JgListe<JgMaschineData.tabBauteil> _ListeBauteilAuswahl;

    private JgDatumZeit _DzReparaturVon { get { return (JgDatumZeit)this.FindResource("dzReparaturVon"); } }
    private JgDatumZeit _DzReparaturBis { get { return (JgDatumZeit)this.FindResource("dzReparaturBis"); } }
    private JgMaschineLib.JgListe<JgMaschineData.tabReparatur> _ListeReparaturAktuell;
    private JgMaschineLib.JgListe<JgMaschineData.tabReparatur> _ListeReparaturAuswahl;

    private System.Threading.Timer _AktualisierungsTimer;

    private FastReport.Report _Report;
    private FastReport.EnvironmentSettings _ReportSettings = new FastReport.EnvironmentSettings();
    private JgMaschineData.tabAuswertung _AktAuswertung = null;

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _Db = new JgMaschineData.JgModelContainer();

      tblDatum.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

      var bis = DateTime.Now.Date;
      var von = bis.AddDays(-10);
      bis = new DateTime(bis.Year, bis.Month, bis.Day, 23, 59, 59);

      // Standort initialieren

      _Standort = await _Db.tabStandortSet.FindAsync(Properties.Settings.Default.IdStandort);
      if (_Standort == null)
        _Standort = await _Db.tabStandortSet.FirstOrDefaultAsync();
      tblStandort.Text = _Standort.Bezeichnung;

      // Maschine Initialisieren

      IQueryable<JgMaschineData.tabMaschine> iqMaschine = _Db.tabMaschineSet.Where(w => (w.fStandort == _Standort.Id) && (w.Status != JgMaschineData.EnumStatusMaschine.Stillgelegt)).OrderBy(o => o.MaschinenName);
      var vs = (CollectionViewSource)FindResource("vsMaschine");
      _ListeMaschinen = new JgMaschineLib.JgListe<JgMaschineData.tabMaschine>(_Db, iqMaschine, vs);
      await _ListeMaschinen.Init();
      vs.View.CurrentChanged += (sen, erg) => { MaschineDatenAktualisieren(); };

      // Arbeitszeit Initialisieren

      cmbBenutzerArbeitszeit.ItemsSource = await _Db.tabBedienerSet.Where(w => w.Status != JgMaschineData.EnumStatusBediener.Stillgelegt).OrderBy(o => o.NachName).ToListAsync();

      IQueryable<JgMaschineData.tabArbeitszeit> iqArbeitszeitAktuell = _Db.tabArbeitszeitSet.Where(w => (w.fStandort == _Standort.Id) && w.IstAktiv).OrderBy(o => o.Anmeldung);
      vs = (CollectionViewSource)FindResource("vsArbeitszeitAktuell");
      _ListeArbeitszeitAktuell = new JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit>(_Db, iqArbeitszeitAktuell, vs, dgArbeitszeitAuswahl);
      await _ListeArbeitszeitAktuell.Init();

      _DzArbeitszeitVon.DatumZeit = von;
      _DzArbeitszeitBis.DatumZeit = bis;

      IQueryable<JgMaschineData.tabArbeitszeit> iqArbeitszeitAuswahl = _Db.tabArbeitszeitSet.Where(w => (w.fStandort == _Standort.Id) && (!w.IstAktiv) && (w.Anmeldung >= _DzArbeitszeitVon.DatumZeit) && ((w.Anmeldung <= _DzArbeitszeitBis.DatumZeit))).OrderBy(o => o.Anmeldung);
      vs = (CollectionViewSource)FindResource("vsArbeitszeitAuswahl");
      _ListeArbeitszeitAuswahl = new JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit>(_Db, iqArbeitszeitAuswahl, vs, dgArbeitszeitAuswahl);
      await _ListeArbeitszeitAuswahl.Init();

      // Anmeldungen initialisieren

      _VsBenutzerAnmeldung.Source = _ListeArbeitszeitAktuell;
      var maschinenIds = _ListeMaschinen.Select(s => s.Id).ToArray();

      IQueryable<JgMaschineData.tabAnmeldungMaschine> iqAnmeldungAktuell = _Db.tabAnmeldungMaschineSet.Where(w => (maschinenIds.Contains(w.fMaschine)) && w.IstAktiv).OrderBy(o => o.Anmeldung);
      vs = (CollectionViewSource)FindResource("vsAnmeldungAktuell");
      _ListeAnmeldungAktuell = new JgMaschineLib.JgListe<JgMaschineData.tabAnmeldungMaschine>(_Db, iqAnmeldungAktuell, vs, dgAnmeldungAktuell);
      await _ListeAnmeldungAktuell.Init();

      _DzAnmeldungVon.DatumZeit = von;
      _DzAnmeldungBis.DatumZeit = bis;

      IQueryable<JgMaschineData.tabAnmeldungMaschine> iqAnmeldungAuswahl = _Db.tabAnmeldungMaschineSet.Where(w => (maschinenIds.Contains(w.fMaschine)) && (!w.IstAktiv) && (w.Anmeldung >= _DzAnmeldungVon.DatumZeit) && ((w.Anmeldung <= _DzAnmeldungBis.DatumZeit))).OrderBy(o => o.Anmeldung);
      vs = (CollectionViewSource)FindResource("vsAnmeldungAuswahl");
      _ListeAnmeldungAuswahl = new JgMaschineLib.JgListe<JgMaschineData.tabAnmeldungMaschine>(_Db, iqAnmeldungAuswahl, vs, dgAnmeldungAuswahl);
      await _ListeAnmeldungAuswahl.Init();

      // Bauteile initialisieren

      _DzBauteilVon.DatumZeit = von;
      _DzBauteilBis.DatumZeit = bis;

      IQueryable<JgMaschineData.tabBauteil> iqBauteilAuswahl = _Db.tabBauteilSet.Where(w => (w.fMaschine == _ListeMaschinen.AktDatensatz.Id) && (w.DatumStart >= _DzBauteilVon.DatumZeit) && (w.DatumStart <= _DzBauteilBis.DatumZeit)).OrderBy(o => o.DatumStart);
      vs = (CollectionViewSource)FindResource("vsBauteilAuswahl");
      _ListeBauteilAuswahl = new JgMaschineLib.JgListe<JgMaschineData.tabBauteil>(_Db, iqBauteilAuswahl, vs, dgBauteilAuswahl);
      await _ListeBauteilAuswahl.Init();

      // Reparaturen initialisieren 

      IQueryable<JgMaschineData.tabReparatur> iqReparaturAktuell = _Db.tabReparaturSet.Where(w => (w.fMaschine == _ListeMaschinen.AktDatensatz.Id) && w.IstAktiv).OrderBy(o => o.VorgangBeginn);
      vs = (CollectionViewSource)FindResource("vsReparaturAktuell");
      _ListeReparaturAktuell = new JgMaschineLib.JgListe<JgMaschineData.tabReparatur>(_Db, iqReparaturAktuell, vs, dgReparaturAktuell);
      await _ListeReparaturAktuell.Init();

      _DzReparaturVon.DatumZeit = von;
      _DzReparaturBis.DatumZeit = bis;

      IQueryable<JgMaschineData.tabReparatur> iqReparaturAuswahl = _Db.tabReparaturSet.Where(w => (w.fMaschine == _ListeMaschinen.AktDatensatz.Id) && (!w.IstAktiv) && (w.VorgangBeginn >= _DzReparaturVon.DatumZeit) && (w.VorgangBeginn <= _DzReparaturBis.DatumZeit)).OrderBy(o => o.VorgangBeginn);
      vs = (CollectionViewSource)FindResource("vsReparaturAuswahl");
      _ListeReparaturAuswahl = new JgMaschineLib.JgListe<JgMaschineData.tabReparatur>(_Db, iqReparaturAuswahl, vs, dgReparaturAuswahl);
      await _ListeReparaturAuswahl.Init();

      CommandBindings.Add(new CommandBinding(MyCommands.ReparaturNeu, ExecuteRepataurNeu, CanExecuteRepataurNeu));
      CommandBindings.Add(new CommandBinding(MyCommands.AnmeldungBediener, ExecuteAnmeldungBenutzerAnmeldung, CanExecuteBedienerAngemeldetMaschineVorhanden));

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
          JgMaschineLib.DbSichern.AbgleichEintragen(_AktAuswertung.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
          _Db.SaveChanges();
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

      var auswertungen = await _Db.tabAuswertungSet.Where(w => w.FilterAuswertung != JgMaschineData.EnumFilterAuswertung.Allgemein).ToListAsync();
      cmbDruckArbeitszeit.ItemsSource = auswertungen.Where(w => w.FilterAuswertung == JgMaschineData.EnumFilterAuswertung.Arbeitszeit).Select(s => s.ReportName).OrderBy(o => o);

      _AktualisierungsTimer = new System.Threading.Timer((obj) =>
      {
        Dispatcher.Invoke((Action)delegate() { });
      }, null, 60000, 60000);
    }

    private async void MaschineDatenAktualisieren()
    {
      await _ListeReparaturAktuell.DatenGenerierenAsync();
      await _ListeReparaturAuswahl.DatenGenerierenAsync();
    }

    #region Formular Reparatur

    private void ExecuteRepataurNeu(object sender, ExecutedRoutedEventArgs e)
    {
      JgMaschineVerwalten.Fenster.FormReparatur form = new Fenster.FormReparatur(_Db, null);
      if (form.ShowDialog() ?? false)
      {
        form.Reparatur.eMaschine = (JgMaschineData.tabMaschine)_ListeMaschinen.AktDatensatz;
        _ListeReparaturAktuell.Add(form.Reparatur);
      };
    }

    private void CanExecuteRepataurNeu(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = _ListeMaschinen.AktDatensatz != null;
    }

    private void ReparaturBearbeitenAktuell_Click(object sender, RoutedEventArgs e)
    {
      Fenster.FormReparatur form = new Fenster.FormReparatur(_Db, _ListeReparaturAktuell.AktDatensatz);
      if (form.ShowDialog() ?? false)
      {
        _ListeReparaturAktuell.Refresh();
        JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabReparatur>(_Db, form.Reparatur, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
      }
    }

    private void ReparaturBearbeitenAuswahl_Click(object sender, RoutedEventArgs e)
    {
      Fenster.FormReparatur form = new Fenster.FormReparatur(_Db, _ListeReparaturAuswahl.AktDatensatz);
      if (form.ShowDialog() ?? false)
      {
        _ListeReparaturAuswahl.Refresh();
        JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabReparatur>(_Db, form.Reparatur, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
      }
    }

    private async void ReparaturBeenden_Click(object sender, RoutedEventArgs e)
    {
      var rep = _ListeReparaturAktuell.AktDatensatz;
      var anzeigeText = string.Format("Maschine {0} mit Ereigniss {1} abmelden.", rep.eMaschine.MaschinenName, rep.Ereigniss);
      JgMaschineLib.Zeit.FormAuswahlDatumZeit form = new FormAuswahlDatumZeit("Abmeldung", anzeigeText, rep.VorgangEnde);
      if (form.ShowDialog() ?? false)
      {
        rep.VorgangEnde = form.DatumZeit;
        rep.IstAktiv = false;
        JgMaschineLib.DbSichern.AbgleichEintragen(rep.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
        _Db.SaveChanges();
        await _ListeReparaturAktuell.DatenGenerierenAsync();
        await _ListeReparaturAuswahl.DatenGenerierenAsync();
      }
    }

    #endregion

    #region Anmeldungen Maschine

    private void ExecuteAnmeldungBenutzerAnmeldung(object sender, ExecutedRoutedEventArgs e)
    {
      var bediener = (_VsBenutzerAnmeldung.View.CurrentItem as JgMaschineData.tabArbeitszeit).eBediener;
      var maschine = _ListeMaschinen.AktDatensatz;

      if (bediener.eAktuelleAnmeldungMaschine == maschine)
        MessageBox.Show("Benutzer ist bereits an Maschine: " + maschine.MaschinenName + " angemeldet!", "Information !", MessageBoxButton.OK, MessageBoxImage.Warning);
      else
      {
        if (bediener.eAktuelleAnmeldungMaschine != null)
          MessageBox.Show("Benutzer ist an Maschine: " + bediener.eAktuelleAnmeldungMaschine.MaschinenName + " angemeldet!\r\nSie müssen Ihn erst von dieser Maschine abemelden.", "Information !", MessageBoxButton.OK, MessageBoxImage.Warning);
        else
        {
          JgFormDatumZeit form = new JgFormDatumZeit();
          if (form.Anzeigen("Anmeldung", "Geben Sie die Zeit an, wo sich der Benutzer angemeldet hat.", DateTime.Now))
          {
            bediener.eAktuelleAnmeldungMaschine = maschine;
            JgMaschineLib.DbSichern.AbgleichEintragen(bediener.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);

            _ListeMaschinen.Refresh();

            var anmeldung = new JgMaschineData.tabAnmeldungMaschine()
            {
              Id = Guid.NewGuid(),
              eBediener = bediener,
              eMaschine = maschine,
              Anmeldung = form.Datum,
              ManuelleAnmeldung = true,
              Abmeldung = form.Datum,
              ManuelleAbmeldung = true,
              IstAktiv = true
            };
            _ListeAnmeldungAktuell.Add(anmeldung);
          }
        }
      }
    }

    private async void AnmeldungBenutzerAbmelden_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = _ListeAnmeldungAktuell.AktDatensatz;
      string anzeigeText = string.Format("Möchten Sie den Bediener {0} von der Maschine {1} abmelden ?", anmeldung.eBediener.Name, anmeldung.eMaschine.MaschinenName);

      JgFormDatumZeit form = new JgFormDatumZeit();
      if (form.Anzeigen("Abmeldung", anzeigeText, DateTime.Now))
      {
        anmeldung.eBediener.eAktuelleAnmeldungMaschine = null;
        JgMaschineLib.DbSichern.AbgleichEintragen(anmeldung.eBediener.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);

        anmeldung.Abmeldung = form.Datum;
        anmeldung.ManuelleAbmeldung = true;
        anmeldung.IstAktiv = false;
        JgMaschineLib.DbSichern.AbgleichEintragen(anmeldung.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
        _Db.SaveChanges();

        _ListeMaschinen.Refresh();

        await _ListeAnmeldungAktuell.DatenGenerierenAsync();
        await _ListeAnmeldungAuswahl.DatenGenerierenAsync();
      }
    }

    private void CanExecuteBedienerAngemeldetMaschineVorhanden(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = (_ListeMaschinen.AktDatensatz != null) && (_ListeArbeitszeitAktuell.AktDatensatz != null);
    }

    #endregion

    private void MaschinenWechseln()
    {
      _ListeMaschinen.DatenGenerieren();
    }

    private async void ButtonOptionen_Click(object sender, RoutedEventArgs e)
    {
      Fenster.FormOptionen form = new Fenster.FormOptionen(_Db);
      if (form.ShowDialog() ?? false)
      {
        _Standort = form.StandOrt;
        tblStandort.Text = _Standort.Bezeichnung;
        await _ListeArbeitszeitAktuell.DatenGenerierenAsync();
        await _ListeArbeitszeitAuswahl.DatenGenerierenAsync();
        MaschinenWechseln();
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      Properties.Settings.Default.Save();
    }

    #region Arbeitszeit verwalten

    private void ArbeitszeitAnmeldung_Click(object sender, RoutedEventArgs e)
    {
      var bediener = (JgMaschineData.tabBediener)cmbBenutzerArbeitszeit.SelectedItem;

      if (bediener == null)
      {
        MessageBox.Show("Wählen Sie einen Mitarbeiter aus.", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      if (_ListeAnmeldungAktuell.FirstOrDefault(f => f.eBediener == bediener) != null)
        MessageBox.Show(string.Format("Bediener {0} bereits angemeldet !", bediener.Name), "Informaation !", MessageBoxButton.OK, MessageBoxImage.Warning);
      else
      {
        string anzeigeText = string.Format("Mitarbeiter {0} im Betrieb anmelden !", bediener.Name);
        FormAuswahlDatumZeit form = new FormAuswahlDatumZeit("Anmeldung", anzeigeText, DateTime.Now);
        if (form.ShowDialog() ?? false)
        {
          var arbeitszeit = new JgMaschineData.tabArbeitszeit()
          {
            Id = Guid.NewGuid(),
            eBediener = bediener,
            eStandort = _Standort,
            Anmeldung = form.DatumZeit,
            ManuelleAnmeldung = true,
            Abmeldung = form.DatumZeit,
            ManuelleAbmeldung = true,
            IstAktiv = true
          };
          _ListeArbeitszeitAktuell.Add(arbeitszeit);
        }
      }
    }

    private async void ArbeitszeitAbmeldung_Click(object sender, RoutedEventArgs e)
    {
      var arbeitszeit = _ListeArbeitszeitAktuell.AktDatensatz;
      var anzeigeText = string.Format("Möchten Sie den Bediener {0} abmelden ?", arbeitszeit.eBediener.Name);
      JgFormDatumZeit form = new JgFormDatumZeit();
      if (form.Anzeigen("Abmeldung bediener", anzeigeText, DateTime.Now))
      {
        arbeitszeit.IstAktiv = false;
        arbeitszeit.Abmeldung = form.Datum;
        arbeitszeit.ManuelleAbmeldung = true;
        JgMaschineLib.DbSichern.AbgleichEintragen(arbeitszeit.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
        _Db.SaveChanges();

        await _ListeArbeitszeitAktuell.DatenGenerierenAsync();
        await _ListeArbeitszeitAuswahl.DatenGenerierenAsync();

        if (arbeitszeit.eBediener.eAktuelleAnmeldungMaschine != null)
        {
          var bediener = arbeitszeit.eBediener;
          bediener.eAktuelleAnmeldungMaschine = null;
          JgMaschineLib.DbSichern.AbgleichEintragen(bediener.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);

          var anmeldung = bediener.sAnmeldungen.FirstOrDefault(f => f.IstAktiv);
          if (anmeldung != null)
          {
            anmeldung.Abmeldung = form.Datum;
            anmeldung.IstAktiv = false;
            anmeldung.ManuelleAbmeldung = true;
            JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabAnmeldungMaschine>(_Db, anmeldung, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
          }
        }
      }
    }

    private void ArbeitszeitBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var arbeitszeit = _ListeArbeitszeitAuswahl.AktDatensatz;
      var anz = string.Format("Korrektur der Arbeitszeit für den Mitarbeiter {0}.", arbeitszeit.eBediener.Name);
      var form = new JgMaschineLib.Zeit.FormAuswahlDatumVonBis("Berichtigung Arbeitszeit", anz, arbeitszeit.Anmeldung, arbeitszeit.Abmeldung);
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
        _ListeArbeitszeitAuswahl.Refresh();

        JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabArbeitszeit>(_Db, arbeitszeit, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
      }
    }

    private void btnAuswahlAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      if (sender == btnArbeitszeitAuswahl)
        _ListeArbeitszeitAuswahl.DatenGenerieren();
      else if (sender == btnAnmeldungAuswahl)
        _ListeAnmeldungAuswahl.DatenGenerieren();
      else if (sender == btnBauteilAuswahl)
        _ListeBauteilAuswahl.DatenGenerieren();
      else if (sender == btnReparaturAuswahl)
        _ListeReparaturAuswahl.DatenGenerieren();
    }

    #endregion

    private void tcArbeitszeit_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
      MessageBox.Show("Test");
    }

    private void btnDrucken_Click(object sender, RoutedEventArgs e)
    {
      JgMaschineData.EnumFilterAuswertung auswahl = (JgMaschineData.EnumFilterAuswertung)Enum.Parse(typeof(JgMaschineData.EnumFilterAuswertung), (sender as Button).Tag.ToString()[0].ToString());
      int vorgang = Convert.ToInt32((sender as Button).Tag.ToString().Substring(1));  // 1 - Anzeigen, 2 - Drucken, 3 - Design, 4 - Neuer Report

      var repName = "FEHLER";
      _Report.Clear();

      if (vorgang < 4)
      {
        switch (auswahl)
        {
          case JgMaschineData.EnumFilterAuswertung.Arbeitszeit: repName = cmbDruckArbeitszeit.SelectedItem == null ? "Fehler" : cmbDruckArbeitszeit.SelectedItem.ToString(); break;
          case JgMaschineData.EnumFilterAuswertung.Anmeldung: repName = cmbDruckAnmeldung.SelectedItem == null ? "Fehler" : cmbDruckAnmeldung.SelectedItem.ToString(); break;
          case JgMaschineData.EnumFilterAuswertung.Bauteil: repName = cmbDruckBauteil.SelectedItem == null ? "Fehler" : cmbDruckBauteil.SelectedItem.ToString(); break;
          case JgMaschineData.EnumFilterAuswertung.Reparatur: repName = cmbDruckReparatur.SelectedItem == null ? "Fehler" : cmbDruckReparatur.SelectedItem.ToString(); break;
        }

        if (repName == "FEHLER")
        {
          MessageBox.Show("Es wurde kein Report ausgewählt.", "Fehler !", MessageBoxButton.OK, MessageBoxImage.Information);
          return;
        }

        _AktAuswertung = _Db.tabAuswertungSet.FirstOrDefault(f => (f.ReportName == repName) && (f.FilterAuswertung == auswahl));

        if (_AktAuswertung.Report == null)
          vorgang = 4;
        else
        {
          MemoryStream mem;
          mem = new MemoryStream(_AktAuswertung.Report);
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
          JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabAuswertung>(_Db, _AktAuswertung, JgMaschineData.EnumStatusDatenabgleich.Neu);

          switch (auswahl)
          {
            case JgMaschineData.EnumFilterAuswertung.Arbeitszeit:
              cmbDruckArbeitszeit.Items.Add(form.ReportName);
              cmbDruckArbeitszeit.SelectedIndex = cmbDruckArbeitszeit.Items.IndexOf(form.ReportName);
              break;
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
  }
}
