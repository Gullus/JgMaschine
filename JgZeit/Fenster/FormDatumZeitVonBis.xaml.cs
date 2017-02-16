using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace JgZeitHelper.Fenster
{
  public partial class FormDatumZeitVonBis : Window
  {
    private JgZeit _ZeitAnzeigeVon { get { return (JgZeit)this.FindResource("dzZeitVon"); } }
    private JgZeit _ZeitAnzeigeBis { get { return (JgZeit)this.FindResource("dzZeitBis"); } }

    public DateTime DatumZeitVon { get { return _ZeitAnzeigeVon.AnzeigeDatumZeit; } }
    public DateTime DatumZeitBis { get { return _ZeitAnzeigeBis.AnzeigeDatumZeit; } }

    public FormDatumZeitVonBis(string Caption, string Anzeigetext, DateTime DatumZeitVon, DateTime DatumZeitBis)
    {
      InitializeComponent();
      this.Title = Caption;
      tbInformation.Text = Anzeigetext;
      _ZeitAnzeigeVon.AnzeigeDatumZeit = DatumZeitVon;
      _ZeitAnzeigeBis.AnzeigeDatumZeit = DatumZeitBis;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
