using System;
using System.Windows;

namespace JgGlobalZeit.Fenster
{
  public partial class FormSollstundenEinstellen : Window
  { 
    public TimeSpan Sollstunden { get { return JgZeitHelper.JgZeit.StringInZeit(tbSollstunden.Text, TimeSpan.Zero); } }

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
