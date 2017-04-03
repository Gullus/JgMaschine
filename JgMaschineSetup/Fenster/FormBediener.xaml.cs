using System;
using System.Collections.Generic;
using System.Windows;
using JgMaschineData;

namespace JgMaschineSetup.Fenster
{

  public partial class FormBediener : Window
  {
    public tabBediener Bediener { get { return (tabBediener)gridBediener.DataContext; } }

    public FormBediener(tabBediener Bediener, IEnumerable<tabStandort> Standorte)
    {
      InitializeComponent();
      cmbStandort.ItemsSource = Standorte;
      cmbStatus.ItemsSource = Enum.GetValues(typeof(EnumStatusBediener));

      if (Bediener == null)
        Bediener = new tabBediener() { Status = EnumStatusBediener.Aktiv, fStandort = (cmbStandort.Items[0] as tabStandort).Id };
      gridBediener.DataContext = Bediener;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
