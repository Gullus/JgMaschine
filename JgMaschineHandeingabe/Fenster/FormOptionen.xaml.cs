﻿using System;
using System.Linq;
using System.Windows;

namespace JgMaschineHandeingabe.Fenster
{
  public partial class FormOptionen : Window
  {
 
    public FormOptionen(JgMaschineData.JgModelContainer _Db)
    {
      InitializeComponent();
      cmbStandort.ItemsSource = _Db.tabStandortSet.OrderBy(o => o.Bezeichnung).ToList();
      cmbStandort.SelectedValue = Properties.Settings.Default.IdStandort;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      Properties.Settings.Default.IdStandort = (Guid)cmbStandort.SelectedValue;
      this.DialogResult = true;
    }
  }
}