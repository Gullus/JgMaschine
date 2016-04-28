using System;
using System.Windows;

namespace JgMaschineLib.Zeit
{
  public partial class FormAuswahlDatumVonBis : Window
  {
    private JgDatumZeit _DatumVon { get { return (JgDatumZeit)this.FindResource("dzDatumVon"); } }
    public DateTime DatumVon { get { return _DatumVon.DatumZeit; } }

    private JgDatumZeit _DatumBis { get { return (JgDatumZeit)this.FindResource("dzDatumBis"); } }
    public DateTime DatumBis { get { return _DatumBis.DatumZeit; } }

    public FormAuswahlDatumVonBis(string Caption, string Anzeigetext, DateTime DatumZeitVon, DateTime DatumZeitBis)
    {
      InitializeComponent();
      this.Title = Caption;
      tbInformation.Text = Anzeigetext;
      _DatumVon.DatumZeit = DatumZeitVon;
      _DatumBis.DatumZeit = DatumZeitBis;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
