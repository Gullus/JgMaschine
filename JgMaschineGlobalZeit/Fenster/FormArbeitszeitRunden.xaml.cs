using System;
using System.Linq;
using System.Windows;
using JgMaschineData;
using JgZeitHelper;

namespace JgMaschineGlobalZeit.Fenster
{
  public partial class FormArbeitszeitRunden : Window
  {
    public tabArbeitszeitRunden AzGerundet { get => (tabArbeitszeitRunden)gridRunden.DataContext; }

    public FormArbeitszeitRunden(AnmeldungAuswertung Auswertung, EnumZeitpunkt NeuZeitpunkt, tabArbeitszeitRunden WertBearbeiten, bool Erstellen)
    {
      InitializeComponent();

      this.Title = $" {(Erstellen ? "Neu" : "Bearbeiten")} {(NeuZeitpunkt == EnumZeitpunkt.Anmeldung ? "Arbeitsbeginn" : "Arbeitsende")} ";

      cmbNiederlassung.ItemsSource = Auswertung.Db.tabStandortSet.Where(w => (!w.DatenAbgleich.Geloescht)).OrderBy(o => o.Bezeichnung).ToList();
      cmbMonat.ItemsSource = Enum.GetValues(typeof(JgZeit.Monate));

      var runden = WertBearbeiten;
      if (Erstellen)
      {
        runden = new tabArbeitszeitRunden()
        {
          Id = Guid.NewGuid(),
          Zeitpunkt = NeuZeitpunkt,
          Jahr = Auswertung.Jahr,
          Monat = WertBearbeiten?.Monat ?? (byte)1,

          ZeitVon = (WertBearbeiten != null) ? WertBearbeiten.ZeitVon : new TimeSpan(5, 30, 0),
          ZeitBis = (WertBearbeiten != null) ? WertBearbeiten.ZeitBis : new TimeSpan(6, 0, 0),
          RundenArbeitszeit = WertBearbeiten?.RundenArbeitszeit ?? new TimeSpan(6, 0, 0),

          eStandort = WertBearbeiten?.eStandort,
        };
      }
      gridRunden.DataContext = runden;
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
