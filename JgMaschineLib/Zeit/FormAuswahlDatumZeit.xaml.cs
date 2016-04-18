using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace JgMaschineLib.Zeit
{
  public partial class FormAuswahlDatumZeit : Window
  {
    private JgMaschineLib.JgDatumZeit _DatumZeit { get { return (JgMaschineLib.JgDatumZeit)this.FindResource("dzUmmeldung"); } }
    public DateTime DatumZeit { get { return _DatumZeit.DatumZeit; } }

    public FormAuswahlDatumZeit(string Caption, string Anzeigetext, DateTime DatumZeit)
    {
      InitializeComponent();
      this.Title = Caption;
      tbInformation.Text = Anzeigetext;
      _DatumZeit.DatumZeit = DatumZeit;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
