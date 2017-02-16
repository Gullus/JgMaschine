using System;
using System.Windows;

namespace JgZeitHelper.Fenster
{
  public partial class FormDatumZeit : Window
  {
    private JgZeit _ZeitAnzeige { get { return (JgZeit)this.FindResource("dzZeit"); } }
    public DateTime DatumZeit { get { return _ZeitAnzeige.AnzeigeDatumZeit; } }

    public FormDatumZeit(string Caption, string Anzeigetext, DateTime DatumZeit)
    {
      InitializeComponent();
      this.Title = Caption;
      tbInformation.Text = Anzeigetext;
      _ZeitAnzeige.AnzeigeDatumZeit = DatumZeit;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
