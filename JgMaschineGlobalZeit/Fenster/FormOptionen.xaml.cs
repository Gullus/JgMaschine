using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JgGlobalZeit.Fenster;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineGlobalZeit.Fenster
{
  public partial class FormOptionen : Window
  {
    private AnmeldungAuswertung _Auswertung;
    private JgEntityList<tabArbeitszeitTerminal> _ListeTerminals;

    private JgEntityList<tabArbeitszeitRunden> _ListeRundenBeginn;
    private JgEntityList<tabArbeitszeitRunden> _ListeRundenEnde;

    public FormOptionen(AnmeldungAuswertung Auswertung, ArbeitszeitRunden AzRunden)
    {
      InitializeComponent();

      _Auswertung = Auswertung;
      tbJahr.Text = Auswertung.Jahr.ToString();

      // Sollstunden für jedes Jahr überprüfen
      for (byte i = 1; i <= 12; i++)
      {
        if (_Auswertung.ListeSollstundenJahr.Daten.FirstOrDefault(f => f.Monat == i) == null)
        {
          var neuSoll = new tabSollStunden()
          {
            Id = Guid.NewGuid(),
            Jahr = Auswertung.Jahr,
            Monat = i,
          };
          _Auswertung.Db.tabSollStundenSet.Add(neuSoll);
          _Auswertung.ListeSollstundenJahr.Add(neuSoll);
        };
      }

      // Kontrolle, ob alle Bediener die Auswertung für Monat 0 besitzen, da hier die Vorjahresdaten eingetragen werden.
      var idisBediener = Auswertung.ListeBediener.Daten.Select(s => s.Id).ToArray();
      var auswertungenVorjahr = Auswertung.Db.tabArbeitszeitAuswertungSet.Where(w => (idisBediener.Contains(w.fBediener)) && (w.Jahr == Auswertung.Jahr) && (w.Monat == 0)).ToList();
      foreach (var bediener in _Auswertung.ListeBediener.Daten)
      {
        var auswVorjahr = auswertungenVorjahr.FirstOrDefault(f => f.fBediener == bediener.Id);
        if (auswVorjahr == null)
        {
          auswVorjahr = new tabArbeitszeitAuswertung()
          {
            Id = Guid.NewGuid(),
            Jahr = Auswertung.Jahr,
            Monat = 0,
            Ueberstunden = "00:00",
            Urlaub = 0,

            eBediener = bediener,
            Status = EnumStatusArbeitszeitAuswertung.Erledigt,
          };
          Auswertung.Db.tabArbeitszeitAuswertungSet.Add(auswVorjahr);
        }
        bediener.EArbeitszeitHelper = auswVorjahr;
      }

      _ListeTerminals = new JgEntityList<tabArbeitszeitTerminal>(_Auswertung.Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsTerminals"),
        Tabellen = new DataGrid[] { gridTerminals },
        Daten = _Auswertung.Db.tabArbeitszeitTerminalSet.ToList()
      };
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var vsBediener = (CollectionViewSource)(this.FindResource("vsBediener"));
      vsBediener.GroupDescriptions.Add(new PropertyGroupDescription("eStandort.Bezeichnung"));
      vsBediener.Source = _Auswertung.ListeBediener.Daten;

      _Auswertung.ListeFeiertageJahr.ViewSource = (CollectionViewSource)(this.FindResource("vsFeiertage"));
      _Auswertung.ListeSollstundenJahr.ViewSource = (CollectionViewSource)(this.FindResource("vsSollStunden"));
      _Auswertung.ListePausen.ViewSource = (CollectionViewSource)(this.FindResource("vsPausen"));

      _ListeRundenBeginn = new JgEntityList<tabArbeitszeitRunden>(_Auswertung.Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsRundenAzBeginn"),
        Daten = _Auswertung.AzRunden.ListeRunden.Where(w => w.Zeitpunkt == EnumZeitpunkt.Anmeldung).OrderBy(o => o.Monat).ThenBy(o => o.ZeitVon).ToList()
      };
      _ListeRundenBeginn.ViewSource.GroupDescriptions.Add(new PropertyGroupDescription("eStandort.Bezeichnung"));

      _ListeRundenEnde = new JgEntityList<tabArbeitszeitRunden>(_Auswertung.Db)
      {
        ViewSource = (CollectionViewSource)FindResource("vsRundenAzEnde"),
        Daten = _Auswertung.AzRunden.ListeRunden.Where(w => w.Zeitpunkt == EnumZeitpunkt.Abmeldung).OrderBy(o => o.Monat).ThenBy(o => o.ZeitVon).ToList()
      };

      _ListeRundenEnde.ViewSource.GroupDescriptions.Add(new PropertyGroupDescription("eStandort.Bezeichnung"));
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private void BtnNeuerFeiertag_Click(object sender, RoutedEventArgs e)
    {
      var feiertag = new tabFeiertage()
      {
        Id = Guid.NewGuid(),
        Datum = DateTime.Now.Date
      };
      _Auswertung.Db.tabFeiertageSet.Add(feiertag);
      _Auswertung.ListeFeiertageJahr.Add(feiertag);
    }

    private void BtnNeuePause_Click(object sender, RoutedEventArgs e)
    {
      var pause = new tabPausenzeit()
      {
        Id = Guid.NewGuid(),
        ZeitVon = new TimeSpan(5, 0, 0),
        ZeitBis = new TimeSpan(13, 0, 0),
        Pausenzeit = new TimeSpan(1, 0, 0)
      };
      _Auswertung.Db.tabPausenzeitSet.Add(pause);
      _Auswertung.ListePausen.Add(pause);
    }

    private void BtnRundungswert_Click(object sender, RoutedEventArgs e)
    {
      var neuerDatensatz = (sender == BtnAzRundenBeginnNeu) || (sender == BtnAzRundenEndeNeu);
      var zeitPunkt = EnumZeitpunkt.Abmeldung;
      if ((sender == BtnAzRundenBeginnNeu) || (sender == BtnAzRundenBeginnBearbeiten))
        zeitPunkt = EnumZeitpunkt.Anmeldung;
      var aktiv = (zeitPunkt == EnumZeitpunkt.Anmeldung) ? _ListeRundenBeginn.Current : _ListeRundenEnde.Current;

      var fo = new FormArbeitszeitRunden(_Auswertung, zeitPunkt, aktiv, neuerDatensatz);
      if (zeitPunkt == EnumZeitpunkt.Anmeldung)
        _ListeRundenBeginn.ErgebnissFormular(fo.ShowDialog(), neuerDatensatz, fo.AzGerundet);
      else
        _ListeRundenEnde.ErgebnissFormular(fo.ShowDialog(), neuerDatensatz, fo.AzGerundet);
    }

    private void BtnTerminal_Click(object sender, RoutedEventArgs e)
    {
      var standorte = _Auswertung.Db.tabStandortSet.OrderBy(o => o.Bezeichnung).ToList();
      var fo = new FormTerminal(standorte, (sender == btnNeuesTermin) ? null : _ListeTerminals.Current);
      _ListeTerminals.ErgebnissFormular(fo.ShowDialog(), fo.IstNeu, fo.Terminal);
    }
  }
}
