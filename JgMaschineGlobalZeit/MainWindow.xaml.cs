using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineLib.Zeit;
using Microsoft.Win32;

namespace JgMaschineGlobalZeit
{
  public partial class MainWindow : Window
  {
    private JgEntityView<tabArbeitszeit> _ListeArbeitszeitAuswahl;
    private JgDatumZeit _DzArbeitszeitVon { get { return (JgDatumZeit)FindResource("dzArbeitszeitVon"); } }
    private JgDatumZeit _DzArbeitszeitBis { get { return (JgDatumZeit)FindResource("dzArbeitszeitBis"); } }

    private CollectionViewSource _VsAuswertungArbeitszeit { get { return (CollectionViewSource)FindResource("vsAuswertungArbeitszeit"); } }
    private CollectionViewSource _VsAuswertungAuswertung { get { return (CollectionViewSource)FindResource("vsAuswertungAuswertung"); } }

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
        (CollectionViewSource)FindResource("vsBediener"))
      {
        AuswertungMonat = (ArbeitszeitAuswertungDs)FindResource("AuswertungMonat"),
        AuswertungKumulativ = (ArbeitszeitAuswertungDs)FindResource("AuswertungKumulativ"),
        AuswertungGesamt = (ArbeitszeitAuswertungDs)FindResource("AuswertungGesamt"),
      };

      (FindResource("vsArbeitszeitTage") as CollectionViewSource).Source = _Erstellung.ListeAnzeigeTage;

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
      _ListeAuswertung.Daten = await _ListeArbeitszeitAuswahl.Db.tabAuswertungSet.Where(w => ((w.FilterAuswertung == EnumFilterAuswertung.Arbeitszeit) || (w.FilterAuswertung == EnumFilterAuswertung.ArbeitszeitAuswertung)) && (!w.DatenAbgleich.Geloescht)).ToListAsync();
      var vsArbeitszeit = (CollectionViewSource)FindResource("vsAuswertungArbeitszeit");
      vsArbeitszeit.Source = _ListeAuswertung.Daten;
      vsArbeitszeit.Filter += (sen, erg) => erg.Accepted = (erg.Item as tabAuswertung).FilterAuswertung == EnumFilterAuswertung.Arbeitszeit;

      var vsAuswertung = (CollectionViewSource)FindResource("vsAuswertungAuswertung");
      vsAuswertung.Source = _ListeAuswertung.Daten;
      vsAuswertung.Filter += (sen, erg) => erg.Accepted = (erg.Item as tabAuswertung).FilterAuswertung == EnumFilterAuswertung.ArbeitszeitAuswertung;

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

    private void Window_Closed(object sender, EventArgs e)
    {
      JgGlobalZeit.Properties.Settings.Default.Save();
    }

    private void btnDrucken_Click(object sender, RoutedEventArgs e)
    {
      var vorgang = Convert.ToInt32((sender as Button).Tag);  // 1 - Anzeigen, 2 - Drucken, 3 - Design, 4 - Neuer Report, 5 - Report Exportieren, 6 - Löschen

      var auswahl = EnumFilterAuswertung.Arbeitszeit;
      if (tcArbeitszeit.SelectedIndex == 1)
        auswahl = EnumFilterAuswertung.ArbeitszeitAuswertung;

      if (vorgang != 4)
      {
        switch (auswahl)
        {
          case EnumFilterAuswertung.Arbeitszeit: _AktAuswertung = (tabAuswertung)_VsAuswertungArbeitszeit.View.CurrentItem; break;
          case EnumFilterAuswertung.ArbeitszeitAuswertung: _AktAuswertung = (tabAuswertung)_VsAuswertungAuswertung.View.CurrentItem; break;
        }

        if (_AktAuswertung == null)
        {
          MessageBox.Show("Es wurde kein Report ausgewählt.", "Fehler !", MessageBoxButton.OK, MessageBoxImage.Information);
          return;
        }

        switch (vorgang)
        {
          case 0: // Reportname ändern
            var formNeu = new Fenster.FormReportName(_AktAuswertung.ReportName);
            if (formNeu.ShowDialog() ?? false)
            {
              _ListeAuswertung.Db.tabAuswertungSet.Attach(_AktAuswertung);
              _AktAuswertung.AnzeigeReportname = formNeu.ReportName;
              _ListeAuswertung.Db.SaveChanges();
            }
            return;
          case 5: // Exportieren
            SaveFileDialog dia = new SaveFileDialog();
            dia.FileName = _AktAuswertung.ReportName;
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
              _ListeAuswertung.AlsGeloeschtKennzeichnen(_AktAuswertung);
              switch (auswahl)
              {
                case EnumFilterAuswertung.Arbeitszeit: _VsAuswertungArbeitszeit.View.Refresh(); break;
                case EnumFilterAuswertung.Anmeldung: _VsAuswertungAuswertung.View.Refresh(); break;
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

      switch (auswahl)
      {
        case EnumFilterAuswertung.Arbeitszeit:
          var bediener = _ListeArbeitszeitAuswahl.Daten.Select(s => new { Id = s.fBediener, Name = s.eBediener.NachName + ", " + s.eBediener.VorName }).Distinct().ToList();
          _Report.RegisterData(bediener, "Bediener");
          _Report.RegisterData(_ListeArbeitszeitAuswahl.Daten, "Daten");

          _Report.SetParameterValue("DatumVon", _DzArbeitszeitVon.Datum);
          _Report.SetParameterValue("DatumBis", _DzArbeitszeitBis.Datum);
          _Report.SetParameterValue("IstAktuell", tcArbeitszeit.SelectedIndex == 0);
          break;
        case EnumFilterAuswertung.ArbeitszeitAuswertung:
          _Report.RegisterData(new List<ArbeitszeitAuswertungDs>() { _Erstellung.AuswertungKumulativ }, "AuswertungKumulativ");
          _Report.RegisterData(new List<ArbeitszeitAuswertungDs>() { _Erstellung.AuswertungMonat }, "AuswertungMonat");
          _Report.RegisterData(new List<ArbeitszeitAuswertungDs>() { _Erstellung.AuswertungGesamt }, "AuswertungGesamt");
          _Report.RegisterData(_Erstellung.ListeAnzeigeTage, "AuswertungTage");
          break;
        default:
          break;
      }

      if (vorgang == 4) // Neuer Report
      {
        var repName = "";

        var formNeu = new Fenster.FormReportName();
        if (!formNeu.ShowDialog() ?? false)
          return;
        repName = formNeu.ReportName;

        string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        _AktAuswertung = new tabAuswertung()
        {
          Id = Guid.NewGuid(),
          FilterAuswertung = auswahl,
          ReportName = repName,
          ErstelltDatum = DateTime.Now,
          ErstelltName = username,
          GeaendertDatum = DateTime.Now,
          GeaendertName = username
        };
        _ListeAuswertung.Add(_AktAuswertung);

        switch (auswahl)
        {
          case EnumFilterAuswertung.Arbeitszeit: _VsAuswertungArbeitszeit.View.MoveCurrentTo(_AktAuswertung); break;
          case EnumFilterAuswertung.Anmeldung: _VsAuswertungAuswertung.View.MoveCurrentTo(_AktAuswertung); break;
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

    private void btnAuswahlAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      _ListeArbeitszeitAuswahl.DatenAktualisieren();
    }

    private void btnOptionen_Click(object sender, RoutedEventArgs e)
    {
      var formOptionen = new Fenster.FormOptionen(_Erstellung);
      formOptionen.ShowDialog();
      _Erstellung.Db.SaveChanges();
    }

    private void btnSollStundenEinstellen_Click(object sender, RoutedEventArgs e)
    {
      var form = new JgGlobalZeit.Fenster.FormSollstundenEinstellen(_Erstellung.AuswertungMonat.SollStundenString);
      if (form.ShowDialog() ?? false)
        _Erstellung.SetSollstunden(form.Sollstunden);
    }

    private void btnUeberstundenAuszahlen_Click(object sender, RoutedEventArgs e)
    {
      var form = new JgGlobalZeit.Fenster.FormUeberstundenAuszahlen(_Erstellung.AuswertungMonat.UeberstundenBezahltString);
      if (form.ShowDialog() ?? false)
        _Erstellung.SetUebestundenAuszahlung(form.UerbstundenAuszahlem);
    }
  }
}
