using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineLib.Zeit;
using System.Data.Entity;

namespace JgGlobalZeit
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

    private JgEntityView<tabBediener> _ListeBediener;

    private int AnzeigeJahr { get { return (int)cbJahr.SelectedItem; } }
    private int AnzeigeMonat { get { return cbMonat.SelectedIndex + 1; } }

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var db = new JgModelContainer();
      var heute = DateTime.Now.Date;

      var jahrStart = heute.Year - 10;
      for (var i = jahrStart; i < jahrStart + 20; i++)
        cbJahr.Items.Add(i);
      cbJahr.SelectedItem = DateTime.Now.Year;

      for (int i = 1; i < 13; i++)
        cbMonat.Items.Add((new DateTime(2000, i, 1)).ToString("MMMM"));
      cbMonat.SelectedItem = DateTime.Now.AddMonths(-1).ToString("MMMM");

      _ListeBediener = new JgEntityView<tabBediener>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsBediener"),
        Tabellen = new DataGrid[] { dgBediener },
        DatenErstellen = (dbIntern) =>
        {
          var daten = dbIntern.tabBedienerSet.Where(w => (w.Status == EnumStatusBediener.Aktiv)).OrderBy(o => o.NachName).ToList();
          foreach (var b in daten)
            b.DelegateStatusArbeitszeit = StatusBedienerEintragen;

          return daten;
        }
      };
      _ListeBediener.DatenAktualisieren(); ;
      _ListeBediener.ViewSource.View.CurrentChanged += (sen, erg) =>
      {
        BenutzerArbeitszeitAnzeigen();
      };

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
      _ListeAuswertung.Daten = await db.tabAuswertungSet.Where(w => (w.FilterAuswertung != EnumFilterAuswertung.Arbeitszeit))
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
          System.Windows.MessageBox.Show("Fehler beim speichern des Reports !\r\nGrund: " + f.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
          memStr.Dispose();
        }
      };
    }

    private EnumStatusArbeitszeitAuswertung StatusBedienerEintragen(tabBediener Bediener)
    {
      var az = Bediener.sArbeitszeitAuswertung.FirstOrDefault(w => (w.Jahr == AnzeigeJahr) && (w.Monat == AnzeigeMonat));
      if (az != null)
        return az.Status;
      return EnumStatusArbeitszeitAuswertung.Leer;
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

    private void btnAuswertungErstellen_Click(object sender, RoutedEventArgs e)
    {
      var db = _ListeBediener.Db;
      var bediener = _ListeBediener.Current;

      var ersterDat = new DateTime(AnzeigeJahr, AnzeigeMonat, 1);

      // Kontrolle ob Vormat vorhanden. Wenn nicht wird einer mit Nullwerten erstellt.
      var tempDat = ersterDat.AddMonths(-1);
      var vorAwMonat = bediener.sArbeitszeitAuswertung.FirstOrDefault(w => (w.Jahr == tempDat.Year) && (w.Monat == tempDat.Month));
      if (vorAwMonat == null)
      {
        vorAwMonat = new tabArbeitszeitAuswertung()
        {
          Id = Guid.NewGuid(),
          eBediener = bediener,
          Jahr = (short)tempDat.Year,
          Monat = (byte)tempDat.Month,

          Urlaub = 0,
          IstStunden = TimeSpan.Zero,
          SollStunden = TimeSpan.Zero,
          Ueberstunden = TimeSpan.Zero,
          AuszahlungUeberstunden = TimeSpan.Zero,
          Status = EnumStatusArbeitszeitAuswertung.Berechnet,
        };
        DbSichern.AbgleichEintragen(vorAwMonat.DatenAbgleich, EnumStatusDatenabgleich.Neu);
        db.tabArbeitszeitAuswertungSet.Add(vorAwMonat);
      }

      var awMonat = new tabArbeitszeitAuswertung()
      {
        Id = Guid.NewGuid(),
        eBediener = bediener,
        Jahr = (short)AnzeigeJahr,
        Monat = (byte)AnzeigeMonat,

        Urlaub = 0,
        IstStunden = TimeSpan.Zero,
        SollStunden = TimeSpan.Zero,
        Ueberstunden = TimeSpan.Zero,
        AuszahlungUeberstunden = TimeSpan.Zero,
        Status = EnumStatusArbeitszeitAuswertung.Berechnet,
      };
      DbSichern.AbgleichEintragen(awMonat.DatenAbgleich, EnumStatusDatenabgleich.Neu);
      db.tabArbeitszeitAuswertungSet.Add(awMonat);

      var anzTage = DateTime.DaysInMonth(AnzeigeJahr, AnzeigeMonat);

      tempDat = ersterDat.AddMonths(1);
      var alleZeiten = db.tabArbeitszeitSet.Where(w => (w.fBediener == bediener.Id) && (w.Abmeldung != null) && (w.Anmeldung >= ersterDat) && (w.Anmeldung < tempDat)).ToList();

      for (byte tag = 1; tag <= anzTage; tag++)
      {
        tempDat = new DateTime(AnzeigeJahr, AnzeigeMonat, tag);
        var zeitenPerson = alleZeiten.Where(w => w.Anmeldung.Day == tag).ToList();

        var awTag = new tabArbeitszeitTag()
        {
          Id = Guid.NewGuid(),
          fArbeitszeitAuswertung = awMonat.Id,
          Tag = tag,
          Pause = new TimeSpan(1, 0, 0),
        };

        long arbeitszeit = 0;
        long nachtschicht = 0;

        foreach (var zeitPerson in zeitenPerson)
        {
          zeitPerson.fArbeitszeitAuswertung = awTag.Id;
          arbeitszeit += ((DateTime)zeitPerson.Abmeldung - zeitPerson.Anmeldung).Ticks;
          nachtschicht += Helper.NachtZeitBerechnen(22, 0, 8, 0, zeitPerson.Anmeldung, (DateTime)zeitPerson.Abmeldung);
        };
        awTag.Zeit = new TimeSpan(arbeitszeit);
        awTag.ZeitKorrektur = awTag.Zeit;
        DbSichern.AbgleichEintragen(awTag.DatenAbgleich, EnumStatusDatenabgleich.Neu);

        db.tabArbeitszeitTagSet.Add(awTag);
      }
      db.SaveChanges();
    }

    private void cbJahr_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if ((sender as ComboBox).IsKeyboardFocusWithin)
        BenutzerArbeitszeitAnzeigen();
    }

    private void BenutzerArbeitszeitAnzeigen()
    {
      var db = _ListeBediener.Db;
      var bediener = _ListeBediener.Current;

      var dsAuswertungVormonat = db.tabArbeitszeitAuswertungSet.FirstOrDefault();
      var dsAuswertungAktuell = db.tabArbeitszeitAuswertungSet.FirstOrDefault(f => (f.fBediener == bediener.Id) && (f.Jahr == AnzeigeJahr) && (f.Monat == AnzeigeMonat));

      MessageBox.Show(dsAuswertungVormonat.Urlaub.ToString());

      var auswVormonat = (tabArbeitszeitAuswertung)FindResource("vsArbeitszeitAuswertungVormonat");
      auswVormonat = dsAuswertungVormonat;
      var auswAktuell = (tabArbeitszeitAuswertung)FindResource("vsArbeitszeitAuswertungAktuell");
      auswAktuell = dsAuswertungAktuell;
    }

    private void button_Click(object sender, RoutedEventArgs e)
    {
      //var db = _ListeBediener.Db;
      //var r = (AnzeigeAuswertungErgebniss)FindResource("dsAnzeigeVormonat");

      //var dsAuswertungVormonat = await db.tabArbeitszeitAuswertungSet.FirstOrDefaultAsync();
      //r.UrlaubKumulativ = dsAuswertungVormonat.Urlaub;

      //var auswVormonat = (tabArbeitszeitAuswertung)FindResource("vsArbeitszeitAuswertungVormonat");
      //auswVormonat = dsAuswertungVormonat;
    }

    private async Task<int> Test()
    {
      var db = _ListeBediener.Db;
      var r = (AnzeigeAuswertungErgebniss)FindResource("dsAnzeigeVormonat");

      var dsAuswertungVormonat = await db.tabArbeitszeitAuswertungSet.FirstOrDefaultAsync();
      r.UrlaubKumulativ = dsAuswertungVormonat.Urlaub;
      return 0;
    }
  }
}
