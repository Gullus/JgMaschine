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
      get
      {
        var anm = (tabAnmeldungMaschine)gridAnmeldung.DataContext;
        anm.eAktivMaschine = anm.eMaschine;
        anm.ManuelleAnmeldung = true;
        return anm;
      }
    }

    public FormNeueAnmeldung(IEnumerable<tabBediener> Bediener, IEnumerable<tabMaschine> Maschinen, tabMaschine AktuelleMaschine)
    {
      InitializeComponent();

      cmbMaschine.ItemsSource = Maschinen;
      var idisAngemeldet = Maschinen.SelectMany(s => s.sAktiveAnmeldungen).Select(s => s.fBediener).ToArray();
      var bediener = Bediener.Where(w => !idisAngemeldet.Contains(w.Id)).OrderBy(o => o.Name).ToList();
      cmbBediener.ItemsSource = bediener;

      var anmeldung = new tabAnmeldungMaschine()
      {
        Anmeldung = DateTime.Now,
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
