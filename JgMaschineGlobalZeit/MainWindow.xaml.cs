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
using JgMaschineLib.Zeit;
using Microsoft.Win32;
using JgZeitHelper;

namespace JgMaschineGlobalZeit
{
  public partial class MainWindow : Window
  {
    private JgEntityView<tabArbeitszeit> _ListeArbeitszeitAuswahl;
    private JgZeit _DzArbeitszeitVon { get { return (JgZeit)FindResource("dzArbeitszeitVon"); } }
    private JgZeit _DzArbeitszeitBis { get { return (JgZeit)FindResource("dzArbeitszeitBis"); } }

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

      Helper.FensterEinstellung(this, JgGlobalZeit.Properties.Settings.Default);

      CommandBindings.Add(new CommandBinding(MyCommands.ArbeitszeitLoeschen, (sen, erg) =>
      {
        var az = _ListeArbeitszeitAuswahl.Current;
        var msg = $"Arbeitszeit von {az.eBediener.Name} löschen ?";
        var ergBox = MessageBox.Show(msg, "Löschabfrage", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);  
        if (ergBox == MessageBoxResult.Yes)
        {
          _ListeArbeitszeitAuswahl.AlsGeloeschtKennzeichnen(az);
          _ListeArbeitszeitAuswahl.Remove(az);
        }
      },
      (sen, erg) =>
      {
        erg.CanExecute = _ListeArbeitszeitAuswahl.Current != null;
      }));
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var heute = DateTime.Now.Date;
      _DzArbeitszeitVon.AnzeigeDatumZeit = heute;
      _DzArbeitszeitBis.AnzeigeDatumZeit = new DateTime(heute.Year, heute.Month, heute.Day, 23, 59, 59);

      _ListeArbeitszeitAuswahl = new JgEntityView<tabArbeitszeit>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsArbeitszeitAuswahl"),
        Tabellen = new DataGrid[] { dgArbeitszeitAuswahl },
        DatenErstellen = (dbIntern) =>
        {
          var tabZeiten = dbIntern.tabArbeitszeitSet.Where(w => (((w.Anmeldung >= _DzArbeitszeitVon.AnzeigeDatumZeit) && (w.Anmeldung <= _DzArbeitszeitBis.AnzeigeDatumZeit)) 
            || ((w.Anmeldung == null) && (w.Abmeldung >= _DzArbeitszeitVon.AnzeigeDatumZeit) && (w.Abmeldung <= _DzArbeitszeitBis.AnzeigeDatumZeit))))
            .OrderBy(o => o.Anmeldung).ToList();

          var listeGerundet = dbIntern.tabArbeitszeitRundenSet.Where(w => !w.DatenAbgleich.Geloescht).ToList();
          foreach(var zeit in tabZeiten)
          {
            if (zeit.Anmeldung != null)
            {
              var zeitAnmeldung = new TimeSpan(zeit.Anmeldung.Value.Hour, zeit.Anmeldung.Value.Minute, 0);
              var wg = listeGerundet.FirstOrDefault(f => (zeitAnmeldung >= f.ZeitVon) && (zeitAnmeldung <= f.ZeitBis) && (f.fStandort == zeit.fStandort));
              if (wg != null)
                zeit.AnmeldungGerundetWert = zeit.Anmeldung.Value.Date.Add(wg.RundenAufZeit);
            }
          }

          return tabZeiten;
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

      _Erstellung = new AnmeldungAuswertung(new JgModelContainer(), cbJahr, cbMonat,
        (CollectionViewSource)FindResource("vsBediener"),
        (ArbeitszeitSummen)FindResource("AuswertungKumulativ"), (ArbeitszeitSummen)FindResource("AuswertungGesamt"),
        (CollectionViewSource)FindResource("vsArbeitszeitTage"));

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

          _Report.SetParameterValue("Zeitraum.DatumVon", _DzArbeitszeitVon.AnzeigeDatumZeit);
          _Report.SetParameterValue("Zeitraum.DatumBis", _DzArbeitszeitBis.AnzeigeDatumZeit);
          _Report.SetParameterValue("IstAktuell", tcArbeitszeit.SelectedIndex == 0);
          break;
        case EnumFilterAuswertung.ArbeitszeitAuswertung:
          var aktStandort = _Erstellung.AktuellerBediener.eStandort;

          var bedienerStandort = _Erstellung.ListeBediener.Where(w => w.fStandort == aktStandort.Id).ToList();
          var listeAuswertung = new List<ArbeitszeitBediener>();

          foreach (var bedAusw in bedienerStandort)
          {
            var ds = new ArbeitszeitBediener(_Erstellung.Db)
            {
              AuswertungKumulativ = new ArbeitszeitSummen(),
              AuswertungGesamt = new ArbeitszeitSummen(),
              ListeTage = new ObservableCollection<tabArbeitszeitTag>()
            };
            ds.BedienerBerechnen(bedAusw, _Erstellung.Jahr, _Erstellung.Monat, _Erstellung.SollStundenMonat, _Erstellung.ListeRundenMonat, _Erstellung.ListeFeiertageMonat, _Erstellung.ListePausen);
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
      var form = new JgGlobalZeit.Fenster.FormSollstundenEinstellen(_Erstellung.AktuellerBediener.eArbeitszeitHelper.SollStunden);
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
        DbSichern.AbgleichEintragen(bediener.eArbeitszeitHelper.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
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
        var daten = _Erstellung.ListeBediener.Where(w => (w.eArbeitszeitHelper != null) && (w.eArbeitszeitHelper.Status == EnumStatusArbeitszeitAuswertung.Fertig)).ToList();


        var anzahlFeiertage =  _Erstellung.ListeFeiertageMonat.Count();
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
        {
          bed.eArbeitszeitHelper.StatusAnzeige = EnumStatusArbeitszeitAuswertung.Erledigt;
          DbSichern.AbgleichEintragen(bed.eArbeitszeitHelper.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
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
  }
}
