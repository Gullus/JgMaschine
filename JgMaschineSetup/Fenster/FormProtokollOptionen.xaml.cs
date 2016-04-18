using JgMaschineData;
using System;
using System.Windows;
using System.Linq;
using System.Windows.Controls;

namespace JgMaschineSetup
{
  public partial class FormProtokollOptionen : Window
  {
    private JgModelContainer _Db;

    private tabProtokoll _Protokoll;
    public tabProtokoll Protokoll
    {
      get { return _Protokoll; }
      set { _Protokoll = value; }
    }

    public FormProtokollOptionen(JgModelContainer Db, tabProtokoll Protokoll)
    {
      InitializeComponent();

      _Db = Db;
      _Protokoll = Protokoll; 
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      gridProtokoll.DataContext = _Protokoll;
    }
  }
}
