using System;
using System.Collections.Generic;
using System.Windows;
using JgMaschineData;
using JgZeitHelper;

namespace JgMaschineVerwalten.Fenster
{
  public partial class FormReparatur : Window
  {
    public tabReparatur Reparatur { get { return (tabReparatur)gridReparatur.DataContext; } }

    public FormReparatur(tabReparatur NeuReparatur, IEnumerable<tabBediener> Bediener, tabMaschine AktuelleMaschine = null)
    {
      InitializeComponent();

      cmbVerursacher.ItemsSource = Bediener;
      cmbProtokollant.ItemsSource = Bediener;
      cmbVorgangs.ItemsSource = Enum.GetValues(typeof(EnumReperaturVorgang));
      cmbVorgangs.SelectedIndex = 0;

      if (NeuReparatur == null)
      {
        NeuReparatur = new JgMaschineData.tabReparatur()
        {
          VorgangBeginn = DateTime.Now,
          eMaschine = AktuelleMaschine
        };

        lbBisDatum.Visibility = Visibility.Collapsed;
        dtpRepBis.Visibility = Visibility.Collapsed;
        lbBisPunkt.Visibility = Visibility.Collapsed;
        tbBisZeit.Visibility = Visibility.Collapsed;
      }
      else
      {
        var dzBis = (JgZeit)this.FindResource("dzReparaturBis");
        dzBis.AnzeigeDatumZeit = NeuReparatur.VorgangEnde ?? DateTime.Now;
        dzBis.OnNeuerWert = (datum, zeit) => Reparatur.AnzeigeVorgangEnde = datum + zeit;
      }

      var dzVon = (JgZeit)this.FindResource("dzReparaturVon");
      dzVon.AnzeigeDatumZeit = NeuReparatur.VorgangBeginn;
      dzVon.OnNeuerWert = (datum, zeit) => NeuReparatur.AnzeigeVorgangBeginn = datum + zeit;

      gridReparatur.DataContext = NeuReparatur;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
