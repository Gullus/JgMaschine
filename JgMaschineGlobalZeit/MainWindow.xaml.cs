using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using JgMaschineData;
using JgMaschineLib;
using Microsoft.Win32;
using JgZeitHelper;
using JgMaschineGlobalZeit.Commands;
using JgMaschineGlobalZeit.Fenster;

namespace JgMaschineGlobalZeit
{
  public partial class MainWindow : Window
  {
    private JgModelContainer _Db;

    private JgEntityList<tabArbeitszeit> _ListeArbeitszeitenAuswahl;
    private JgZeit _DzArbeitszeitVon { get { return (JgZeit)FindResource("dzArbeitszeitVon"); } }
    private JgZeit _DzArbeitszeitBis { get { return (JgZeit)FindResource("dzArbeitszeitBis"); } }

    private JgEntityList<tabAuswertung> _ListeReporteArbeitszeiten;
    private JgEntityList<tabAuswertung> _ListeReporteAuswertung;
    private tabAuswertung _AktuellerReport = null;
    private FastReport.Report _Report;
    private FastReport.EnvironmentSettings _ReportSettings = new FastReport.EnvironmentSettings();

    private AnmeldungAuswertung _Erstellung;

    public MainWindow()
    {
      InitializeComponent();

      Helper.FensterEinstellung(this, Properties.Settings.Default);
      InitCommands();
    }

    private void InitCommands()
    {
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

      void IstArbeitzeitAndern(object Obj, CanExecuteRoutedEventArgs Erg)
      {
        Erg.CanExecute = _Erstellung.AktuellerBediener.eArbeitszeitHelper.Status == EnumStatusArbeitszeitAuswertung.InArbeit;
      }

      CommandBindings.Add(new CommandBinding(MyCommands.SollStundenAendern, (sen, erg) =>
      {
        var form = new FormSollstundenEinstellen(_Erstellung.AktuellerBediener.eArbeitszeitHelper.SollStunden);
        if (form.ShowDialog() ?? false)
          _Erstellung.AzBediener.SetSollstunden(form.Sollstunden);
      }, IstArbeitzeitAndern));

      CommandBindings.Add(new CommandBinding(MyCommands.UberstundenBezahltAendern, (sen, erg) =>
      {
        var form = new FormUeberstundenAuszahlen(_Erstellung.AzBediener.AuswertungMonat.UeberstundenBezahltAnzeige);
        if (form.ShowDialog() ?? false)
          _Erstellung.AzBediener.SetUebestundenAuszahlung(form.UerbstundenAuszahlem);
      }, IstArbeitzeitAndern));
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _Db = new JgModelContainer();
      if (Properties.Settings.Default.VerbindungsString != "")
        _Db.Database.Connection.ConnectionString = Properties.Settings.Default.VerbindungsString;

      var heute = DateTime.Now.Date;
      _DzArbeitszeitVon.AnzeigeDatumZeit = heute;
      _DzArbeitszeitBis.AnzeigeDatumZeit = new DateTime(heute.Year, heute.Month, heute.Day, 23, 59, 59);

      _ListeArbeitszeitenAuswahl = new JgEntityList<tabArbeitszeit>(_Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsArbeitszeitAuswahl"),
        Tabellen = new DataGrid[] { dgArbeitszeitAuswahl },
        OnDatenLaden = (d, p) =>
        {
          var datVom = (DateTime)p.Params["DatumVon"];
          var datBis = (DateTime)p.Params["DatumBis"];

          var lZeiten = d.tabArbeitszeitSet.Where(w => (((w.Anmeldung >= datVom) && (w.Anmeldung <= datBis))
            || ((w.Anmeldung == null) && (w.Abmeldung >= datVom) && (w.Abmeldung <= datBis))));

          if (!p.IstSortiert)
            lZeiten = lZeiten.OrderBy(o => o.Anmeldung);

          var listeGerundet = d.tabArbeitszeitRundenSet.Where(w => !w.DatenAbgleich.Geloescht).ToList();
          foreach (var zeit in lZeiten.ToList())
          {
            if (zeit.Anmeldung != null)
            {
              var zeitAnmeldung = JgZeit.DatumInZeitMinute(zeit.Anmeldung.Value);
              var wg = listeGerundet.FirstOrDefault(f => (f.fStandort == zeit.fStandort) && (zeitAnmeldung >= f.ZeitVon) && (zeitAnmeldung <= f.ZeitBis));
              if (wg != null)
                zeit.AnmeldungGerundetWert = zeit.Anmeldung.Value.Date.Add(wg.RundenArbeitszeitBeginn);
            }
          }

          return lZeiten;
        }
      };
      _ListeArbeitszeitenAuswahl.Parameter = new Dictionary<string, object>()
      {
        { "DatumVon", _DzArbeitszeitVon.AnzeigeDatumZeit },
        { "DatumBis", _DzArbeitszeitBis.AnzeigeDatumZeit }
      };
      _ListeArbeitszeitenAuswahl.DatenLaden();

      // Report initialisieren ********************************

      _ListeReporteArbeitszeiten = new JgEntityList<tabAuswertung>(_Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsReporteArbeitszeit"),
        OnDatenLaden = (d, p) =>
        {
          return _Db.tabAuswertungSet.Where(w => (w.FilterAuswertung == EnumFilterAuswertung.Arbeitszeit) && (!w.DatenAbgleich.Geloescht))
            .OrderBy(o => o.ReportName).ToList();
        }
      };
      _ListeReporteArbeitszeiten.DatenLaden();

      _ListeReporteAuswertung = new JgEntityList<tabAuswertung>(_ListeReporteArbeitszeiten.Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsReporteAuswertung"),
        OnDatenLaden = (d, p) =>
        {
          return _Db.tabAuswertungSet.Where(w => (w.FilterAuswertung == EnumFilterAuswertung.ArbeitszeitAuswertung) && (!w.DatenAbgleich.Geloescht))
            .OrderBy(o => o.ReportName).ToList();
        }
      };
      _ListeReporteAuswertung.DatenLaden();

      // Klasse Auswertung intitialisieren

      _Erstellung = new AnmeldungAuswertung(_Db, cbJahr, cbMonat,
        (CollectionViewSource)FindResource("vsBediener"),
        (ArbeitszeitBediener)FindResource("ArbeitszeitBediener"),
        (CollectionViewSource)FindResource("vsArbeitszeitTage"));

      // Report für Auswertung erstellen

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
      var anmeldung = (DateTime?)(az.Anmeldung ?? DateTime.Now);
      var abmeldung = az.Abmeldung;

      if (JgZeit.AbfrageZeit(msg, " Zeitabfrage !", ref anmeldung, false, ref abmeldung, true))
      {
        var sichern = false;
        if (anmeldung != az.Anmeldung)
        {
          az.AnzeigeAnmeldung = anmeldung;
          az.AnmeldungGerundetWert = null;
          if (anmeldung != null)
          {
            var zeit = JgZeit.DatumInZeitMinute(anmeldung.Value);
            var anm = anmeldung.Value;
            var azBegin = _Erstellung.Db.tabArbeitszeitRundenSet.FirstOrDefault(w =>
              (w.fStandort == az.fStandort)
              && (zeit >= w.ZeitVon) && (zeit <= w.ZeitBis)
              && (w.Jahr == anm.Year) && (w.Monat == anm.Month) && (!w.DatenAbgleich.Geloescht));
            if (azBegin != null)
              az.AnmeldungGerundetWert = anm.Date + azBegin.RundenArbeitszeitBeginn;
          }
          sichern = true;
        }

        if (abmeldung != az.Abmeldung)
        {
          az.AnzeigeAbmeldung = abmeldung;
          sichern = true;
        }

        if (sichern)
        {
          _ListeArbeitszeitenAuswahl.DsSave();
          if (az.eBediener == _Erstellung.AktuellerBediener)
            _Erstellung.BenutzerGeaendert();
        }
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      Properties.Settings.Default.Save();
    }

    private void BtnDrucken_Click(object sender, RoutedEventArgs e)
    {
      _Report.Clear();
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
            SaveFileDialog dia = new SaveFileDialog()
            {
              FileName = _AktuellerReport.ReportName,
              Filter = "Fastreport (*.frx)|*.frx|Alle Dateien (*.*)|*.*",
              FilterIndex = 1
            };
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
          var bediener = _ListeArbeitszeitenAuswahl.Daten.Select(s => s.eBediener).Distinct().ToList();
          _Report.RegisterData(bediener.Select(s => new { s.Id, s.Name }).ToList(), "Bediener");

          var dat = _DzArbeitszeitBis.AnzeigeDatumZeit;
          var db = _ListeArbeitszeitenAuswahl.Db;
          var sollStunden = JgZeit.StringInZeit(db.tabSollStundenSet.FirstOrDefault(w => (w.Jahr == dat.Year) && (w.Monat == dat.Month) && !w.DatenAbgleich.Geloescht).SollStunden, TimeSpan.Zero);
          var lRunden = db.tabArbeitszeitRundenSet.Where(w => (w.Jahr == dat.Year) && (w.Monat == dat.Month) && !w.DatenAbgleich.Geloescht).ToList();
          var lFeiertage = db.tabFeiertageSet.Where(w => (w.Datum.Year == dat.Year) && (w.Datum.Month == dat.Month) && !w.DatenAbgleich.Geloescht).ToList();
          var lPausen = db.tabPausenzeitSet.Where(w => !w.DatenAbgleich.Geloescht).ToList();

          var listeAuswertungen = new List<ArbeitszeitBediener>();

          foreach (var bedAusw in bediener)
          {
            var ds = new ArbeitszeitBediener(_Erstellung.Db);
            ds.BedienerBerechnen(bedAusw, (short)dat.Year, (byte)dat.Month, sollStunden, lRunden, lFeiertage, lPausen);
            listeAuswertungen.Add(ds);
          }
          _Report.RegisterData(listeAuswertungen, "Auswertungen");

          _Report.RegisterData(_ListeArbeitszeitenAuswahl.Daten, "Stechzeiten");
          _Report.SetParameterValue("Zeitraum.DatumVon", _DzArbeitszeitVon.AnzeigeDatumZeit);
          _Report.SetParameterValue("Zeitraum.DatumBis", _DzArbeitszeitBis.AnzeigeDatumZeit);
          break;
        case EnumFilterAuswertung.ArbeitszeitAuswertung:
          var aktStandort = _Erstellung.AktuellerBediener.eStandort;

          var bedienerImStandort = _Erstellung.ListeBediener.Daten.Where(w => w.fStandort == aktStandort.Id).ToList();
          var listeAuswertung = new List<ArbeitszeitBediener>();

          foreach (var bedAusw in bedienerImStandort)
          {
            var ds = new ArbeitszeitBediener(_Erstellung.Db);
            ds.BedienerBerechnen(bedAusw, _Erstellung.Jahr, _Erstellung.Monat, _Erstellung.SollStundenMonat, _Erstellung.ListeRundenMonat, _Erstellung.ListeFeiertageMonat, _Erstellung.ListePausen.Daten);
            listeAuswertung.Add(ds);
          }

          _Report.RegisterData(bedienerImStandort.Select(s => new { s.Id, s.Name }).ToList(), "Bediener");
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
          case 1:
            try
            {
              _Report.Show();
            }
            catch (Exception ex)
            {
              Helper.InfoBox("Fehler beim Aufruf der Auswertung", ex);
            }
            break;
          case 2:
            try
            {
              _Report.Print();
            }
            catch (Exception ex)
            {
              Helper.InfoBox("Fehler beim Drucken der Auswertung", ex);
            }
            break;
          case 3:
            try
            {
              _Report.Design();
            }
            catch { }
            break;
        }
    }

    private void BtnAuswahlAktualisieren_Click(object sender, RoutedEventArgs e)
    {
      _ListeArbeitszeitenAuswahl.Parameter = new Dictionary<string, object>()
      {
        { "DatumVon", _DzArbeitszeitVon.AnzeigeDatumZeit },
        { "DatumBis", _DzArbeitszeitBis.AnzeigeDatumZeit }
      };
      _ListeArbeitszeitenAuswahl.DatenAktualisieren();
    }

    private void BtnOptionen_Click(object sender, RoutedEventArgs e)
    {
      var formOptionen = new Fenster.FormOptionen(_Erstellung);
      formOptionen.ShowDialog();
      _Erstellung.Db.SaveChanges();
    }

    private void BtnSollStundenEinstellen_Click(object sender, RoutedEventArgs e)
    {
      var form = new FormSollstundenEinstellen(_Erstellung.AktuellerBediener.eArbeitszeitHelper.SollStunden);
      if (form.ShowDialog() ?? false)
        _Erstellung.AzBediener.SetSollstunden(form.Sollstunden);
    }

    private void BtnAuswertungErledigt_Click(object sender, RoutedEventArgs e)
    {
      var bediener = _Erstellung.AktuellerBediener;
      if (bediener?.eArbeitszeitHelper != null)
      {
        bediener.eArbeitszeitHelper.StatusAnzeige = (EnumStatusArbeitszeitAuswertung)Convert.ToByte((sender as Button).Tag);
        _Erstellung.Db.SaveChanges();
      }

      var vfs = (CollectionViewSource)FindResource("vsArbeitszeitTage");
      vfs.View.Refresh();
    }

    private string XmlZeitInString(TimeSpan Zeit, string AusgabeFormat)
    {
      try
      {
        var totalHour = (int)Zeit.TotalHours;
        var hours = totalHour.ToString("D2");
        if ((totalHour == 0) && ((Zeit.Minutes < 0) || (Zeit.Seconds < 0)))
          hours = "-" + hours;

        var min = (Zeit.Minutes < 0 ? -1 * Zeit.Minutes : Zeit.Minutes).ToString("D2");
        var sec = (Zeit.Seconds < 0 ? -1 * Zeit.Seconds : Zeit.Seconds).ToString("D2");

        return string.Format(AusgabeFormat, hours, min, sec);
      }
      catch (Exception f)
      {
        Helper.InfoBox($"Fehler bei Konvertierung von Wert {Zeit} in Format {AusgabeFormat} !", f);
      }
      return "";
    }

    private void BtnExporteren_Click(object sender, RoutedEventArgs e)
    {
      var datName = Properties.Settings.Default.NameXmlDatei;
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

        var en = new CultureInfo("en-US", false);
        var formatDezimal = Properties.Settings.Default.FormatXmlAusgabeDezimal;
        var formatZeit = Properties.Settings.Default.FormatXmlAusgabeZeit;

        XDocument xDoc = new XDocument(
          new XComment($"Arbeitszeit Monat: {_Erstellung.Monat}.{_Erstellung.Jahr} Datum: {DateTime.Now.ToString("dd.MM.yy HH:mm")}"),
          new XElement("Root",
          new XElement("Monat", $"{(JgZeit.Monate)_Erstellung.Monat} {_Erstellung.Jahr.ToString()}"),

          from z in daten
          let normalStunden = JgZeit.StringInZeit(z.eArbeitszeitHelper.SollStunden) - (new TimeSpan(8 * (z.eArbeitszeitHelper.Urlaub + z.eArbeitszeitHelper.Krank + z.eArbeitszeitHelper.Feiertage), 0, 0))
          let istStunden = JgZeit.ZeitStringAddieren(z.eArbeitszeitHelper.SollStunden, z.eArbeitszeitHelper.Ueberstunden)
          select new XElement("Datensatz",
            new XElement("Mitarbeiter", z.Name),
            new XElement("Nachname", z.NachName),
            new XElement("Vorname", z.VorName),
            new XElement("IdBuchhaltung", z.IdBuchhaltung),

            new XElement("Standort", z.eStandort.Bezeichnung),
            new XElement("Zahltag", z.AuszahlungGehalt),
            new XElement("Urlaubstage", z.Urlaubstage),

            // Formatierung als Dezimalzahl mit einer Kommastelle mit Frau Glatter besprochen

            new XElement("IstStunden", istStunden.TotalHours.ToString(formatDezimal, en)),
            new XElement("UeberStunden", JgZeit.StringInZeit(z.eArbeitszeitHelper.Ueberstunden).TotalHours.ToString(formatDezimal, en)),

            new XElement("SollStunden", JgZeit.StringInZeit(z.eArbeitszeitHelper.SollStunden).TotalHours.ToString(formatDezimal, en)),
            new XElement("Normalstunden", normalStunden.TotalHours.ToString(formatDezimal, en)),
            new XElement("UeberstundenAusgezahlt", JgZeit.StringInZeit(z.eArbeitszeitHelper.AuszahlungUeberstunden).TotalHours.ToString(formatDezimal, en)),
            new XElement("Urlaub", (z.eArbeitszeitHelper.Urlaub * 8).ToString(formatDezimal, en)),
            new XElement("Krank", (z.eArbeitszeitHelper.Krank * 8).ToString(formatDezimal, en)),
            new XElement("Feiertage", (z.eArbeitszeitHelper.Feiertage * 8).ToString(formatDezimal, en)),

            new XElement("NachtschichtZuschlag", JgZeit.StringInZeit(z.eArbeitszeitHelper.NachtschichtZuschlaege).TotalHours.ToString(formatDezimal, en)),
            new XElement("NachtschichtZuschlagGerundet", z.eArbeitszeitHelper.NachtschichtZuschlaegeGerundet.TotalHours.ToString(formatDezimal, en)),
            new XElement("FeiertagsZuschlag", JgZeit.StringInZeit(z.eArbeitszeitHelper.FeiertagZuschlaege).TotalHours.ToString(formatDezimal, en)),
            new XElement("FeiertagsZuschlagGerundet", z.eArbeitszeitHelper.FeiertagZuschlaegeGerundet.TotalHours.ToString(formatDezimal, en)),

            // Ausgabe als Zeit

            new XElement("IstStundenD", XmlZeitInString(istStunden, formatZeit)),
            new XElement("UeberStundenD", XmlZeitInString(JgZeit.StringInZeit(z.eArbeitszeitHelper.Ueberstunden), formatZeit)),

            new XElement("SollStundenD", XmlZeitInString(JgZeit.StringInZeit(z.eArbeitszeitHelper.SollStunden), formatZeit)),
            new XElement("NormalstundenD", XmlZeitInString(normalStunden, formatZeit)),
            new XElement("UeberstundenAusgezahltD", XmlZeitInString(JgZeit.StringInZeit(z.eArbeitszeitHelper.AuszahlungUeberstunden), formatZeit)),
            new XElement("UrlaubD", XmlZeitInString(new TimeSpan(z.eArbeitszeitHelper.Urlaub * 8, 0, 0), formatZeit)),
            new XElement("KrankD", XmlZeitInString(new TimeSpan(z.eArbeitszeitHelper.Krank * 8), formatZeit)),
            new XElement("FeiertageD", XmlZeitInString(new TimeSpan(z.eArbeitszeitHelper.Feiertage * 8), formatZeit)),

            new XElement("NachtschichtZuschlagD", XmlZeitInString(JgZeit.StringInZeit(z.eArbeitszeitHelper.NachtschichtZuschlaege), formatZeit)),
            new XElement("NachtschichtZuschlagGerundetD", XmlZeitInString(z.eArbeitszeitHelper.NachtschichtZuschlaegeGerundet, formatZeit)),
            new XElement("FeiertagsZuschlagD", XmlZeitInString(JgZeit.StringInZeit(z.eArbeitszeitHelper.FeiertagZuschlaege), formatZeit)),
            new XElement("FeiertagsZuschlagGerundetD", XmlZeitInString(z.eArbeitszeitHelper.FeiertagZuschlaegeGerundet, formatZeit))
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

        if (Properties.Settings.Default.NameXmlDatei != fo.FileName)
          Properties.Settings.Default.NameXmlDatei = fo.FileName;

        foreach (var bed in daten)
          bed.eArbeitszeitHelper.StatusAnzeige = EnumStatusArbeitszeitAuswertung.Erledigt;

        _Erstellung.Db.SaveChanges();

        Helper.InfoBox($"{daten.Count} Mitarbeiter in Datei gespeichert !", Helper.ProtokollArt.Info);
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
          aktDs.NachtschichtZuschlagAnzeige = JgZeit.ZeitInString(aktDs.NachtschichtBerechnet);
      }
    }

    private void NeueArbeitszeit_Click(object sender, RoutedEventArgs e)
    {
      var standorte = _Erstellung.Db.tabStandortSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.Bezeichnung).ToList();
      var form = new FormNeueArbeitszeit(standorte, _Erstellung.ListeBediener.Daten);
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

    private void BtnAutomatischeDsBerechnung_Click(object sender, RoutedEventArgs e)
    {
      _Erstellung.DatensatzAutomatischBerechnenGeaendert();
    }

    private void BtnNeuBerechnen_Click(object sender, RoutedEventArgs e)
    {
      _Erstellung.BenutzerGeaendert();
    }
  }
}
