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
  /// <summary>
  /// Interaktionslogik für FormBediener.xaml
  /// </summary>
  public partial class FormStandort : Window
  {
    private JgModelContainer _Db;
    
    private tabStandort _Standort;
    public tabStandort Standort
    {
      get { return _Standort; }
      set { _Standort = value; }
    }

    public FormStandort()
    {
      InitializeComponent();
    }

    public FormStandort(JgModelContainer Db, tabStandort Standort) : this()
    {
      _Db = Db;
      _Standort = Standort;
      if (_Standort == null)
        _Standort = new tabStandort() { Id = Guid.NewGuid() };
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      gridStandort.DataContext = _Standort;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
