using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using JgMaschineData;

namespace JgMaschineSetup.Fenster
{
  public partial class FormMaschinenOptionen : Window
  {
    public tabMaschine Maschine { get; set; }

    public FormMaschinenOptionen(tabMaschine Maschine, IEnumerable<tabStandort> Standorte)
    {
      InitializeComponent();
      this.Maschine = Maschine;

      cmbStandort.ItemsSource = Standorte;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      cmbStatus.ItemsSource = Enum.GetValues(typeof(JgMaschineData.EnumStatusMaschine));
      cmbProtokolle.ItemsSource = Enum.GetValues(typeof(JgMaschineData.EnumMaschinenArt));

      if (Maschine == null)
        Maschine = new tabMaschine() { Id = Guid.NewGuid(), fStandort = (cmbStandort.Items[0] as tabStandort).Id };
     
      gridMaschine.DataContext = Maschine;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
