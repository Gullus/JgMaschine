using JgMaschineData;
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

namespace JgMaschineSetup.Fenster
{

  public partial class FormBediener : Window
  {
    private JgModelContainer _Db;
    
    private tabBediener _Bediener;
    public tabBediener Bediener
    {
      get { return _Bediener; }
      set { _Bediener = value; }
    }

    public FormBediener(JgModelContainer Db, tabBediener Bediener)
    {
      InitializeComponent();
      _Db = Db;
      _Bediener = Bediener;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      cmbStatus.ItemsSource = Enum.GetValues(typeof(EnumStatusBediener));
      cmbStandort.ItemsSource = _Db.tabStandortSet.Where(w => w.DatenAbgleich.Geloescht).OrderBy(o => o.Bezeichnung).ToList();

      if (_Bediener == null)
        _Bediener = new tabBediener() { Id = Guid.NewGuid(), Status = EnumStatusBediener.Aktiv, fStandort = (cmbStandort.Items[0] as tabStandort).Id };
      gridBediener.DataContext = _Bediener;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
