using System;
using System.Collections.Generic;
using System.Windows;

namespace JgGlobalZeit.Fenster
{
  public partial class FormOptionen : Window
  {
    public JgMaschineData.tabStandort StandOrt
    {
      get { return (JgMaschineData.tabStandort)cmbStandort.SelectedItem; }
    }

    public FormOptionen(IEnumerable<JgMaschineData.tabStandort> Standorte)
    {
      InitializeComponent();
      cmbStandort.ItemsSource = Standorte;
      cmbStandort.SelectedValue = Properties.Settings.Default.IdStandort;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      Properties.Settings.Default.IdStandort = (Guid)cmbStandort.SelectedValue;
      this.DialogResult = true;
    }
  }
}
