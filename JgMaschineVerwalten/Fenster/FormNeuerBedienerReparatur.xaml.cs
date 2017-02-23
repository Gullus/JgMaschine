using System;
using System.Collections.Generic;
using System.Windows;
using JgMaschineData;
using JgZeitHelper;

namespace JgMaschineVerwalten.Fenster
{
  public partial class FormNeuerBedienerReparatur : Window
  {
    public tabAnmeldungReparatur Anmeldung { get { return (tabAnmeldungReparatur)gridAnmeldungReparatur.DataContext; } }

    public FormNeuerBedienerReparatur(IEnumerable<tabBediener> Bediener)
    {
      InitializeComponent();
      cmbBediener.ItemsSource = Bediener;

      var anmeldung = new tabAnmeldungReparatur()
      {
        Anmeldung = DateTime.Now
      };

      var dz = (JgZeit)this.FindResource("dzReparaturVon");
      dz.AnzeigeDatumZeit = anmeldung.Anmeldung;
      dz.OnNeuerWert = (datum, zeit) => anmeldung.Anmeldung = datum + zeit;

      gridAnmeldungReparatur.DataContext = anmeldung;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
