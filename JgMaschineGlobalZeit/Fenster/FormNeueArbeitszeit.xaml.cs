using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JgMaschineData;
using JgZeitHelper;

namespace JgMaschineGlobalZeit.Fenster
{
  public partial class FormNeueArbeitszeit : Window
  {
    private JgZeit _ZeitVon { get { return (JgZeit)FindResource("JgZeitVon"); } }
    private JgZeit _ZeitBis { get { return (JgZeit)FindResource("JgZeitBis"); } }

    public tabStandort Standort { get { return (tabStandort)cmbStandort.SelectedValue; } }
    public tabBediener Bediener { get { return (tabBediener)cmbMitarbeiter.SelectedValue; } }

    public DateTime DatumVon { get { return _ZeitVon.AnzeigeDatumZeit; } }
    public DateTime DatumBis { get { return _ZeitBis.AnzeigeDatumZeit; } }

    public FormNeueArbeitszeit(IEnumerable<tabStandort> ListeStandort, IEnumerable<tabBediener> ListeBediener)
    {
      InitializeComponent();

      cmbStandort.ItemsSource = ListeStandort;
      cmbMitarbeiter.ItemsSource = ListeBediener;

      _ZeitVon.AnzeigeDatumZeit = DateTime.Now;
      _ZeitBis.AnzeigeDatumZeit = DateTime.Now.Add(new TimeSpan(0, 32, 0));
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
