using System;
using System.Windows;

namespace JgMaschineGlobalZeit.Fenster
{
  public partial class FormUeberstundenAuszahlen : Window
  { 
    public TimeSpan UerbstundenAuszahlem { get { return JgZeitHelper.JgZeit.StringInZeit(tbUeberstunden.Text, TimeSpan.Zero); } }

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
