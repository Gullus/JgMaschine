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
    private CollectionViewSource _VsMaschine { get { return (CollectionViewSource)this.FindResource("vsMaschine"); } }
    private CollectionViewSource _VsBediener { get { return (CollectionViewSource)this.FindResource("vsBediener"); } }
    private CollectionViewSource _VsReparatur { get { return (CollectionViewSource)this.FindResource("vsReparatur"); } }
    //private CollectionViewSource _VsAnmeldung { get { return (CollectionViewSource)this.FindResource("vsAnmeldung"); } }
    //private CollectionViewSource _VsUmmeldung { get { return (CollectionViewSource)this.FindResource("vsUmmeldung"); } }


    
    private JgMaschineData.tabStandort _Standort;
    private JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit> _ListeMaschinen;

    private JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit> _ListeArbeitszeitAktuell;
    private JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit> _ListeArbeitszeitAuswahl;




    //private JgMaschineLib.JgListe<JgMaschineData.tabBediener> _ListeBediener;

    private System.Threading.Timer _AktualisierungsTimer;

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _Db = new JgMaschineData.JgModelContainer();

      _Standort = await _Db.tabStandortSet.FindAsync(Properties.Settings.Default.IdStandort);
      if (_Standort == null)
        _Standort = await _Db.tabStandortSet.FirstOrDefaultAsync();

      var maschinen = await _Db.tabMaschineSet.Where(w => (w.fStandort == _Standort.Id) && (w.Status != JgMaschineData.EnumStatusMaschine.Stillgelegt)).OrderBy(o => o.MaschinenName).ToListAsync();
      var vs = (CollectionViewSource)this.FindResource("vsArbeitszeitAktuell");
      //vs.View.CurrentChanging += (sen, erg) =>
      //{
      //  // if ((sen as CollectionViewSource).)
      
      
      //};

      //_ListeMaschinen = new JgMaschineLib.JgListe<JgMaschineData.tabMaschine>(maschinen, vs );
      
      
      
      cmbBenutzerAnmeldungArbeitszeit.ItemsSource = await _Db.tabBedienerSet.Where(w => w.Status != JgMaschineData.EnumStatusBediener.Stillgelegt).OrderBy(o => o.NachName).ToListAsync();
      vs = (CollectionViewSource)this.FindResource("vsMaschine");






      var arbeitszeiten = await _Db.tabArbeitszeitSet.Where(w => (w.fStandort == _Standort.Id) && w.IstAktiv).OrderBy(o => o.Anmeldung).ToListAsync();
      vs = (CollectionViewSource)this.FindResource("vsArbeitszeitAktuell");
      _ListeArbeitszeitAktuell = new JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit>(arbeitszeiten, vs, dgArbeitszeitAktuell);

      arbeitszeiten = await _Db.tabArbeitszeitSet.Where(w => w.IstAktiv).OrderBy(o => o.Anmeldung).ToListAsync();
      vs = (CollectionViewSource)this.FindResource("vsArbeitszeitAuswahl");
      _ListeArbeitszeitAuswahl = new JgMaschineLib.JgListe<JgMaschineData.tabArbeitszeit>(arbeitszeiten, vs, dgArbeitszeitAktuell);
      





      
      //var bediener = await _Db.tabBedienerSet.Where(w => w.Status != JgMaschineData.EnumStatusBediener.Stillgelegt).OrderBy(o => o.NachName).ToListAsync();
      //var vs = (CollectionViewSource)this.FindResource("vsBediener");
      //_ListeBediener = new JgMaschineLib.JgListe<JgMaschineData.tabBediener>(bediener, vs, null);

      //MaschinenWechseln();

      //InitTabelleAnmeldedaten();

      IniCommands();

      tblDatum.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
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
          form.Reparatur.eMaschine = (JgMaschineData.tabMaschine)_VsMaschine.View.CurrentItem;
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
        var bediener = (JgMaschineData.tabBediener)cmbBenutzerAnmeldungArbeitszeit.SelectedItem;
        var aktAnmeldung = _ListeArbeitszeitAktuell.FirstOrDefault(f => f.eBediener == bediener);
        if (aktAnmeldung != null)
        {
          _ListeArbeitszeitAktuell.MoveTo(bediener);
          MessageBox.Show(string.Format("Bediener {0} ist bereits angemeldet !", bediener.Name), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
          JgFormDatumZeit form = new JgFormDatumZeit();
          if (form.Anzeigen("Anmeldung", "AnzeigeText", DateTime.Now))
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

    private void MaschinenWechseln()
    {
      _VsMaschine.Source = _Db.tabMaschineSet.Where(w => (w.fStandort == Properties.Settings.Default.IdStandort) && (w.Status != JgMaschineData.EnumStatusMaschine.Stillgelegt)).ToList();
      _VsMaschine.View.CurrentChanged += View_CurrentChangedMaschine;
      View_CurrentChangedMaschine(null, null);
    }

    void View_CurrentChangedMaschine(object sender, EventArgs e)
    {
      if (_VsMaschine.View.CurrentItem != null)
      {
        var maschine = (JgMaschineData.tabMaschine)_VsMaschine.View.CurrentItem;
        var anzeigeAb = DateTime.Today.AddDays(-30);
        _VsReparatur.Source = _Db.tabReparaturSet.Where(w => (w.fMaschine == maschine.Id) && (w.VorgangBeginn >= anzeigeAb)).OrderBy(o => o.Id).Take(30).ToList();
      }
    }

    private void ButtonOptionen_Click(object sender, RoutedEventArgs e)
    {
      JgMaschineVerwalten.Fenster.FormOptionen form = new JgMaschineVerwalten.Fenster.FormOptionen(_Db);
      if (form.ShowDialog() ?? false)
      {
        InitTabelleAnmeldedaten();
        MaschinenWechseln();
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      Properties.Settings.Default.Save();
    }

    private void btnTabellenAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      TabellenAktualisieren();
    }
  }
}
