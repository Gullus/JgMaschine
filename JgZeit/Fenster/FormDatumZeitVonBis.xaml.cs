using System;
using System.Windows;

namespace JgZeitHelper.Fenster
{
  public partial class FormDatumZeitVonBis : Window
  {
    private JgZeit _ZeitAnzeigeVon { get { return (JgZeit)this.FindResource("dzZeitVon"); } }
    private JgZeit _ZeitAnzeigeBis { get { return (JgZeit)this.FindResource("dzZeitBis"); } }


    public DateTime? _ErgDatumZeitVon;
    public DateTime? _ErgDatumZeitBis;

    public DateTime? DatumZeitVon { get { return (cbDatumVonLeer.IsChecked ?? false) ? null : _ErgDatumZeitVon; } }
    public DateTime? DatumZeitBis { get { return (cbDatumBisLeer.IsChecked ?? false) ? null : _ErgDatumZeitBis; } }

    public FormDatumZeitVonBis(string Caption, string Anzeigetext, DateTime? DatumZeitVon, bool DatumVonNullZulassen, DateTime? DatumZeitBis, bool DatumBisNullZulassen)
    {
      InitializeComponent();
      this.Title = Caption;
      tbInformation.Text = Anzeigetext;

      _ErgDatumZeitVon = DatumZeitVon;
      _ZeitAnzeigeVon.AnzeigeDatumZeit = DatumZeitVon == null ? DateTime.Now : DatumZeitVon.Value;
      _ZeitAnzeigeVon.OnNeuerWert = (d, z) =>
      {
        _ErgDatumZeitVon = d + z;
        if (cbDatumVonLeer.IsChecked ?? false)
          cbDatumVonLeer.IsChecked = false;
      };
 
      _ErgDatumZeitBis = DatumZeitBis;
      _ZeitAnzeigeBis.AnzeigeDatumZeit = DatumZeitBis == null ? (_ErgDatumZeitVon == null ? DateTime.Now : _ErgDatumZeitVon.Value.Date + JgZeit.DatumInZeitMinute(DateTime.Now)) : DatumZeitBis.Value;
      _ZeitAnzeigeBis.OnNeuerWert = (d, z) =>
      {
        _ErgDatumZeitBis = d + z;
        if (cbDatumBisLeer.IsChecked ?? false)
          cbDatumBisLeer.IsChecked = false;
      };

      cbDatumVonLeer.Visibility = DatumVonNullZulassen ? Visibility.Visible : Visibility.Collapsed;
      cbDatumVonLeer.IsChecked = !DatumZeitVon.HasValue;

      cbDatumBisLeer.Visibility = DatumBisNullZulassen ? Visibility.Visible : Visibility.Collapsed;
      cbDatumBisLeer.IsChecked = !DatumZeitBis.HasValue;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
