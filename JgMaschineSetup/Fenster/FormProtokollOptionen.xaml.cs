using System.Windows;
using JgMaschineData;

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
