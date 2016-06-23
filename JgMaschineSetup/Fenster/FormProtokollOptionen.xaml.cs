using JgMaschineData;
using System;
using System.Windows;
using System.Linq;
using System.Windows.Controls;

namespace JgMaschineSetup
{
  public partial class FormProtokollOptionen : Window
  {
    public tabProtokoll Protokoll { set; get; }

    public FormProtokollOptionen(tabProtokoll Protokoll)
    {
      InitializeComponent();
      this.Protokoll = Protokoll; 
      gridProtokoll.DataContext = this.Protokoll;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
