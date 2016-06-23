using JgMaschineLib.Zeit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace JgMaschineVerwalten.Fenster
{
  public partial class FormReparatur : Window
  {
    public JgMaschineData.tabReparatur Reparatur { get; set; }

    public FormReparatur(JgMaschineData.tabReparatur Reparatur, IEnumerable<JgMaschineData.tabMaschine> Maschinen, IEnumerable<JgMaschineData.tabBediener> Bediener, JgMaschineData.tabMaschine AktuelleMaschine = null)
    {
      InitializeComponent();

      cmbMaschine.ItemsSource = Maschinen;
      cmbVerursacher.ItemsSource = Bediener;
      cmbProtokollant.ItemsSource = Bediener;
      cmbEreigniss.ItemsSource = Enum.GetValues(typeof(JgMaschineData.EnumReperaturEreigniss));

      if (Reparatur == null)
      {
        Reparatur = new JgMaschineData.tabReparatur()
        {
          Id = Guid.NewGuid(),
          VorgangBeginn = DateTime.Now,
          VorgangEnde = DateTime.Now.AddMinutes(30),
          fMaschine = AktuelleMaschine.Id,
          IstAktiv = true
        };
      }
      this.Reparatur = Reparatur;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var dz = (JgDatumZeit)this.FindResource("dzReparaturVon");
      dz.DatumZeit = Reparatur.VorgangBeginn;
      dz.NeuerWert = (dat) => Reparatur.VorgangBeginn = dat;

      dz = (JgDatumZeit)this.FindResource("dzReparaturBis");
      dz.DatumZeit = Reparatur.VorgangEnde;
      dz.NeuerWert = (dat) => { Reparatur.VorgangEnde = dat; };

      gridReparatur.DataContext = Reparatur;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
