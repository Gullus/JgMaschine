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
        private ArbeitszeitRunden _AzRunden;

        public MainWindow()
        {
            InitializeComponent();

            Helper.FensterEinstellung(this, Properties.Settings.Default);

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
                Erg.CanExecute = _Erstellung?.AktuellerBediener?.EArbeitszeitHelper?.Status == EnumStatusArbeitszeitAuswertung.InArbeit || false;
            }

            CommandBindings.Add(new CommandBinding(MyCommands.SollStundenAendern, (sen, erg) =>
            {
                var form = new FormSollstundenEinstellen(_Erstellung.AktuellerBediener.EArbeitszeitHelper.SollStunden);
                if (form.ShowDialog() ?? false)
                {
                    _Erstellung.AzBediener.SetSollstunden(form.Sollstunden);
                    _Erstellung.BenutzerGeaendert();
                }
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
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            this.Title += " - V " + version.ToString();

            _Db = new JgModelContainer();
            if (Properties.Settings.Default.DatenbankVerbindungsString != "")
                _Db.Database.Connection.ConnectionString = Properties.Settings.Default.DatenbankVerbindungsString;

            var heute = DateTime.Now.Date;

            _AzRunden = new ArbeitszeitRunden(_Db, heute.Year);

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
                    var azRunden = (ArbeitszeitRunden)p.Params["AzRunden"];

                    var lZeiten = d.tabArbeitszeitSet.Where(w => (((w.Anmeldung >= datVom) && (w.Anmeldung <= datBis))
              || ((w.Anmeldung == null) && (w.Abmeldung >= datVom) && (w.Abmeldung <= datBis))));

                    if (!p.IstSortiert)
                        lZeiten = lZeiten.OrderBy(o => o.Anmeldung);

                    foreach (var zeit in lZeiten.ToList())
                    {
                        zeit.AnmeldungGerundet = azRunden.GetZeitGerundet(EnumZeitpunkt.Anmeldung, zeit.fStandort, zeit.Anmeldung);
                        zeit.AbmeldungGerundet = azRunden.GetZeitGerundet(EnumZeitpunkt.Abmeldung, zeit.fStandort, zeit.Abmeldung);
                    }

                    return lZeiten;
                }
            };
            _ListeArbeitszeitenAuswahl.Parameter["AzRunden"] = _AzRunden;
            _ListeArbeitszeitenAuswahl.Parameter["DatumVon"] = _DzArbeitszeitVon.AnzeigeDatumZeit;
            _ListeArbeitszeitenAuswahl.Parameter["DatumBis"] = _DzArbeitszeitBis.AnzeigeDatumZeit;
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
              _AzRunden,
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
                    az.AnzeigeAnmeldungGerundet = _AzRunden.GetZeitGerundet(EnumZeitpunkt.Anmeldung, az.fStandort, anmeldung);
                    sichern = true;
                }

                if (abmeldung != az.Abmeldung)
                {
                    az.AnzeigeAbmeldung = abmeldung;
                    az.AnzeigeAbmeldungGerundet = _AzRunden.GetZeitGerundet(EnumZeitpunkt.Abmeldung, az.fStandort, abmeldung);
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
                    var lFeiertage = db.tabFeiertageSet.Where(w => (w.Datum.Year == dat.Year) && (w.Datum.Month == dat.Month) && !w.DatenAbgleich.Geloescht).ToList();
                    var lPausen = db.tabPausenzeitSet.Where(w => !w.DatenAbgleich.Geloescht).ToList();

                    var listeAuswertungen = new List<ArbeitszeitBediener>();

                    foreach (var bedAusw in bediener)
                    {
                        var ds = new ArbeitszeitBediener(_Erstellung.Db, _AzRunden);
                        ds.BedienerBerechnen(bedAusw, (short)dat.Year, (byte)dat.Month, sollStunden, lFeiertage, lPausen);
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
                        var ds = new ArbeitszeitBediener(_Erstellung.Db, _AzRunden);
                        ds.BedienerBerechnen(bedAusw, _Erstellung.Jahr, _Erstellung.Monat, _Erstellung.SollStundenMonat, _Erstellung.ListeFeiertageMonat, _Erstellung.ListePausen.Daten);
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
                    case EnumFilterAuswertung.ArbeitszeitAuswertung: _ListeReporteAuswertung.Add(_AktuellerReport); break;
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
            _ListeArbeitszeitenAuswahl.Parameter["DatumVon"] = _DzArbeitszeitVon.AnzeigeDatumZeit;
            _ListeArbeitszeitenAuswahl.Parameter["DatumBis"] = _DzArbeitszeitBis.AnzeigeDatumZeit;
            _ListeArbeitszeitenAuswahl.DatenAktualisieren();
        }

        private void BtnOptionen_Click(object sender, RoutedEventArgs e)
        {
            var formOptionen = new Fenster.FormOptionen(_Erstellung, _AzRunden);
            formOptionen.ShowDialog();
            _Erstellung.Db.SaveChanges();
        }

        private void BtnSollStundenEinstellen_Click(object sender, RoutedEventArgs e)
        {
            var form = new FormSollstundenEinstellen(_Erstellung.AktuellerBediener.EArbeitszeitHelper.SollStunden);
            if (form.ShowDialog() ?? false)
                _Erstellung.AzBediener.SetSollstunden(form.Sollstunden);
        }

        private void BtnAuswertungErledigt_Click(object sender, RoutedEventArgs e)
        {
            var bediener = _Erstellung.AktuellerBediener;
            if (bediener?.EArbeitszeitHelper != null)
            {
                bediener.EArbeitszeitHelper.StatusAnzeige = (EnumStatusArbeitszeitAuswertung)Convert.ToByte((sender as Button).Tag);
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
                var lBediener = _Erstellung.ListeBediener.Daten.Where(w => (w.EArbeitszeitHelper != null) && (w.EArbeitszeitHelper.Status == EnumStatusArbeitszeitAuswertung.Fertig)).ToList();

                var listeAuswertungen = new List<ArbeitszeitBediener>();
                foreach (var bedAusw in lBediener)
                {
                    var ds = new ArbeitszeitBediener(_Db, _AzRunden);
                    ds.BedienerBerechnen(bedAusw, _Erstellung.Jahr, _Erstellung.Monat, _Erstellung.SollStundenMonat, _Erstellung.ListeFeiertageMonat, _Erstellung.ListePausen.Daten);
                    listeAuswertungen.Add(ds);
                };

                var en = new CultureInfo("en-US", false);
                var formatDezimal = Properties.Settings.Default.FormatXmlAusgabeDezimal;
                var formatZeit = Properties.Settings.Default.FormatXmlAusgabeZeit;

                XDocument xDoc = new XDocument(
                  new XComment($"Arbeitszeit Monat: {_Erstellung.Monat}.{_Erstellung.Jahr} Datum: {DateTime.Now.ToString("dd.MM.yy HH:mm")}"),
                  new XElement("Root",
                  new XElement("Monat", $"{(JgZeit.Monate)_Erstellung.Monat} {_Erstellung.Jahr.ToString()}"),

                  from z in listeAuswertungen
                  let azMonat = z.AuswertungMonat.AzAuswertung
                  let normalStunden = JgZeit.StringInZeit(azMonat.SollStunden) - (new TimeSpan(8 * (azMonat.Urlaub + azMonat.Krank + azMonat.Feiertage), 0, 0))
                  let istStunden = JgZeit.ZeitStringAddieren(azMonat.SollStunden, azMonat.Ueberstunden)
                  select new XElement("Datensatz",
                    new XElement("Mitarbeiter", z.Bediener.Name),
                    new XElement("Nachname", z.Bediener.NachName),
                    new XElement("Vorname", z.Bediener.VorName),
                    new XElement("IdBuchhaltung", z.Bediener.IdBuchhaltung),

                    new XElement("Standort", z.Bediener.eStandort.Bezeichnung),
                    new XElement("Zahltag", z.Bediener.AuszahlungGehalt),
                    new XElement("Urlaubstage", z.Bediener.Urlaubstage),

                    // Formatierung als Dezimalzahl mit einer Kommastelle mit Frau Glatter besprochen

                    new XElement("IstStunden", istStunden.TotalHours.ToString(formatDezimal, en)),
                    new XElement("Normalstunden", normalStunden.TotalHours.ToString(formatDezimal, en)),
                    new XElement("SollStunden", JgZeit.StringInZeit(azMonat.SollStunden).TotalHours.ToString(formatDezimal, en)),

                    new XElement("UeberStunden", JgZeit.StringInZeit(azMonat.Ueberstunden).TotalHours.ToString(formatDezimal, en)),
                    new XElement("UeberstundenAusgezahlt", JgZeit.StringInZeit(azMonat.AuszahlungUeberstunden).TotalHours.ToString(formatDezimal, en)),
                    new XElement("UeberstundenGesamt", z.AuswertungGesamt.fUeberstunden.TotalHours.ToString(formatDezimal, en)),

                    new XElement("Urlaub", (azMonat.Urlaub * 8).ToString(formatDezimal, en)),
                    new XElement("UrlaubstageOffen", z.AuswertungMonat.UrlaubOffenAnzeige),

                    new XElement("Krank", (azMonat.Krank * 8).ToString(formatDezimal, en)),
                    new XElement("Feiertage", (azMonat.Feiertage * 8).ToString(formatDezimal, en)),

                    new XElement("NachtschichtZuschlag", JgZeit.StringInZeit(azMonat.NachtschichtZuschlaege).TotalHours.ToString(formatDezimal, en)),
                    new XElement("NachtschichtZuschlagGerundet", azMonat.NachtschichtZuschlaegeGerundet.TotalHours.ToString(formatDezimal, en)),
                    new XElement("FeiertagsZuschlag", JgZeit.StringInZeit(azMonat.FeiertagZuschlaege).TotalHours.ToString(formatDezimal, en)),
                    new XElement("FeiertagsZuschlagGerundet", azMonat.FeiertagZuschlaegeGerundet.TotalHours.ToString(formatDezimal, en)),


                    // Ausgabe als Zeit

                    new XElement("IstStundenD", XmlZeitInString(istStunden, formatZeit)),
                    new XElement("NormalstundenD", XmlZeitInString(normalStunden, formatZeit)),
                    new XElement("SollStundenD", XmlZeitInString(JgZeit.StringInZeit(azMonat.SollStunden), formatZeit)),

                    new XElement("UeberStundenD", XmlZeitInString(JgZeit.StringInZeit(azMonat.Ueberstunden), formatZeit)),
                    new XElement("UeberstundenAusgezahltD", XmlZeitInString(JgZeit.StringInZeit(azMonat.AuszahlungUeberstunden), formatZeit)),
                    new XElement("UeberstundenGesamtD", XmlZeitInString(z.AuswertungGesamt.fUeberstunden, formatZeit)),


                    new XElement("UrlaubD", XmlZeitInString(new TimeSpan(azMonat.Urlaub * 8, 0, 0), formatZeit)),
                    new XElement("UrlaubstageOffenD", XmlZeitInString(new TimeSpan(z.AuswertungMonat.UrlaubOffenAnzeige * 8, 0, 0), formatZeit)),

                    new XElement("KrankD", XmlZeitInString(new TimeSpan(azMonat.Krank * 8), formatZeit)),
                    new XElement("FeiertageD", XmlZeitInString(new TimeSpan(azMonat.Feiertage * 8), formatZeit)),

                    new XElement("NachtschichtZuschlagD", XmlZeitInString(JgZeit.StringInZeit(azMonat.NachtschichtZuschlaege), formatZeit)),
                    new XElement("NachtschichtZuschlagGerundetD", XmlZeitInString(azMonat.NachtschichtZuschlaegeGerundet, formatZeit)),
                    new XElement("FeiertagsZuschlagD", XmlZeitInString(JgZeit.StringInZeit(azMonat.FeiertagZuschlaege), formatZeit)),
                    new XElement("FeiertagsZuschlagGerundetD", XmlZeitInString(azMonat.FeiertagZuschlaegeGerundet, formatZeit))
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

                foreach (var bed in lBediener)
                    bed.EArbeitszeitHelper.StatusAnzeige = EnumStatusArbeitszeitAuswertung.Erledigt;

                _Erstellung.Db.SaveChanges();

                Helper.InfoBox($"{lBediener.Count} Mitarbeiter in Datei gespeichert !", Helper.ProtokollArt.Info);
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
            var form = new FormNeueArbeitszeit(_Db, _Erstellung.ListeBediener.Daten);
            if (_ListeArbeitszeitenAuswahl.ErgebnissFormular(form.ShowDialog(), true, form.ArbeitsZeit))
            {
                form.ArbeitsZeit.AnzeigeAnmeldungGerundet = _AzRunden.GetZeitGerundet(EnumZeitpunkt.Anmeldung, form.ArbeitsZeit.fStandort, form.ArbeitsZeit.Anmeldung);
                form.ArbeitsZeit.AnzeigeAbmeldungGerundet = _AzRunden.GetZeitGerundet(EnumZeitpunkt.Abmeldung, form.ArbeitsZeit.fStandort, form.ArbeitsZeit.Abmeldung);
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
