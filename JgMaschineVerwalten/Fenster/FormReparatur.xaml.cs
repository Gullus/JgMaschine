﻿using JgMaschineLib.Zeit;
using System;
using System.Linq;
using System.Windows;

namespace JgMaschineVerwalten.Fenster
{
  public partial class FormReparatur : Window
  {
    private JgMaschineData.JgModelContainer _Db;
    private JgMaschineData.tabReparatur _Reparatur;
    public JgMaschineData.tabReparatur Reparatur
    {
      get { return _Reparatur; }
      set { _Reparatur = value; }
    }

    public FormReparatur(JgMaschineData.JgModelContainer Db, JgMaschineData.tabReparatur Reparatur)
    {
      InitializeComponent();

      _Db = Db;

      _Reparatur = Reparatur;
      if (_Reparatur == null)
      {
        _Reparatur = new JgMaschineData.tabReparatur()
        {
          Id = Guid.NewGuid(),
          ReparaturVon = DateTime.Now,
          ReparaturBis = DateTime.Now,
          InBearbeitung = true
        };
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      cmbEreigniss.ItemsSource = Enum.GetValues(typeof(JgMaschineData.EnumReperaturEreigniss));

      var bediener = _Db.tabBedienerSet.Where(w => w.Status == JgMaschineData.EnumStatusBediener.Aktiv).ToList();
      cmbVerursacher.ItemsSource = bediener;
      cmbProtokollant.ItemsSource = bediener;

      var dz = (JgDatumZeit)this.FindResource("dzReparaturVon");
      dz.DatumZeit = _Reparatur.ReparaturVon;
      dz.NeuerWert = (dat) =>
      {
        _Reparatur.ReparaturVon = dat;
      };

      dz = (JgDatumZeit)this.FindResource("dzReparaturBis");
      dz.DatumZeit = _Reparatur.ReparaturBis;
      dz.NeuerWert = (dat) => { _Reparatur.ReparaturBis = dat; };

      gridReparatur.DataContext = _Reparatur;
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
