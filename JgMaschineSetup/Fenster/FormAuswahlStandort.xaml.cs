using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
