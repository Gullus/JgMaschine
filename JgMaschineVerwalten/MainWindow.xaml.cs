using JgMaschineLib.Zeit;
using JgMaschineVerwalten.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Data.Entity;

namespace JgMaschineVerwalten
{
  public partial class MainWindow : Window
  {
    JgMaschineData.JgModelContainer _Db;
    private CollectionViewSource _VsBediener { get { return (CollectionViewSource)this.FindResource("vsBedienerAktuell"); } }
    private CollectionViewSource _VsReparatur { get { return (CollectionViewSource)this.FindResource("vsReparaturAktuell"); } }
    //private CollectionViewSource _VsAnmeldung { get { return (CollectionViewSource)this.FindResource("vsAnmeldung"); } }
    //private CollectionViewSource _VsUmmeldung { get { return (CollectionViewSource)this.FindResource("vsUmmeldung"); } }


    
    private JgMaschineData.tabStandort _Standort;
    private JgMaschineLib.JgListe<JgMaschineData.tabMaschine> _ListeMaschinen;

    private JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit> _ListeArbeitszeitAktuell;
    private JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit> _ListeArbeitszeitAuswahl;
    private JgDatumZeit _DzArbeitszeitVon { get { return (JgDatumZeit)this.FindResource("dzArbeitszeitVon"); } }
    private JgDatumZeit _DzArbeitszeitBis { get { return (JgDatumZeit)this.FindResource("dzArbeitszeitBis"); } }

    private JgMaschineData.tabMaschine _Maschine { get { return _ListeMaschinen.AktDatensatz;  } }

    private JgMaschineLib.JgListe<JgMaschineData.tabAnmeldungMaschine> _ListeAnmeldungAktuell;
    private JgMaschineLib.JgListe<JgMaschineData.tabAnmeldungMaschine> _ListeAnmeldungAuswahl;
    private JgDatumZeit _DzAnmeldungVon { get { return (JgDatumZeit)this.FindResource("dzAnmeldungVon"); } }
    private JgDatumZeit _DzAnmeldungBis { get { return (JgDatumZeit)this.FindResource("dzAnmeldungBis"); } }


    private JgMaschineLib.JgListe<JgMaschineData.tabBauteil> _ListeBauteilAktuell;
    private JgMaschineLib.JgListe<JgMaschineData.tabBauteil> _ListeBauteilAuswahl;

    private JgMaschineLib.JgListe<JgMaschineData.tabReparatur> _ListeReparaturAktuell;
    private JgMaschineLib.JgListe<JgMaschineData.tabReparatur> _ListeReparaturAuswahl;

    private System.Threading.Timer _AktualisierungsTimer;

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

      // Arbeitszeit Initialisieren

      cmbBenutzerArbeitszeit.ItemsSource = await _Db.tabBedienerSet.Where(w => w.Status != JgMaschineData.EnumStatusBediener.Stillgelegt).OrderBy(o => o.NachName).ToListAsync();

      IQueryable<JgMaschineData.tabArbeitszeit> iqArbeitszeitAktuell = _Db.tabArbeitszeitSet.Where(w => (w.fStandort == _Standort.Id) && w.IstAktiv).OrderBy(o => o.Anmeldung);
      vs = (CollectionViewSource)FindResource("vsArbeitszeitAktuell");
      _ListeArbeitszeitAktuell = new JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit>(_Db, iqArbeitszeitAktuell, vs, dgArbeitszeitAktuell);
      await _ListeArbeitszeitAktuell.Init();

      _DzArbeitszeitVon.DatumZeit = von;
      _DzArbeitszeitBis.DatumZeit = bis;

      IQueryable<JgMaschineData.tabArbeitszeit> iqArbeitszeitAuswahl = _Db.tabArbeitszeitSet.Where(w => (w.fStandort == _Standort.Id) && (!w.IstAktiv) && (w.Anmeldung >= _DzArbeitszeitVon.DatumZeit) && ((w.Anmeldung <= _DzArbeitszeitBis.DatumZeit))).OrderBy(o => o.Anmeldung);
      vs = (CollectionViewSource)FindResource("vsArbeitszeitAuswahl");
      _ListeArbeitszeitAuswahl = new JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit>(_Db, iqArbeitszeitAuswahl, vs, dgArbeitszeitAuswahl);
      await _ListeArbeitszeitAuswahl.Init();

      // Anmeldung initialisieren

      var vsBenutzerAnmeldung = (CollectionViewSource)FindResource("vsAnmeldungBenutzer");
      vsBenutzerAnmeldung.Source = _ListeArbeitszeitAktuell;

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



      IniCommands();

      _AktualisierungsTimer = new System.Threading.Timer((obj) =>
      {
        Dispatcher.Invoke((Action)delegate() { TabellenAktualisieren(); });
      }, null, 60000, 60000);
    }

    private void InitTabelleAnmeldedaten()
    {
      //_VsAnmeldung.Source = _Db.tabArbeitszeitSet.Where(w => (w.fStandort == Properties.Settings.Default.IdStandort)).ToList();
      //_VsAnmeldung.View.CurrentChanged += View_CurrentChangedAnmeldung;
      View_CurrentChangedAnmeldung(null, null);
    }

    private void IniCommands()
    {
      #region Formular Reparatur aufrufen

      CommandBindings.Add(new CommandBinding(MyCommands.ReparaturNeu, (sen, erg) =>
      {
        JgMaschineVerwalten.Fenster.FormReparatur form = new Fenster.FormReparatur(_Db, null);
        if (form.ShowDialog() ?? false)
        {
          form.Reparatur.eMaschine = (JgMaschineData.tabMaschine)_ListeMaschinen.AktDatensatz;
          _Db.tabReparaturSet.Add(form.Reparatur);
          _Db.SaveChanges();
          (_VsReparatur.View.SourceCollection as List<JgMaschineData.tabReparatur>).Insert(0, form.Reparatur);
          _VsReparatur.View.Refresh();
          _VsReparatur.View.MoveCurrentToFirst();
        };
      }, (sen, erg) =>
      {
        // erg.CanExecute = _VsMaschine.View.CurrentItem != null;
      }));

      #endregion

      #region Formular Reperaturarbeiten aufrufen

      CommandBindings.Add(new CommandBinding(MyCommands.ReparaturBearbeiten, (sen, erg) =>
      {
        JgMaschineVerwalten.Fenster.FormReparatur form = new Fenster.FormReparatur(_Db, (JgMaschineData.tabReparatur)_VsReparatur.View.CurrentItem);
        if (form.ShowDialog() ?? false)
          _VsReparatur.View.Refresh();
        else
          _Db.Entry(form.Reparatur).Reload();
        _Db.SaveChanges();
      }, (sen, erg) =>
      {
        erg.CanExecute = (_VsReparatur.View != null) && !_VsReparatur.View.IsEmpty;
      }));

      #endregion

      #region Manuelle Anmeldung Bediener

      CommandBindings.Add(new CommandBinding(MyCommands.ManuelleAnmeldungBediener, (sen, erg) =>
      {
        var bediener = (JgMaschineData.tabBediener)cmbBenutzerArbeitszeit.SelectedItem;
        var aktAnmeldung = _ListeArbeitszeitAktuell.FirstOrDefault(f => f.eBediener == bediener);
        if (aktAnmeldung != null)
        {
          _ListeArbeitszeitAktuell.MoveTo(bediener);
          MessageBox.Show(string.Format("Bediener {0} ist bereits angemeldet !", bediener.Name), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
          JgFormDatumZeit form = new JgFormDatumZeit();
          if (form.Anzeigen("Anmeldung", "Geben Sie die Zeit an, wo sich der Benutzer angemeldet hat.", DateTime.Now))
          {
            var arbeitszeit = new JgMaschineData.tabArbeitszeit()
            {
              Id = Guid.NewGuid(),
              eBediener = bediener,
              eStandort = _Standort,
              Anmeldung = form.Datum,
              ManuelleAnmeldung = true,
              Abmeldung = form.Datum,
              ManuelleAbmeldung = true,
              IstAktiv = true
            };
            _ListeArbeitszeitAktuell.Add(arbeitszeit);
          }
        }
      }));

      #endregion

      #region Manuelle Abmeldung Bediener

      CommandBindings.Add(new CommandBinding(MyCommands.ManuelleAbmeldungBediener, (sen, erg) =>
      {
        ////var anmeldung = (JgMaschineData.tabArbeitszeit)_VsAnmeldung.View.CurrentItem;
        //string anzeigeText = string.Format("Möchten Sie den Bediener {0} abmelden ?", anmeldung.eBediener.Name);

        //JgFormDatumZeit form = new JgFormDatumZeit();
        //if (form.Anzeigen("An- bzw. Abmeldung", anzeigeText, DateTime.Now))
        //{
        //  //var letzteUmmbuchung = (anmeldung.Ummeldung == null) ? anmeldung.Anmeldung : anmeldung.Ummeldung;
        //  //if (anmeldung.sUmmeldungen.Count > 0)
        //  //  letzteUmmbuchung = anmeldung.sUmmeldungen.Max(m => m.Ummeldung);

        //  //if (letzteUmmbuchung > form.Datum)
        //  //  MessageBox.Show("Abmeldung muss nach der Anmeldung bzw. letzten Ummeldung liegen !", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        //  //else
        //  //{
        //  //  anmeldung.Vorgang = JgMaschineData.EnumVorgangAnmeldung.Abgemeldet;
        //  //  anmeldung.Abmeldung = form.Datum;
        //  //  anmeldung.ManuelleAbmeldung = true;
        //  //  (_VsAnmeldung.View.SourceCollection as List<JgMaschineData.tabAnmeldung>).Remove(anmeldung);

        //  //  // Letzte Ummeldung Zeit Abmeldung eintragen

        //  //  if (anmeldung.Ummeldung == null)
        //  //    anmeldung.Ummeldung = form.Datum;
        //  //  else
        //  //  {
        //  //    var ummeldungen = anmeldung.sUmmeldungen.Where(w => (w.Ummeldung == null)).ToList();
        //  //    foreach (var ummeldung in ummeldungen)
        //  //      ummeldung.Ummeldung = form.Datum;
        //  //  }

        //  //  _Db.SaveChanges();
        //  //  TabellenAktualisieren();
        //  //}
        //}
      }));

      #endregion

      #region Bediener Ummelden

      CommandBindings.Add(new CommandBinding(MyCommands.BedienerUmmelden, (sen, erg) =>
      {
        ////var anmeldung = (JgMaschineData.tabArbeitszeit)_VsAnmeldung.View.CurrentItem;
        ////var neueMaschine = (JgMaschineData.tabMaschine)_VsMaschine.View.CurrentItem;

        ////string anzeigeText = "hallo"; // string.Format("Bearbeiter {0} von Maschine {1} auf Maschine {2} ummelden ?", anmeldung.eBediener.Name, anmeldung.eMaschine.MaschinenName, neueMaschine.MaschinenName);

        ////JgFormDatumZeit datumZeit = new JgFormDatumZeit();
        //if (datumZeit.Anzeigen("Ummeldung !", anzeigeText, DateTime.Now))
        //{
        //  var aktDatum = datumZeit.Datum;

        //  if (aktDatum < anmeldung.Anmeldung)
        //  {
        //    MessageBox.Show("Datum muss nach dem Anmeldedatum liegen !", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        //    return;
        //  }

        //  //var anmeldung = new JgMaschineData.tabAnmeldungMaschine()
        //  //{
        //  //  Id = Guid.NewGuid(),
        //  //  eMaschine = neueMaschine,
        //  //  eUmmeldungZuAnmeldung = anmeldung,
        //  //  Anmeldung = DateTime.Now,
        //  //  eBediener = anmeldung.eBediener,
        //  //};

        //  //var ummeldungen = (List<JgMaschineData.tabAnmeldung>)anmeldung.sUmmeldungen.ToList();
        //  //ummeldungen.Add(anmeldung);
        //  //ummeldungen = ummeldungen.OrderBy(o => o.Anmeldung).ToList();

        //  //var vorgaenger = ummeldungen.Where(w => (w.Anmeldung < aktDatum)).OrderByDescending(o => o.Anmeldung).FirstOrDefault();

        //  //anmeldung.Anmeldung = aktDatum;
        //  //if (vorgaenger.Ummeldung != null)
        //  //  anmeldung.Ummeldung = vorgaenger.Ummeldung;

        //  //vorgaenger.Ummeldung = aktDatum.AddMilliseconds(-1);

        //  //_Db.tabAnmeldungSet.Add(anmeldung);
        //  //_Db.SaveChanges();

        //  //(_VsUmmeldung.View.SourceCollection as List<JgMaschineData.tabAnmeldung>).Add(anmeldung);
        //  //_VsUmmeldung.View.Refresh();
        //  //_VsMaschine.View.Refresh();
        //  //_VsAnmeldung.View.Refresh();
        //}
      }));

      #endregion
    }

    private void TabellenAktualisieren()
    {
      //_AktualisierungsTimer.Change(System.Threading.Timeout.Infinite, 0);

      //tblDatum.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
      //_VsMaschine.View.Refresh();
      //_VsAnmeldung.View.Refresh();
      //_VsUmmeldung.View.Refresh();

      _AktualisierungsTimer.Change(60000, 60000);
    }

    private void View_CurrentChangedAnmeldung(object sender, EventArgs e)
    {
      //if (_VsAnmeldung.View.CurrentItem != null)
      //  _VsUmmeldung.Source = (_VsAnmeldung.View.CurrentItem as JgMaschineData.tabAnmeldung).sUmmeldungen.OrderBy(o => o.Anmeldung).ToList();
      //else
      //  _VsUmmeldung.Source = new List<JgMaschineData.tabAnmeldung>();
    }


    void View_CurrentChangedMaschine(object sender, EventArgs e)
    {
      //if (_VsMaschine.View.CurrentItem != null)
      //{
      //  var maschine = (JgMaschineData.tabMaschine)_VsMaschine.View.CurrentItem;
      //  var anzeigeAb = DateTime.Today.AddDays(-30);
      //  _VsReparatur.Source = _Db.tabReparaturSet.Where(w => (w.fMaschine == maschine.Id) && (w.VorgangBeginn >= anzeigeAb)).OrderBy(o => o.Id).Take(30).ToList();
      //}
    }

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

    private void btnAuswahlAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      if (sender == btnArbeitszeitAktuallisieren)
        _ListeArbeitszeitAuswahl.DatenGenerieren();
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
          var bediener =  arbeitszeit.eBediener;
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

    private void ArbeitszeitBearbeiten(object sender, RoutedEventArgs e)
    {
      var arbeitszeit = _ListeArbeitszeitAuswahl.AktDatensatz;
      var anz = string.Format("Korrektur der Arbeitszeit für den Mitarbeiter {0}.", arbeitszeit.eBediener.Name);
      var form = new JgMaschineLib.Zeit.FormAuswahlDatumVonBis("Berichtigung Arbeitszeit", anz, arbeitszeit.Anmeldung, arbeitszeit.Abmeldung);
      if(form.ShowDialog() ?? false)
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
  }
}
