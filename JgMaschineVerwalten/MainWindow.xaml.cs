using JgMaschineData;
using JgMaschineLib;
using JgMaschineLib.Zeit;
using JgMaschineVerwalten.Commands;
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

namespace JgMaschineVerwalten
{
  public partial class MainWindow : Window
  {
    private tabStandort _Standort;
    private JgList<tabBediener> _ListeBediener;
    private CollectionViewSource _VsBedienerArbeitszeit { get { return (CollectionViewSource)this.FindResource("vsBedienerArbeitszeit"); } }
    private CollectionViewSource _VsBedienerAnmeldung { get { return (CollectionViewSource)this.FindResource("vsBedienerAnmeldung"); } }

    private JgList<tabMaschine> _ListeMaschinen;
    private JgList<tabAuswertung> _ListeAuswertung;

    private CollectionViewSource _VsAuswertungArbeitszeit { get { return (CollectionViewSource)this.FindResource("vsAuswertungArbeitszeit"); } }
    private CollectionViewSource _VsAuswertungAnmeldung { get { return (CollectionViewSource)this.FindResource("vsAuswertungAnmeldung"); } }
    private CollectionViewSource _VsAuswertungBauteil { get { return (CollectionViewSource)this.FindResource("vsAuswertungBauteil"); } }
    private CollectionViewSource _VsAuswertungReparatur { get { return (CollectionViewSource)this.FindResource("vsAuswertungReparatur"); } }
           
    private tabMaschine _Maschine { get { return _ListeMaschinen.AktDatensatz; } }
    private Guid[] _IdisMaschine = null;

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

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      tblDatum.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
      var bis = DateTime.Now.Date;
      var von = bis.AddDays(-10);
      bis = new DateTime(bis.Year, bis.Month, bis.Day, 23, 59, 59);

      // Standort initialieren

      using (var db = new JgMaschineData.JgModelContainer())
      {
        _Standort = await db.tabStandortSet.FindAsync(Properties.Settings.Default.IdStandort);
        if (_Standort == null)
          _Standort = await db.tabStandortSet.FirstOrDefaultAsync();
      }
      tblStandort.Text = _Standort.Bezeichnung;

      // Bediener initialisieren

      _ListeBediener = new JgList<tabBediener>(_VsBedienerArbeitszeit); 
      _ListeBediener.MyQuery = _ListeBediener.Db.tabBedienerSet.Where(w => w.Status != JgMaschineData.EnumStatusBediener.Stillgelegt)
        .Include(i => i.eAktuelleAnmeldungMaschine).OrderBy(o => o.NachName);
      await _ListeBediener.DatenGenerierenAsync();
      _VsBedienerAnmeldung.Source = _ListeBediener;

      // Maschine intialisieren

      _ListeMaschinen = new JgList<tabMaschine>((CollectionViewSource)FindResource("vsMaschine"));
      _ListeMaschinen.MyQuery = _ListeMaschinen.Db.tabMaschineSet.Where(w => (w.fStandort == _Standort.Id) && (w.Status != JgMaschineData.EnumStatusMaschine.Stillgelegt)).Include(i => i.sAktuelleBediener).OrderBy(o => o.MaschinenName);
      await _ListeMaschinen.DatenGenerierenAsync();
      _IdisMaschine = _ListeMaschinen.Select(s => s.Id).ToArray();

      // Arbeitszeit Initialisieren

      _ListeArbeitszeitAktuell = new JgList<tabArbeitszeit>((CollectionViewSource)FindResource("vsArbeitszeitAktuell"));
      _ListeArbeitszeitAktuell.MyQuery = _ListeArbeitszeitAktuell.Db.tabArbeitszeitSet.Where(w => (w.fStandort == _Standort.Id) && w.IstAktiv)
        .Include(i => i.eBediener).Include(i => i.eBediener.eAktuelleAnmeldungMaschine).OrderBy(o => o.Anmeldung);
      _ListeArbeitszeitAktuell.ListeTabellen = new DataGrid[] { dgArbeitszeitAktuell };
      await _ListeArbeitszeitAktuell.DatenGenerierenAsync();

      _DzArbeitszeitVon.DatumZeit = von;
      _DzArbeitszeitBis.DatumZeit = bis;

      _ListeArbeitszeitAuswahl = new JgList<tabArbeitszeit>((CollectionViewSource)FindResource("vsArbeitszeitAuswahl"));
      _ListeArbeitszeitAuswahl.MyQuery = _ListeArbeitszeitAuswahl.Db.tabArbeitszeitSet.Where(w => (w.fStandort == _Standort.Id) && (!w.IstAktiv) && (w.Anmeldung >= _DzArbeitszeitVon.DatumZeit) && ((w.Anmeldung <= _DzArbeitszeitBis.DatumZeit))).OrderBy(o => o.Anmeldung);
      _ListeArbeitszeitAuswahl.ListeTabellen = new DataGrid[] { dgArbeitszeitAuswahl };

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

      // Anmeldungen initialisieren

      _ListeAnmeldungAktuell = new JgList<tabAnmeldungMaschine>((CollectionViewSource)FindResource("vsAnmeldungAktuell"));
      _ListeAnmeldungAktuell.MyQuery = _ListeAnmeldungAktuell.Db.tabAnmeldungMaschineSet.Where(w => (_IdisMaschine.Contains(w.fMaschine)) && w.IstAktiv).Include(i => i.eBediener).Include(i => i.eMaschine).OrderBy(o => o.Anmeldung);
      _ListeAnmeldungAktuell.ListeTabellen = new DataGrid[] { dgAnmeldungAktuell };
      await _ListeAnmeldungAktuell.DatenGenerierenAsync();

      _DzAnmeldungVon.DatumZeit = von;
      _DzAnmeldungBis.DatumZeit = bis;

      _ListeAnmeldungAuswahl = new JgList<tabAnmeldungMaschine>((CollectionViewSource)FindResource("vsAnmeldungAuswahl"));
      _ListeAnmeldungAuswahl.MyQuery = _ListeAnmeldungAuswahl.Db.tabAnmeldungMaschineSet.Where(w => (_IdisMaschine.Contains(w.fMaschine)) && (!w.IstAktiv) && (w.Anmeldung >= _DzAnmeldungVon.DatumZeit) && ((w.Anmeldung <= _DzAnmeldungBis.DatumZeit))).OrderBy(o => o.Anmeldung);
      _ListeAnmeldungAuswahl.ListeTabellen = new DataGrid[] { dgAnmeldungAuswahl };

      // Bauteile initialisieren

      _DzBauteilVon.DatumZeit = von;
      _DzBauteilBis.DatumZeit = bis;

      _ListeBauteilAuswahl = new JgList<tabBauteil>((CollectionViewSource)FindResource("vsBauteilAuswahl"));
      _ListeBauteilAuswahl.MyQuery = _ListeBauteilAuswahl.Db.tabBauteilSet.Where(w => (w.fMaschine == _ListeMaschinen.AktDatensatz.Id) && (w.DatumStart >= _DzBauteilVon.DatumZeit) && (w.DatumStart <= _DzBauteilBis.DatumZeit)).OrderBy(o => o.DatumStart).Include(i => i.sBediener);
      _ListeBauteilAuswahl.ListeTabellen = new DataGrid[] { dgBauteilAuswahl };

      // Reparaturen initialisieren 

      _ListeReparaturAktuell = new JgList<tabReparatur>((CollectionViewSource)FindResource("vsReparaturAktuell"));
      _ListeReparaturAktuell.MyQuery = _ListeReparaturAktuell.Db.tabReparaturSet.Where(w => _IdisMaschine.Contains(w.fMaschine) && w.IstAktiv).OrderBy(o => o.VorgangBeginn);
      _ListeReparaturAktuell.ListeTabellen = new DataGrid[] { dgReparaturAktuell };
      await _ListeReparaturAktuell.DatenGenerierenAsync();

      _DzReparaturVon.DatumZeit = von;
      _DzReparaturBis.DatumZeit = bis;

      _ListeReparaturAuswahl = new JgList<tabReparatur>((CollectionViewSource)FindResource("vsReparaturAuswahl"));
      _ListeReparaturAuswahl.MyQuery = _ListeReparaturAuswahl.Db.tabReparaturSet.Where(w => _IdisMaschine.Contains(w.fMaschine) && (!w.IstAktiv) && (w.VorgangBeginn >= _DzReparaturVon.DatumZeit) && (w.VorgangBeginn <= _DzReparaturBis.DatumZeit)).OrderBy(o => o.VorgangBeginn);
      _ListeReparaturAuswahl.ListeTabellen = new DataGrid[] { dgReparaturAuswahl };

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

      _AktualisierungsTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = new TimeSpan(0, 0, 10) };
      _AktualisierungsTimer.Tick += _AktualisierungsTimer_Tick;
      _AktualisierungsTimer.Start();
    }

    private async void _AktualisierungsTimer_Tick(object sender, EventArgs e)
    {
      await _ListeMaschinen.DatenGenerierenAsync();
      await _ListeArbeitszeitAktuell.DatenGenerierenAsync();
      await _ListeAnmeldungAktuell.DatenGenerierenAsync();
      await _ListeReparaturAktuell.DatenGenerierenAsync();
    }

    #region Formular Reparatur

    private void ExecuteRepataurNeu(object sender, ExecutedRoutedEventArgs e)
    {
      JgMaschineVerwalten.Fenster.FormReparatur form = new Fenster.FormReparatur(null, _ListeMaschinen, _ListeBediener, _Maschine);
      if (form.ShowDialog() ?? false)
        _ListeReparaturAktuell.Add(form.Reparatur);
    }

    private void CanExecuteRepataurNeu(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = _ListeMaschinen.AktDatensatz != null;
    }

    private void ReparaturBearbeitenAktuell_Click(object sender, RoutedEventArgs e)
    {
      Fenster.FormReparatur form = new Fenster.FormReparatur(_ListeReparaturAktuell.AktDatensatz, _ListeMaschinen, _ListeBediener);
      if (form.ShowDialog() ?? false)
        _ListeReparaturAktuell.AktSichern(EnumStatusDatenabgleich.Geaendert);
      else
        _ListeReparaturAktuell.Reload(form.Reparatur);
    }

    private void ReparaturBearbeitenAuswahl_Click(object sender, RoutedEventArgs e)
    {
      Fenster.FormReparatur form = new Fenster.FormReparatur(_ListeReparaturAuswahl.AktDatensatz, _ListeMaschinen, _ListeBediener);
      if (form.ShowDialog() ?? false)
        _ListeReparaturAuswahl.AktSichern(EnumStatusDatenabgleich.Geaendert);
      else
        _ListeReparaturAuswahl.Reload(form.Reparatur);
    }

    private async void ReparaturBeenden_Click(object sender, RoutedEventArgs e)
    {
      var rep = _ListeReparaturAktuell.AktDatensatz;
      var anzeigeText = $"Maschine {rep.eMaschine.MaschinenName} mit Ereigniss {rep.Ereigniss} abmelden?";
      JgMaschineLib.Zeit.FormAuswahlDatumZeit form = new FormAuswahlDatumZeit("Abmeldung", anzeigeText, rep.VorgangEnde);
      if (form.ShowDialog() ?? false)
      {
        rep.VorgangEnde = form.DatumZeit;
        rep.IstAktiv = false;
        _ListeReparaturAuswahl.AktSichern(EnumStatusDatenabgleich.Geaendert);
        await _ListeReparaturAktuell.DatenGenerierenAsync();
      }
    }

    #endregion

    #region Anmeldungen Maschine

    private async void ExecuteAnmeldungBenutzerAnmeldung(object sender, ExecutedRoutedEventArgs e)
    {
      var bediener = (tabBediener)_VsBedienerAnmeldung.View.CurrentItem;
      var maschine = _ListeMaschinen.AktDatensatz;

      _ListeBediener.Reload(bediener);
      if (bediener.eAktuelleAnmeldungMaschine != null)
      {
        MessageBox.Show($"{bediener.Name} ist bereits an Maschine: '{maschine.MaschinenName}' angemeldet! Bitte erst an dieser Maschine abmelden.", "Information !", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      JgFormDatumZeit form = new JgFormDatumZeit();
      if (form.Anzeigen("Anmeldung", $"Geben Sie die Zeit an, in welcher sich der '{bediener.Name}' angemeldet hat.", DateTime.Now))
      {
        bediener.eAktuelleAnmeldungMaschine = await _ListeBediener.DsAttach<tabMaschine>(maschine.Id);
        _ListeBediener.DsSichern(bediener, EnumStatusDatenabgleich.Geaendert);

        var anmeldung = new JgMaschineData.tabAnmeldungMaschine()
        {
          Id = Guid.NewGuid(),
          eBediener = await _ListeAnmeldungAktuell.DsAttach<tabBediener>(bediener.Id),
          eMaschine = await _ListeAnmeldungAktuell.DsAttach<tabMaschine>(maschine.Id),
          Anmeldung = form.Datum,
          ManuelleAnmeldung = true,
          Abmeldung = form.Datum,
          ManuelleAbmeldung = true,
          IstAktiv = true
        };
        _ListeAnmeldungAktuell.Add(anmeldung);
        await _ListeMaschinen.DatenGenerierenAsync();
      }
    }

    private async void AnmeldungBenutzerAbmelden_Click(object sender, RoutedEventArgs e)
    {
      var anmeldung = _ListeAnmeldungAktuell.AktDatensatz;
      string anzeigeText = $"Möchten Sie den Bediener {anmeldung.eBediener.Name} von der Maschine {anmeldung.eMaschine.MaschinenName} abmelden ?";

      JgFormDatumZeit form = new JgFormDatumZeit();
      if (form.Anzeigen("Abmeldung", anzeigeText, DateTime.Now))
      {
        var bediener = _ListeBediener.FirstOrDefault(f => f.Id == anmeldung.fBediener);
        bediener.eAktuelleAnmeldungMaschine = null;
        _ListeBediener.DsSichern(bediener, EnumStatusDatenabgleich.Geaendert);

        anmeldung.Abmeldung = form.Datum;
        anmeldung.ManuelleAbmeldung = true;
        anmeldung.IstAktiv = false;
        _ListeAnmeldungAktuell.AktSichern(EnumStatusDatenabgleich.Geaendert);

        _ListeAnmeldungAktuell.Remove(anmeldung);
        await _ListeMaschinen.DatenGenerierenAsync();
      }
    }

    private void CanExecuteBedienerAngemeldetMaschineVorhanden(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = (_ListeMaschinen.AktDatensatz != null) && (_ListeArbeitszeitAktuell.AktDatensatz != null);
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
        tblStandort.Text = _Standort.Bezeichnung;
        await _ListeMaschinen.DatenGenerierenAsync();
        _IdisMaschine = _ListeMaschinen.Select(s => s.Id).ToArray();
        await _ListeArbeitszeitAktuell.DatenGenerierenAsync();
        await _ListeArbeitszeitAuswahl.DatenGenerierenAsync();
        await _ListeAnmeldungAktuell.DatenGenerierenAsync();
        await _ListeAnmeldungAuswahl.DatenGenerierenAsync();
        await _ListeBauteilAuswahl.DatenGenerierenAsync();
        await _ListeReparaturAktuell.DatenGenerierenAsync();
        await _ListeReparaturAuswahl.DatenGenerierenAsync();
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      Properties.Settings.Default.Save();
    }

    #region Arbeitszeit verwalten

    private async void ArbeitszeitAnmeldung_Click(object sender, RoutedEventArgs e)
    {
      var bediener = (tabBediener)(FindResource("vsBedienerArbeitszeit") as CollectionViewSource).View.CurrentItem;

      if (_ListeArbeitszeitAktuell.FirstOrDefault(f => f.fBediener == bediener.Id) != null)
        MessageBox.Show($"Bediener {bediener.Name} bereits angemeldet !", "Informaation !", MessageBoxButton.OK, MessageBoxImage.Warning);
      else
      {
        string anzeigeText = string.Format("Mitarbeiter {0} im Betrieb anmelden !", bediener.Name);
        FormAuswahlDatumZeit form = new FormAuswahlDatumZeit("Anmeldung", anzeigeText, DateTime.Now);
        if (form.ShowDialog() ?? false)
        {
          var arbeitszeit = new JgMaschineData.tabArbeitszeit()
          {
            Id = Guid.NewGuid(),
            eBediener = await _ListeArbeitszeitAktuell.DsAttach<tabBediener>(bediener.Id),
            eStandort = await _ListeArbeitszeitAktuell.DsAttach<tabStandort>(_Standort.Id),
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
      var bediener = arbeitszeit.eBediener;

      JgFormDatumZeit form = new JgFormDatumZeit();
      if (form.Anzeigen("Abmeldung Bediener", $"Möchten Sie '{bediener.Name}' abmelden ?", DateTime.Now))
      {
        arbeitszeit.IstAktiv = false;
        arbeitszeit.Abmeldung = form.Datum;
        arbeitszeit.ManuelleAbmeldung = true;
        _ListeArbeitszeitAktuell.AktSichern(EnumStatusDatenabgleich.Geaendert);
        _ListeArbeitszeitAktuell.Remove(arbeitszeit);

        bediener = _ListeBediener.FirstOrDefault(f => f.Id == bediener.Id);

        if (bediener?.eAktuelleAnmeldungMaschine != null)
        {
          var anzeigeText = $"{bediener.Name} ist noch an Maschine '{bediener.eAktuelleAnmeldungMaschine.MaschinenName}' angemeldet. Von dieser Maschine abmelden?";
          var erg = MessageBox.Show(anzeigeText, "Abfrage Abmeldung", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
          if (erg == MessageBoxResult.Yes)
          {
            bediener.eAktuelleAnmeldungMaschine = null;
            _ListeBediener.DsSichern(bediener, EnumStatusDatenabgleich.Geaendert);

            var anmeldung = await _ListeAnmeldungAktuell.Db.tabAnmeldungMaschineSet.FirstOrDefaultAsync(w => (w.fMaschine == arbeitszeit.eBediener.fAktuellAngemeldet) && (w.fBediener == arbeitszeit.fBediener));
            if (anmeldung != null)
            {
              anmeldung.Abmeldung = form.Datum;
              anmeldung.IstAktiv = false;
              anmeldung.ManuelleAbmeldung = true;
              _ListeAnmeldungAktuell.DsSichern(anmeldung, EnumStatusDatenabgleich.Geaendert);
            }

            if (_ListeAnmeldungAktuell.FirstOrDefault(w => (w.Id == anmeldung.Id)) != null)
              _ListeAnmeldungAktuell.Remove(anmeldung);

            await _ListeMaschinen.DatenGenerierenAsync();
          }
        }
      }
    }

    private void ArbeitszeitBearbeiten_Click(object sender, RoutedEventArgs e)
    {
      var arbeitszeit = _ListeArbeitszeitAuswahl.AktDatensatz;
      var anz = $"Korrektur der Arbeitszeit für den Mitarbeiter {arbeitszeit.eBediener.Name}.";
      var form = new FormAuswahlDatumVonBis("Berichtigung Arbeitszeit", anz, arbeitszeit.Anmeldung, arbeitszeit.Abmeldung);
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
        case "1": await _ListeArbeitszeitAktuell.DatenGenerierenAsync(); break;
        case "2": await _ListeAnmeldungAktuell.DatenGenerierenAsync(); break;
        case "4": await _ListeReparaturAktuell.DatenGenerierenAsync(); break;
      }
    }
  }
}
