using System.Collections.Generic;
using System.Windows;
using JgMaschineData;

namespace JgMaschineSetup.Fenster
{
  /// <summary>
  /// Interaktionslogik für AuswahlStandort.xaml
  /// </summary>
  public partial class FormAuswahlStandort : Window
  {
    public tabStandort Standort { get { return (tabStandort)cmbStandort.SelectedItem; } }

    public FormAuswahlStandort(IEnumerable<tabStandort> Standorte)
    {
      InitializeComponent();
      cmbStandort.ItemsSource = Standorte;
      cmbStandort.SelectedIndex = 0;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
