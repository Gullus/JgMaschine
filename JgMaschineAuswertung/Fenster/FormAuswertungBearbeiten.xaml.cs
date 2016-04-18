using JgMaschineData;
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

namespace JgMaschineAuswertung.Fenster
{

  public partial class FormAuswertungBearbeiten : Window
  {
    private tabAuswertung _Auswertung;
    public tabAuswertung Auswertung
    {
      get { return _Auswertung; }
      set { _Auswertung = value; }
    }

    public FormAuswertungBearbeiten(tabAuswertung Auswertung)
    {     
      InitializeComponent();
      
      _Auswertung = Auswertung;
      if (_Auswertung == null)
        _Auswertung = new tabAuswertung();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      gridAuswertung.DataContext = _Auswertung;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
