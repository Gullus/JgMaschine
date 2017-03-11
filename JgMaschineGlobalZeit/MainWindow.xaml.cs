using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using JgGlobalZeit.Commands;
using JgMaschineData;
using JgMaschineLib;
using Microsoft.Win32;
using JgZeitHelper;
using JgGlobalZeit.Fenster;

namespace JgMaschineGlobalZeit
{
  public partial class MainWindow : Window
  {
    private JgEntityTab<tabArbeitszeit> _ListeArbeitszeitenAuswahl;
    private JgZeit _DzArbeitszeitVon { get { return (JgZeit)FindResource("dzArbeitszeitVon"); } }
    private JgZeit _DzArbeitszeitBis { get { return (JgZeit)FindResource("dzArbeitszeitBis"); } }

    private JgEntityTab<tabAuswertung> _ListeReporteArbeitszeiten;
    private JgEntityTab<tabAuswertung> _ListeReporteAuswertung;
    private tabAuswertung _AktuellerReport = null;
    private FastReport.Report _Report;
    private FastReport.EnvironmentSettings _ReportSettings = new FastReport.EnvironmentSettings();

    private AnmeldungAuswertung _Erstellung;

    public MainWindow()
    {
      InitializeComponent();

      Helper.FensterEinstellung(this, JgGlobalZeit.Properties.Settings.Default);

      CommandBindings.Add(new CommandBinding(MyCommands.ArbeitszeitLoeschen, (sen, erg) =>
      {
        var az = _ListeArbeitszeitenAuswahl.Current;
        var msg = $"Arbeitszeit von {az.eBediener.Name} löschen ?";
        var ergBox = MessageBox.Show(msg, "Löschabfrage", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
        if (ergBox == MessageBoxResult.Yes)
        {
          az.AnzeigeGeloescht = true;
          _ListeArbeitszeitenAuswahl.DsSave();
        }
      },
      (sen, erg) =>
      {
        erg.CanExecute = _ListeArbeitszeitenAuswahl.Current?.DatenAbgleich.Geloescht == false;
      }));
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var heute = DateTime.Now.Date;
      _DzArbeitszeitVon.AnzeigeDatumZeit = heute;
      _DzArbeitszeitBis.AnzeigeDatumZeit = new DateTime(heute.Year, heute.Month, heute.Day, 23, 59, 59);

      _ListeArbeitszeitenAuswahl = new JgEntityTab<tabArbeitszeit>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsArbeitszeitAuswahl"),
        Tabellen = new DataGrid[] { dgArbeitszeitAuswahl },
        OnDatenLaden = (d, p) =>
        {
          var datVom = (DateTime)p["DatumVon"];
          var datBis = (DateTime)p["DatumBis"];

          var tabZeiten = d.tabArbeitszeitSet.Where(w => (((w.Anmeldung >= datVom) && (w.Anmeldung <= datBis))
            || ((w.Anmeldung == null) && (w.Abmeldung >= datVom) && (w.Abmeldung <= datBis))))
            .OrderBy(o => o.Anmeldung).ToList();

          var listeGerundet = d.tabArbeitszeitRundenSet.Where(w => !w.DatenAbgleich.Geloescht).ToList();
          foreach (var zeit in tabZeiten)
          {
            if (zeit.Anmeldung != null)
            {
              var zeitAnmeldung = new TimeSpan(zeit.Anmeldung.Value.Hour, zeit.Anmeldung.Value.Minute, 0);
              var wg = listeGerundet.FirstOrDefault(f => (zeitAnmeldung >= f.ZeitVon) && (zeitAnmeldung <= f.ZeitBis) && (f.fStandort == zeit.fStandort));
              if (wg != null)
                zeit.AnmeldungGerundetWert = zeit.Anmeldung.Value.Date.Add(wg.RundenArbeitszeitBeginn);
            }
          }

          return tabZeiten;
        }
      };
      _ListeArbeitszeitenAuswahl.Parameter = new Dictionary<string, object>()
      {
        { "DatumVon", _DzArbeitszeitVon.AnzeigeDatumZeit },
        { "DatumBis", _DzArbeitszeitBis.AnzeigeDatumZeit }
      };
      _ListeArbeitszeitenAuswahl.DatenLaden();

      // Report initialisieren ********************************

      _ListeReporteArbeitszeiten = new JgEntityTab<tabAuswertung>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsReporteArbeitszeit")
      };
      var ausw = _ListeReporteArbeitszeiten.Db.tabAuswertungSet
        .Where(w => ((w.FilterAuswertung == EnumFilterAuswertung.Arbeitszeit) || (w.FilterAuswertung == EnumFilterAuswertung.ArbeitszeitAuswertung)) && (!w.DatenAbgleich.Geloescht))
        .ToList();
      _ListeReporteArbeitszeiten.Daten = ausw
        .Where(w => w.FilterAuswertung == EnumFilterAuswertung.Arbeitszeit)
        .OrderBy(o => o.AnzeigeReportname).ToList();

      _ListeReporteAuswertung = new JgEntityTab<tabAuswertung>(_ListeReporteArbeitszeiten.Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsReporteAuswertung")
      };
      _ListeReporteArbeitszeiten.Daten = ausw
        .Where(w => w.FilterAuswertung == EnumFilterAuswertung.ArbeitszeitAuswertung)
        .OrderBy(o => o.AnzeigeReportname).ToList();

      // Auswerung intitialisieren

      _Erstellung = new AnmeldungAuswertung(new JgModelContainer(), cbJahr, cbMonat,
        (CollectionViewSource)FindResource("vsBediener"),
        (ArbeitszeitSummen)FindResource("AuswertungKumulativ"), (ArbeitszeitSummen)FindResource("AuswertungGesamt"),
        (CollectionViewSource)FindResource("vsArbeitszeitTage"));

      // Report für Auswertung erstellen

      _Report = new FastReport.Report();
      _Report.FileName = "Datenbank";
      _ReportSettings.CustomSaveReport += (obj, repEvent) =>
      {
        MemoryStream memStr = new MemoryStream();
        try
        {
          repEvent.Report.Save(memStr);
          _AktuellerReport.Report = memStr.ToArray();
          _AktuellerReport.GeaendertDatum = DateTime.Now;
          _AktuellerReport.GeaendertName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
          _ListeReporteArbeitszeiten.DsSave(_AktuellerReport);
          if (_AktuellerReport.FilterAuswertung == EnumFilterAuswertung.ArbeitszeitAuswertung)
            _ListeReporteAuswertung.Refresh();
          else
            _ListeReporteArbeitszeiten.Refresh();
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
      var az = _ListeArbeitszeitenAuswahl.Current;
      var msg = $"Korrektur der Arbeitszeit für den Mitarbeiter {az.eBediener.Name}.";
      var anm = az.Anmeldung ?? DateTime.Now;
      var abm = az.Abmeldung ?? DateTime.Now;

      if (JgZeit.AbfrageZeit(msg, " Zeitabfrage !", ref anm, ref abm))
      {
        if (anm != az.Anmeldung)
        {
          az.AnmeldungGerundetWert = null;
          if (anm != null)
          {
            var zeit = JgZeit.DatumInZeit(anm);
            var azBegin = _Erstellung.Db.tabArbeitszeitRundenSet.FirstOrDefault(w =>
              (w.fStandort == az.fStandort)
              && (zeit >= w.ZeitVon) && (zeit <= w.ZeitBis)
              && (w.Jahr == anm.Year) && (w.Monat == anm.Month) && (!w.DatenAbgleich.Geloescht));
            if (azBegin != null)
              az.AnmeldungGerundetWert = anm.Date + azBegin.RundenArbeitszeitBeginn;
          }

          az.AnzeigeAnmeldung = anm;
          _ListeArbeitszeitenAuswahl.DsSave();
        }
        if (abm != az.Abmeldung)
        {
          az.AnzeigeAbmeldung = abm;
          _ListeArbeitszeitenAuswahl.DsSave();
        }
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
          case EnumFilterAuswertung.Arbeitszeit: _AktuellerReport = _ListeReporteArbeitszeiten.Current; break;
          case EnumFilterAuswertung.ArbeitszeitAuswertung: _AktuellerReport = _ListeReporteAuswertung.Current; break;
        }

        if (_AktuellerReport == null)
        {
          MessageBox.Show("Es wurde kein Report ausgewählt.", "Fehler !", MessageBoxButton.OK, MessageBoxImage.Information);
          return;
        }

        switch (vorgang)
        {
          case 0: // Reportname ändern
            var formNeu = new Fenster.FormReportName(_AktuellerReport.ReportName);
            if (formNeu.ShowDialog() ?? false)
            {
              _AktuellerReport.AnzeigeReportname = formNeu.ReportName;
              _ListeReporteArbeitszeiten.DsSave();
            }
            return;
          case 5: // Exportieren
            SaveFileDialog dia = new SaveFileDialog();
            dia.FileName = _AktuellerReport.ReportName;
            dia.Filter = "Fastreport (*.frx)|*.frx|Alle Dateien (*.*)|*.*";
            dia.FilterIndex = 1;
            if (dia.ShowDialog() ?? false)
            {
              _Report.Save(dia.FileName);
              MemoryStream mem;
              mem = new MemoryStream(_AktuellerReport.Report);
              using (Stream f = File.Create(dia.FileName))
              {
                mem.CopyTo(f);
              }
            }
            return;
          case 6:  // Report löschen
            var mb = MessageBox.Show($"Report {_AktuellerReport.ReportName} löschen ?", "Löschabfrage", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None);
            if (mb == MessageBoxResult.Yes)
            {
              _AktuellerReport.DatenAbgleich.Geloescht = true;
              switch (auswahl)
              {
                case EnumFilterAuswertung.Arbeitszeit: _ListeReporteArbeitszeiten.Remove(_AktuellerReport); break;
                case EnumFilterAuswertung.ArbeitszeitAuswertung: _ListeReporteAuswertung.Remove(_AktuellerReport); break;
              }
            }
            return;
        }

        _Report.Clear();
        if (_AktuellerReport.Report == null)
          vorgang = 3;
        else
        {
          var mem = new MemoryStream(_AktuellerReport.Report);
          _Report.Load(mem);
        }
      }

      switch (auswahl)
      {
        case EnumFilterAuswertung.Arbeitszeit:
          var bediener = _ListeArbeitszeitenAuswahl.Daten.Select(s => new { Id = s.fBediener, Name = s.eBediener.NachName + ", " + s.eBediener.VorName }).Distinct().ToList();
          _Report.RegisterData(bediener, "Bediener");
          _Report.RegisterData(_ListeArbeitszeitenAuswahl.Daten, "Daten");

          _Report.SetParameterValue("Zeitraum.DatumVon", _DzArbeitszeitVon.AnzeigeDatumZeit);
          _Report.SetParameterValue("Zeitraum.DatumBis", _DzArbeitszeitBis.AnzeigeDatumZeit);
          _Report.SetParameterValue("IstAktuell", tcArbeitszeit.SelectedIndex == 0);
          break;
        case EnumFilterAuswertung.ArbeitszeitAuswertung:
          var aktStandort = _Erstellung.AktuellerBediener.eStandort;

          var bedienerStandort = _Erstellung.ListeBediener.Daten.Where(w => w.fStandort == aktStandort.Id).ToList();
          var listeAuswertung = new List<ArbeitszeitBediener>();

          foreach (var bedAusw in bedienerStandort)
          {
            var ds = new ArbeitszeitBediener(_Erstellung.Db)
            {
              AuswertungKumulativ = new ArbeitszeitSummen(),
              AuswertungGesamt = new ArbeitszeitSummen(),
              ListeTage = new ObservableCollection<tabArbeitszeitTag>()
            };
            ds.BedienerBerechnen(bedAusw, _Erstellung.Jahr, _Erstellung.Monat, _Erstellung.SollStundenMonat, _Erstellung.ListeRundenMonat, _Erstellung.ListeFeiertageMonat, _Erstellung.ListePausen.Daten);
            listeAuswertung.Add(ds);
          }

          _Report.RegisterData(bedienerStandort.Select(s => new { s.Id, s.Name }).ToList(), "Bediener");
          _Report.RegisterData(listeAuswertung, "ListeAuswertung");
          _Report.SetParameterValue("Auswertung.Monat", (JgZeitHelper.JgZeit.Monate)_Erstellung.Monat);
          _Report.SetParameterValue("Auswertung.Jahr", _Erstellung.Jahr);
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
        _AktuellerReport = new tabAuswertung()
        {
          Id = Guid.NewGuid(),
          FilterAuswertung = auswahl,
          ReportName = repName,
          ErstelltDatum = DateTime.Now,
          ErstelltName = username,
          GeaendertDatum = DateTime.Now,
          GeaendertName = username,
        };

        switch (auswahl)
        {
          case EnumFilterAuswertung.Arbeitszeit: _ListeReporteArbeitszeiten.Add(_AktuellerReport); break;
          case EnumFilterAuswertung.Anmeldung: _ListeReporteAuswertung.Add(_AktuellerReport); break;
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
      _ListeArbeitszeitenAuswahl.Parameter = new Dictionary<string, object>()
      {
        { "DatumVon", _DzArbeitszeitVon.AnzeigeDatumZeit },
        { "DatumBis", _DzArbeitszeitBis.AnzeigeDatumZeit }
      };
      _ListeArbeitszeitenAuswahl.DatenNeuLaden();
    }

    private void btnOptionen_Click(object sender, RoutedEventArgs e)
    {
      var formOptionen = new Fenster.FormOptionen(_Erstellung);
      formOptionen.ShowDialog();
      _Erstellung.Db.SaveChanges();
    }

    private void btnSollStundenEinstellen_Click(object sender, RoutedEventArgs e)
    {
      var form = new FormSollstundenEinstellen(_Erstellung.AktuellerBediener.eArbeitszeitHelper.SollStunden);
      if (form.ShowDialog() ?? false)
        _Erstellung.AuswertungBediener.SetSollstunden(form.Sollstunden);
    }

    private void btnUeberstundenAuszahlen_Click(object sender, RoutedEventArgs e)
    {
      var form = new JgGlobalZeit.Fenster.FormUeberstundenAuszahlen(_Erstellung.AktuellerBediener.eArbeitszeitHelper.AuszahlungUeberstunden);
      if (form.ShowDialog() ?? false)
        _Erstellung.AuswertungBediener.SetUebestundenAuszahlung(form.UerbstundenAuszahlem);
    }

    private void btnAuswertungErledigt_Click(object sender, RoutedEventArgs e)
    {
      var bediener = _Erstellung.AktuellerBediener;
      if (bediener?.eArbeitszeitHelper != null)
      {
        bediener.eArbeitszeitHelper.StatusAnzeige = (EnumStatusArbeitszeitAuswertung)Convert.ToByte((sender as Button).Tag);
        _Erstellung.Db.SaveChanges();
      }
    }

    private void btnExporteren_Click(object sender, RoutedEventArgs e)
    {
      var datName = JgGlobalZeit.Properties.Settings.Default.NameXmlDatei;
      if (string.IsNullOrWhiteSpace(datName))
        datName = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\JgArbeitszeit.xml";

      var fo = new SaveFileDialog()
      {
        Title = "Xml Datei speichern",
        FileName = Path.GetFileName(datName),
        InitialDirectory = Path.GetDirectoryName(datName),
        Filter = "Xml Files | *.xml | Alle Dateien | *.*",
      };

      if (fo.ShowDialog() ?? false)
      {
        var daten = _Erstellung.ListeBediener.Daten.Where(w => (w.eArbeitszeitHelper != null) && (w.eArbeitszeitHelper.Status == EnumStatusArbeitszeitAuswertung.Fertig)).ToList();


        var anzahlFeiertage = _Erstellung.ListeFeiertageMonat.Count();
        var en = new CultureInfo("en-US", false);

        XDocument xDoc = new XDocument(
          new XComment($"Arbeitszeit Monat: {_Erstellung.Monat}.{_Erstellung.Jahr} Datum: {DateTime.Now.ToString("dd.MM.yy HH:mm")}"),
          new XElement("Root",
          new XElement("Monat", $"{(JgZeit.Monate)_Erstellung.Monat} {_Erstellung.Jahr.ToString()}"),

          from z in daten
          select new XElement("Datensatz",
            new XElement("Mitarbeiter", z.Name),
            new XElement("Nachname", z.NachName),
            new XElement("Vorname", z.VorName),
            new XElement("IdBuchhaltung", z.IdBuchhaltung),

            new XElement("Standort", z.eStandort.Bezeichnung),
            new XElement("Zahltag", z.AuszahlungGehalt),
            new XElement("Urlaubstage", z.Urlaubstage),

            new XElement("SollStunden", JgZeit.StringInZeit(z.eArbeitszeitHelper.SollStunden).TotalHours.ToString("N1", en)),

            new XElement("Normalstunden", (JgZeit.StringInZeit(z.eArbeitszeitHelper.SollStunden)
              - (new TimeSpan(8 * (z.eArbeitszeitHelper.Urlaub + z.eArbeitszeitHelper.Krank + anzahlFeiertage), 0, 0))).TotalHours.ToString("N1", en)),

            new XElement("UeberstundenAusgezahlt", JgZeit.StringInZeit(z.eArbeitszeitHelper.AuszahlungUeberstunden).TotalHours.ToString("N1", en)),

            new XElement("Urlaub", z.eArbeitszeitHelper.Urlaub * 8),
            new XElement("Krank", z.eArbeitszeitHelper.Krank * 8),
            new XElement("Feiertage", anzahlFeiertage * 8),

            // Formatierung als Dezimalzahl mit einer Kommastelle mit Frau Glatter besprochen

            new XElement("NachtschichtZuschlag", JgZeit.StringInZeit(z.eArbeitszeitHelper.Nachtschichten).TotalHours.ToString("N1", en)),
            new XElement("NachtschichtZuschlagGerundet", z.eArbeitszeitHelper.NachtschichtGerundet.ToString("N1", en)),
            new XElement("FeiertagsZuschlag", JgZeit.StringInZeit(z.eArbeitszeitHelper.Feiertage).TotalHours.ToString("N1", en)),
            new XElement("FeiertagsZuschlagGerundet", z.eArbeitszeitHelper.FeiertageGerundet.ToString("N1", en)),

            new XElement("IstStunden", JgZeit.StringInZeit(z.eArbeitszeitHelper.IstStunden).TotalHours.ToString("N1", en)),
            new XElement("UeberStunden", JgZeit.StringInZeit(z.eArbeitszeitHelper.Ueberstunden).TotalHours.ToString("N1", en))
            )
          )
        );

        try
        {
          xDoc.Save(fo.FileName);
        }
        catch (Exception f)
        {
          var msg = $"Datei konnte nicht erstellt werden.\nGrund: {f.Message}";
          MessageBox.Show(msg, "Fehlermeldung", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }

        if (JgGlobalZeit.Properties.Settings.Default.NameXmlDatei != fo.FileName)
        {
          JgGlobalZeit.Properties.Settings.Default.NameXmlDatei = fo.FileName;
          JgGlobalZeit.Properties.Settings.Default.Save();
        }

        foreach (var bed in daten)
          bed.eArbeitszeitHelper.StatusAnzeige = EnumStatusArbeitszeitAuswertung.Erledigt;

        _Erstellung.Db.SaveChanges();

        MessageBox.Show("Datei gespeichert !", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
      }
    }

    private void Datagrid_DoppelClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      var aktTag = (FindResource("vsArbeitszeitTage") as CollectionViewSource).View.CurrentItem;
      if (aktTag != null)
      {
        var aktDs = (tabArbeitszeitTag)aktTag;
        if (dgArbeitszeit.CurrentColumn == clPauseBerechnet)
          aktDs.PauseAnzeige = JgZeit.ZeitInString(aktDs.PauseBerechnet);
        else if (dgArbeitszeit.CurrentColumn == clZeitBerechnet)
          aktDs.ZeitAnzeige = JgZeit.ZeitInString(aktDs.ZeitBerechnet);
        else if (dgArbeitszeit.CurrentColumn == clNachtschichtBerechnet)
          aktDs.NachtschichtAnzeige = JgZeit.ZeitInString(aktDs.NachtschichtBerechnet);
      }
    }

    private void NeueArbeitszeit_Click(object sender, RoutedEventArgs e)
    {
      var standorte = _Erstellung.Db.tabStandortSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.Bezeichnung).ToList();
      var form = new JgMaschineSetup.Fenster.FormNeueArbeitszeit(standorte, _Erstellung.ListeBediener.Daten);
      if (form.ShowDialog() ?? false)
      {
        var az = new tabArbeitszeit()
        {
          Id = Guid.NewGuid(),
          fStandort = form.Standort.Id,
          fBediener = form.Bediener.Id,
          Anmeldung = form.DatumVon,
          ManuelleAnmeldung = true,
          Abmeldung = form.DatumBis,
          ManuelleAbmeldung = true,
        };
        _ListeArbeitszeitenAuswahl.Add(az);
      }
    }
  }
}
