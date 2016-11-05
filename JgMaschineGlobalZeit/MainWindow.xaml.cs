using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineLib.Zeit;

namespace JgMaschineGlobalZeit
{
  public partial class MainWindow : Window
  {
    private JgEntityView<tabArbeitszeit> _ListeArbeitszeitAuswahl;
    private JgDatumZeit _DzArbeitszeitVon { get { return (JgDatumZeit)FindResource("dzArbeitszeitVon"); } }
    private JgDatumZeit _DzArbeitszeitBis { get { return (JgDatumZeit)FindResource("dzArbeitszeitBis"); } }

    private FastReport.Report _Report;
    private tabAuswertung _AktAuswertung = null;
    private JgEntityView<tabAuswertung> _ListeAuswertung;
    private FastReport.EnvironmentSettings _ReportSettings = new FastReport.EnvironmentSettings();

    private AnmeldungAuswertung _Erstellung;

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _Erstellung = new AnmeldungAuswertung(new JgModelContainer(), cbJahr, cbMonat, 
        (CollectionViewSource)FindResource("vsBediener"), (CollectionViewSource)FindResource("vsArbeitszeitTage"))
      {
        AuswertungMonat = (tabArbeitszeitAuswertung)FindResource("AuswertungMonat"),
        AuswertungKumulativ = (tabArbeitszeitAuswertung)FindResource("AuswertungKumulativ"),
        AuswertungGesamt = (tabArbeitszeitAuswertung)FindResource("AuswertungGesamt"),
      };

      var heute = DateTime.Now.Date;
      _DzArbeitszeitVon.DatumZeit = heute;
      _DzArbeitszeitBis.DatumZeit = new DateTime(heute.Year, heute.Month, heute.Day, 23, 59, 59);

      _ListeArbeitszeitAuswahl = new JgEntityView<tabArbeitszeit>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsArbeitszeitAuswahl"),
        Tabellen = new DataGrid[] { dgArbeitszeitAuswahl },
        DatenErstellen = (dbIntern) =>
        {
          return dbIntern.tabArbeitszeitSet.Where(w => (w.Anmeldung >= _DzArbeitszeitVon.DatumZeit) && ((w.Anmeldung <= _DzArbeitszeitBis.DatumZeit)))
            .OrderBy(o => o.Anmeldung).ToList();
        }
      };

      // Auswertung initialisieren ********************************

      _ListeAuswertung = new JgEntityView<tabAuswertung>();
      _ListeAuswertung.Daten = await _ListeArbeitszeitAuswahl.Db.tabAuswertungSet.Where(w => (w.FilterAuswertung != EnumFilterAuswertung.Arbeitszeit))
        .OrderBy(o => o.ReportName).ToListAsync();

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
          MessageBox.Show("Fehler beim speichern des Reports !\r\nGrund: " + f.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
          memStr.Dispose();
        }
      };
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

    private void Window_Closed(object sender, EventArgs e)
    {
      JgGlobalZeit.Properties.Settings.Default.Save();
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

      _Report.RegisterData(_ListeArbeitszeitAuswahl.Daten, "Daten");
      _Report.SetParameterValue("DatumVon", _DzArbeitszeitVon);
      _Report.SetParameterValue("DatumBis", _DzArbeitszeitBis);
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

          _ListeAuswertung.ViewSource.View.MoveCurrentTo(_AktAuswertung);
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

    private void btnAuswahlAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      _ListeArbeitszeitAuswahl.DatenAktualisieren();
    }

    private void btnOptionen_Click(object sender, RoutedEventArgs e)
    {
      var formOptionen = new Fenster.FormOptionen(_Erstellung);
      formOptionen.ShowDialog();
      _Erstellung.Db.SaveChanges();
      _Erstellung.MonatOderJahrGeandert();
    }
  }
}
