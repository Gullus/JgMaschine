using System.Windows;

namespace JgGlobalZeit.Fenster
{
  public partial class FormSollstundenEinstellen : Window
  { 
    public string Sollstunden { get { return tbSollstunden.Text; } }

    public FormSollstundenEinstellen(string AltSollStunden)
    {
      InitializeComponent();
      tbSollstunden.Text = AltSollStunden;
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
