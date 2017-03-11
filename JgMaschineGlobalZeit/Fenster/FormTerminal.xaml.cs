using System;
using System.Collections.Generic;
using System.Windows;
using JgMaschineData;

namespace JgGlobalZeit.Fenster
{
  public partial class FormTerminal : Window
  {
    public tabArbeitszeitTerminal Terminal { get { return (tabArbeitszeitTerminal)gridTerminal.DataContext; } }
    public bool IstNeu { get; set; }

    public FormTerminal(IEnumerable<tabStandort> ListeStandorte, tabArbeitszeitTerminal NeuTerminal = null)
    {
      InitializeComponent();
      cmbStandort.ItemsSource = ListeStandorte;
      IstNeu = NeuTerminal == null;
      gridTerminal.DataContext = NeuTerminal ?? new tabArbeitszeitTerminal() { Id = Guid.NewGuid() };
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
