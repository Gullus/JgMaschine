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

    public FormReparatur(tabReparatur Reparatur, IEnumerable<tabBediener> Bediener, tabMaschine AktuelleMaschine = null)
    {
      InitializeComponent();
      tblMaschine.Text = Reparatur?.eMaschine.MaschinenName ?? AktuelleMaschine.MaschinenName;

      cmbVerursacher.ItemsSource = Bediener;
      cmbProtokollant.ItemsSource = Bediener;
      cmbVorgangs.ItemsSource = Enum.GetValues(typeof(EnumReperaturVorgang));
      cmbVorgangs.SelectedIndex = 0;

      if (Reparatur == null)
      {
        Reparatur = new JgMaschineData.tabReparatur()
        {
          VorgangBeginn = DateTime.Now,
          fMaschine = AktuelleMaschine.Id
        };

        tblMaschine.Text = AktuelleMaschine.MaschinenName;

        lbBisDatum.Visibility = Visibility.Collapsed;
        dtpRepBis.Visibility = Visibility.Collapsed;
        lbBisPunkt.Visibility = Visibility.Collapsed;
        tbBisZeit.Visibility = Visibility.Collapsed;
      }
      else
      {
        tblMaschine.Text = Reparatur.eMaschine.MaschinenName;

        var dzBis = (JgZeit)this.FindResource("dzReparaturBis");
        dzBis.AnzeigeDatumZeit = Reparatur.VorgangEnde ?? DateTime.Now;
        dzBis.OnNeuerWert = (datum, zeit) => Reparatur.AnzeigeVorgangEnde = datum + zeit;
      }

      var dzVon = (JgZeit)this.FindResource("dzReparaturVon");
      dzVon.AnzeigeDatumZeit = Reparatur.VorgangBeginn;
      dzVon.OnNeuerWert = (datum, zeit) => Reparatur.AnzeigeVorgangBeginn = datum + zeit;

      gridReparatur.DataContext = Reparatur;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
