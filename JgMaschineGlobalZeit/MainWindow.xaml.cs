using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineLib.Zeit;

namespace JgGlobalZeit
{
  public partial class MainWindow : Window
  {
    private JgModelContainer _Db;
    private tabStandort _Standort;

    #region Bediener

    private tabBediener _AuswahlBedienerArbeitszeit { get { return (tabBediener)(FindResource("vsBedienerArbeitszeit") as CollectionViewSource).View.CurrentItem; } }
    private tabBediener _AuswahlBedienerAnmeldung { get { return (tabBediener)(FindResource("vsBedienerAnmeldung") as CollectionViewSource).View.CurrentItem; } }

    #endregion

    private JgEntityAuto _DatenPool;
    private JgEntityTab<tabBediener> _JgBediener { get { return (JgEntityTab<tabBediener>)_DatenPool.Tabs[JgEntityAuto.TabArt.Bediener]; } }
    private JgEntityTab<tabArbeitszeit> _JgArbeitszeit { get { return (JgEntityTab<tabArbeitszeit>)_DatenPool.Tabs[JgEntityAuto.TabArt.Arbeitszeit]; } }

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
      _Standort = await _Db.tabStandortSet.FirstOrDefaultAsync(f => f.Id == Properties.Settings.Default.IdStandort);
      if (_Standort == null)
        _Standort = await _Db.tabStandortSet.FirstOrDefaultAsync();

      tblStandort.Text = _Standort.Bezeichnung;

      await InitListen();

      _Report = new FastReport.Report();
      _Report.FileName = "Datenbank";
      _ReportSettings.CustomSaveReport += (obj, repEvent) =>
      {
        MemoryStream memStr = new MemoryStream();
        try
        {
          repEvent.Report.Save(memStr);
          _ListeAuswertung.Db.tabAuswertungSet.Attach(_AktAuswertung);
          _AktAuswertung.Report = memStr.ToArray();
          _AktAuswertung.GeaendertDatum = DateTime.Now;
          _AktAuswertung.GeaendertName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
          DbSichern.AbgleichEintragen(_AktAuswertung.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
          _ListeAuswertung.Db.SaveChanges();
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
      _DatenPool = new JgEntityAuto(JgGlobalZeit.Properties.Settings.Default.VerbindungsString, 5)
      {
        TimerAusgeloest = () =>
        {
          tblDatum.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
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


      // Auswertung initialisieren ********************************

      _ListeAuswertung = new JgEntityView<tabAuswertung>();
      _ListeAuswertung.Daten = await _Db.tabAuswertungSet.Where(w => w.FilterAuswertung != EnumFilterAuswertung.Allgemein)
        .OrderBy(o => o.ReportName).ToListAsync();

      _VsAuswertungArbeitszeit.Source = _ListeAuswertung.Daten;
      _VsAuswertungArbeitszeit.Filter += (sen, erg) => erg.Accepted = (erg.Item as tabAuswertung).FilterAuswertung == EnumFilterAuswertung.Arbeitszeit;
    }

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

      var form = new FormAuswahlDatumVonBis("Berichtigung Arbeitszeit", anz, _ListeArbeitszeitAuswahl.Current.Anmeldung ?? DateTime.Now, _ListeArbeitszeitAuswahl.Current.Abmeldung ?? DateTime.Now);
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

    private void btnDrucken_Click(object sender, RoutedEventArgs e)
    {
      var vorgang = Convert.ToInt32((sender as Button).Tag);  // 1 - Anzeigen, 2 - Drucken, 3 - Design, 4 - Neuer Report
      _Report.Clear();
      if (_AktAuswertung.Report == null)
        vorgang = 3;
      else
      {
        var mem = new MemoryStream(_AktAuswertung.Report);
        _Report.Load(mem);
      }

      if (tcArbeitszeit.SelectedIndex == 0)
        _Report.RegisterData(_JgArbeitszeit.Daten, "Daten");
      else
      {
        _Report.RegisterData(_ListeArbeitszeitAuswahl.Daten, "Daten");
        _Report.SetParameterValue("DatumVon", _DzArbeitszeitVon);
        _Report.SetParameterValue("DatumBis", _DzArbeitszeitBis);
      }
      _Report.SetParameterValue("IstAktuell", tcArbeitszeit.SelectedIndex == 0);

      if (vorgang == 4)
      {
        Fenster.FormNeuerReport form = new Fenster.FormNeuerReport();
        if (form.ShowDialog() ?? false)
        {
          string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
          _AktAuswertung = new JgMaschineData.tabAuswertung()
          {
            Id = Guid.NewGuid(),
            FilterAuswertung = 0,
            ReportName = form.ReportName,
            ErstelltDatum = DateTime.Now,
            ErstelltName = username,
            GeaendertDatum = DateTime.Now,
            GeaendertName = username
          };
          _ListeAuswertung.Add(_AktAuswertung);

            _VsAuswertungArbeitszeit.View.MoveCurrentTo(_AktAuswertung);
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


    private void btnFastCube_Click(object sender, RoutedEventArgs e)
    {
      var dat = Helper.StartVerzeichnis() + @"\jgFastCube";
      if (!Directory.Exists(dat))
        Directory.CreateDirectory(dat);

      var prop = Properties.Settings.Default;
      JgMaschineLib.Imports.JgFastCube.JgFastCubeStart(prop.JgCubeVerbindungsString, prop.JgCubeSqlText, dat);
    }

    private void btnAuswahlAktualisieren_Click(object sender, RoutedEventArgs e)
    {
        _ListeArbeitszeitAuswahl.DatenAktualisieren();
    }
  }
}
