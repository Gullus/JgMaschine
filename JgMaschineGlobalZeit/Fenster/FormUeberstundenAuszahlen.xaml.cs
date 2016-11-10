using System.Windows;

namespace JgGlobalZeit.Fenster
{
  public partial class FormUeberstundenAuszahlen : Window
  { 
    public string UerbstundenAuszahlem { get { return tbUeberstunden.Text; } }

    public FormUeberstundenAuszahlen(string AltSollStunden)
    {
      InitializeComponent();
      tbUeberstunden.Text = AltSollStunden;
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
