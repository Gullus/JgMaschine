using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JgMaschineData;
using JgZeitHelper;

namespace JgMaschineGlobalZeit.Fenster
{
  public partial class FormNeueArbeitszeit : Window
  {
    private JgZeit _ZeitVon { get { return (JgZeit)FindResource("JgZeitVon"); } }
    private JgZeit _ZeitBis { get { return (JgZeit)FindResource("JgZeitBis"); } }

    public tabArbeitszeit ArbeitsZeit { get => (tabArbeitszeit)gridArbeitszeit.DataContext; }

    public FormNeueArbeitszeit(JgModelContainer Db, IEnumerable<tabBediener> ListeBediener)
    {
      InitializeComponent();

      var standorte = Db.tabStandortSet.Where(w => !w.DatenAbgleich.Geloescht).OrderBy(o => o.Bezeichnung).ToList();
      cmbStandort.ItemsSource = standorte;
      cmbMitarbeiter.ItemsSource = ListeBediener;

      var jetzt = DateTime.Now;

      var az = new tabArbeitszeit()
      {
        Id = Guid.NewGuid(),
        eStandort = standorte.FirstOrDefault(),
        eBediener = ListeBediener.FirstOrDefault(),
        Anmeldung = jetzt,
        ManuelleAnmeldung = true,
        Abmeldung = jetzt.Add(new TimeSpan(0, 32, 0)),
        ManuelleAbmeldung = true,
      };

      var zVon = (JgZeit)FindResource("JgZeitVon");
      zVon.AnzeigeDatumZeit = az.Anmeldung.Value;
      zVon.OnNeuerWert = (d, z) => { az.Anmeldung = d + z; };

      var zBis = (JgZeit)FindResource("JgZeitBis");
      zBis.AnzeigeDatumZeit = az.Abmeldung.Value;
      zBis.OnNeuerWert = (d, z) => { az.Abmeldung = d + z; };

      gridArbeitszeit.DataContext = az;
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
