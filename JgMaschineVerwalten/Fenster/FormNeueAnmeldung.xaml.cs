using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JgMaschineData;
using JgZeitHelper;

namespace JgMaschineVerwalten.Fenster
{
  /// <summary>
  /// Interaktionslogik für FormNeueAnmeldung.xaml
  /// </summary>
  public partial class FormNeueAnmeldung : Window
  {
    public tabAnmeldungMaschine AnmeldungMaschine
    {
      get => (tabAnmeldungMaschine)gridAnmeldung.DataContext;
    }

    public FormNeueAnmeldung(IEnumerable<tabBediener> Bediener, IEnumerable<tabMaschine> Maschinen, tabMaschine AktuelleMaschine)
    {
      InitializeComponent();

      cmbMaschine.ItemsSource = Maschinen;
      var bediener = Bediener.Where(w => w.eAktivAnmeldung == null).OrderBy(o => o.Name).ToList();
      cmbBediener.ItemsSource = bediener;

      var anmeldung = new tabAnmeldungMaschine()
      {
        Anmeldung = DateTime.Now,
        ManuelleAnmeldung = true,
        eMaschine = AktuelleMaschine ?? Maschinen.FirstOrDefault(),
        eBediener = bediener.FirstOrDefault()
      };
      gridAnmeldung.DataContext = anmeldung;

      var dz = (JgZeit)this.FindResource("zeitAnmeldung");
      dz.AnzeigeDatumZeit = anmeldung.Anmeldung;
      dz.OnNeuerWert = (datum, zeit) => anmeldung.Anmeldung = datum + zeit;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
