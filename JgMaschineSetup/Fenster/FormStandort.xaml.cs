using System;
using System.Windows;
using JgMaschineData;

namespace JgMaschineSetup.Fenster
{
  public partial class FormStandort : Window
  {
    public tabStandort Standort { get; set; }

    public FormStandort(tabStandort Standort)
    {
      InitializeComponent();
      this.Standort = Standort;
      if (this.Standort == null)
        this.Standort = new tabStandort() { Id = Guid.NewGuid() };
      gridStandort.DataContext = this.Standort;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
