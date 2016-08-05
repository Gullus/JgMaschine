﻿using System;
using System.Collections.Generic;
using System.Windows;
using JgMaschineData;
using JgMaschineLib.Zeit;

namespace JgMaschineVerwalten.Fenster
{
  public partial class FormReparatur : Window
  {
    public JgMaschineData.tabReparatur Reparatur { get { return (tabReparatur)gridReparatur.DataContext; } }

    public FormReparatur(JgMaschineData.tabReparatur Reparatur, IEnumerable<tabBediener> Bediener, tabMaschine AktuelleMaschine = null)
    {
      InitializeComponent();
      tblMaschine.Text = Reparatur?.eMaschine?.MaschinenName ?? AktuelleMaschine.MaschinenName;

      cmbVerursacher.ItemsSource = Bediener;
      cmbProtokollant.ItemsSource = Bediener;
      cmbVorgangs.ItemsSource = Enum.GetValues(typeof(JgMaschineData.EnumReperaturVorgang));

      if (Reparatur == null)
      {
        Reparatur = new JgMaschineData.tabReparatur()
        {
          VorgangBeginn = DateTime.Now,
          fMaschine = AktuelleMaschine.Id
        };

        lbEnde2.Visibility = Visibility.Collapsed;
        lbStunde2.Visibility = Visibility.Collapsed;
        lbMinute2.Visibility = Visibility.Collapsed;

        dtpRepBis.Visibility = Visibility.Collapsed;
        cmbStunde2.Visibility = Visibility.Collapsed;
        cmbMinute2.Visibility = Visibility.Collapsed;

        btnAktuelleZeit2.Visibility = Visibility.Collapsed;
      }
      else
      {
        var dzBis = (JgDatumZeit)this.FindResource("dzReparaturBis");
        dzBis.DatumZeit = Reparatur.VorgangEnde ?? DateTime.Now;
        dzBis.NeuerWert = (dat) => { Reparatur.VorgangEnde = dat; };
      }

      var dzVon = (JgDatumZeit)this.FindResource("dzReparaturVon");
      dzVon.DatumZeit = Reparatur.VorgangBeginn;
      dzVon.NeuerWert = (dat) => Reparatur.VorgangBeginn = dat;

      gridReparatur.DataContext = Reparatur;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
