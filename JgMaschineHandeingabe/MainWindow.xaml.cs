using JgMaschineData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Data.Entity;

namespace JgMaschineHandeingabe
{
  public partial class MainWindow : Window
  {
    private JgModelContainer _Db;
    private CollectionViewSource _VsMaschine { get { return (System.Windows.Data.CollectionViewSource)this.FindResource("vsMaschine"); } }
    private CollectionViewSource _VsDaten { get { return (System.Windows.Data.CollectionViewSource)this.FindResource("vsDaten"); } }
    private JgMaschineLib.JgDatumZeit _DsDatumZeit { get { return (JgMaschineLib.JgDatumZeit)this.FindResource("dsDatumZeit"); } }

    public MainWindow()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _Db = new JgModelContainer();

      _DsDatumZeit.DatumZeit = DateTime.Now;
      MaschinenWechseln();
    }

    private void MaschinenWechseln()
    {
      _VsMaschine.Source = _Db.tabMaschineSet.Where(w => (w.fStandort == Properties.Settings.Default.IdStandort) 
        && (w.ProtokollName == EnumProtokollName.Handbiegung)
        && (w.Status != EnumStatusMaschine.Stillgelegt)).ToList();      
      _VsMaschine.View.CurrentChanged += View_CurrentChanged;
      View_CurrentChanged(null, null);
    }

    void View_CurrentChanged(object sender, EventArgs e)
    {
      if (_VsMaschine.View.CurrentItem != null)
      {
        var maschine = (JgMaschineData.tabMaschine)_VsMaschine.View.CurrentItem;
        _VsDaten.Source = _Db.tabDatenSet.Where(w => w.fMaschine == maschine.Id).OrderByDescending(o => o.Id).Take(10).ToList();
      }
    }

    private void ButtonOptionen_Click(object sender, RoutedEventArgs e)
    {
      JgMaschineHandeingabe.Fenster.FormOptionen form = new Fenster.FormOptionen(_Db);
      if (form.ShowDialog() ?? false)
        MaschinenWechseln();
    }

    private void tbEingabe_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        tbEingabe.SelectAll();
        e.Handled = true;

        int idPosition = 0;
        try
        {
          idPosition = Convert.ToInt32((sender as TextBox).Text);
        }
        catch (Exception f)
        {
          MessageBox.Show(string.Format("Fehler beim konvertieren der Position !\n{0}", f.Message), "Eingabfehler", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
          
        var datum = _DsDatumZeit.DatumZeit;
        var maschine = (JgMaschineData.tabMaschine)_VsMaschine.View.CurrentItem;

        // var erg = _Db.BauteilInDaten(datum, idPosition, maschine.Id);
        // int zurueck = (int)erg.FirstOrDefault();

        int zurueck = 1;

        switch (zurueck)
        {
          case -1: tbAntwortServer.Text = "Daten bereits eingetragen !"; break;
          case -2: tbAntwortServer.Text = "Position nicht in JG Data gefunden !"; break;
          default :
            tbAntwortServer.Text = "OK !";
            (_VsDaten.View.SourceCollection as List<JgMaschineData.tabDaten>).Insert(0, _Db.tabDatenSet.Find(zurueck));
            _VsDaten.View.Refresh();
            _VsDaten.View.MoveCurrentToFirst();
            break;
        }
      }
    }

    private void ButtonAktuelleZeit_Click(object sender, RoutedEventArgs e)
    {
      _DsDatumZeit.DatumZeit = DateTime.Now;
    }

    private void FormBauteilHandeingabe_Closed(object sender, EventArgs e)
    {
      Properties.Settings.Default.Save();
    }

    private void cmbTest_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }
  }
}
