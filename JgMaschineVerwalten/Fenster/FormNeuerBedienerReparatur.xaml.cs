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
using JgMaschineLib.Zeit;

namespace JgMaschineVerwalten.Fenster
{
  public partial class FormNeuerBedienerReparatur : Window
  {
    public tabAnmeldungReparatur Anmeldung { get { return (tabAnmeldungReparatur)gridAnmeldungReparatur.DataContext; } }

    public FormNeuerBedienerReparatur(Dictionary<Guid, string> Bediener)
    {
      InitializeComponent();
      cmbBediener.ItemsSource = Bediener;

      var anmeldung = new tabAnmeldungReparatur()
      {
        Id = Guid.NewGuid(),
        Anmeldung = DateTime.Now
      };

      var dz = (JgDatumZeit)this.FindResource("dzReparaturVon");
      dz.DatumZeit = anmeldung.Anmeldung;
      dz.NeuerWert = (dat) => anmeldung.Anmeldung = dat;

      gridAnmeldungReparatur.DataContext = anmeldung;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
