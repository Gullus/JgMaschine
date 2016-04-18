using JgMaschineData;
using System.Windows;

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
