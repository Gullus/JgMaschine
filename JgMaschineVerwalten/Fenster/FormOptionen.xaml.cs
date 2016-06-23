using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace JgMaschineVerwalten.Fenster
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
