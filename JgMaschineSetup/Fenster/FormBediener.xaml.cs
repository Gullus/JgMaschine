using System;
using System.Collections.Generic;
using System.Windows;
using JgMaschineData;

namespace JgMaschineSetup.Fenster
{

  public partial class FormBediener : Window
  {
    public tabBediener Bediener { get; set; }

    public FormBediener(tabBediener Bediener, IEnumerable<tabStandort> Standorte)
    {
      InitializeComponent();
      this.Bediener = Bediener;
      cmbStandort.ItemsSource = Standorte;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      cmbStatus.ItemsSource = Enum.GetValues(typeof(EnumStatusBediener));

      if (Bediener == null)
        Bediener = new tabBediener() { Id = Guid.NewGuid(), Status = EnumStatusBediener.Aktiv, fStandort = (cmbStandort.Items[0] as tabStandort).Id };
      gridBediener.DataContext = Bediener;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
