using System;
using System.Collections.Generic;
using System.Windows;
using JgMaschineData;
using JgMaschineLib.Zeit;

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

      var dz = (JgDatumZeit)this.FindResource("dzReparaturVon");
      dz.DatumZeit = anmeldung.Anmeldung;
      dz.NeuerWert = (dat) => anmeldung.Anmeldung = dat;

      gridAnmeldungReparatur.DataContext = anmeldung;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
