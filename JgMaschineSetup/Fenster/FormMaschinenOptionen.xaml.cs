using JgMaschineData;
using System;
using System.Windows;
using System.Linq;
using System.Windows.Controls;

namespace JgMaschineSetup
{
  public partial class FormMaschinenOptionen : Window
  {
    private JgModelContainer _Db;

    private tabMaschine _Maschine;
    public tabMaschine Maschine
    {
      get { return _Maschine; }
      set { _Maschine = value; }
    }

    public FormMaschinenOptionen(JgModelContainer Db, tabMaschine Maschine)
    {
      InitializeComponent();

      _Db = Db;
      _Maschine = Maschine;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      if (_Maschine == null)
        _Maschine = new tabMaschine() { Id = Guid.NewGuid(), fStandort = _Db.tabStandortSet.FirstOrDefault().Id };


      gridMaschine.DataContext = _Maschine;

      cmbStandort.ItemsSource = _Db.tabStandortSet.OrderBy(o => o.Bezeichnung).ToList();
      cmbProtokolle.ItemsSource = Enum.GetValues(typeof(JgMaschineData.EnumProtokollName));
      cmbStatus.ItemsSource = Enum.GetValues(typeof(JgMaschineData.EnumStatusMaschine));
    }

    private void ButtonPfad_Click(object sender, RoutedEventArgs e)
    {
      System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
      dialog.SelectedPath = _Maschine.PfadDaten;
      if (sender == btnDateiBediener)
        dialog.SelectedPath = _Maschine.PfadBediener;
      if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        if (sender == btnDateiBediener)
        {
          _Maschine.PfadBediener = dialog.SelectedPath;
          tbPfadBediener.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }
        else
        {
          _Maschine.PfadDaten = dialog.SelectedPath;
          tbPfadDaten.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }
      }
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
