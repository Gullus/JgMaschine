using System;
using System.Windows;

namespace JgZeitHelper.Fenster
{
  public partial class FormDatumZeit : Window
  {
    private JgZeit _ZeitAnzeige { get { return (JgZeit)this.FindResource("dzZeit"); } }

    private DateTime? _ErgDatumZeit;
    public DateTime? DatumZeit { get { return (cbDatumLeer.IsChecked ?? false) ? null : _ErgDatumZeit; } }

    public FormDatumZeit(string Caption, string Anzeigetext, DateTime? DatumZeit, bool DatumNullZulassen)
    {
      InitializeComponent();
      this.Title = Caption;
      tbInformation.Text = Anzeigetext;

      _ErgDatumZeit = DatumZeit;
      _ZeitAnzeige.AnzeigeDatumZeit = DatumZeit == null ? DateTime.Now : DatumZeit.Value;
      _ZeitAnzeige.OnNeuerWert = (d, z) =>
      {
        _ErgDatumZeit = d + z;
        if (cbDatumLeer.IsChecked ?? false)
          cbDatumLeer.IsChecked = false;
      };

      cbDatumLeer.Visibility = DatumNullZulassen ? Visibility.Visible : Visibility.Collapsed;
      cbDatumLeer.IsChecked = !DatumZeit.HasValue;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
