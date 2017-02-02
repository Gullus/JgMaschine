using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineGlobalZeit.Fenster
{
  public partial class FormOptionen : Window
  {
    private AnmeldungAuswertung _Auswertung;

    public FormOptionen(AnmeldungAuswertung Auswertung)
    {
      InitializeComponent();

      _Auswertung = Auswertung;
      tbJahr.Text = Auswertung.Jahr.ToString();

      for (byte i = 1; i <= 12; i++)
      {
        if (_Auswertung.ListeSollstundenJahr.FirstOrDefault(f => f.Monat == i) == null)
        {
          var neuSoll = new tabSollStunden()
          {
            Id = Guid.NewGuid(),
            Jahr = Auswertung.Jahr,
            Monat = i,
          };
          DbSichern.AbgleichEintragen(neuSoll.DatenAbgleich, EnumStatusDatenabgleich.Neu);
          _Auswertung.Db.tabSollStundenSet.Add(neuSoll);
          _Auswertung.ListeSollstundenJahr.Add(neuSoll);
        };
      }

      var idisBediener = Auswertung.ListeBediener.Select(s => s.Id).ToArray();
      var auswertungenVorjahr = Auswertung.Db.tabArbeitszeitAuswertungSet.Where(w => (idisBediener.Contains(w.fBediener)) && (w.Jahr == Auswertung.Jahr) && (w.Monat == 0)).ToList();
      foreach (var bediener in _Auswertung.ListeBediener)
      {
        var auswVorjahr = auswertungenVorjahr.FirstOrDefault(f => f.fBediener == bediener.Id);
        if (auswVorjahr == null)
        {
          auswVorjahr = new tabArbeitszeitAuswertung()
          {
            Id = Guid.NewGuid(),
            Jahr = Auswertung.Jahr,
            Monat = 0,
            Ueberstunden = "00:00",
            Urlaub = 0,

            eBediener = bediener,
            Status = EnumStatusArbeitszeitAuswertung.Erledigt,
          };
          DbSichern.AbgleichEintragen(auswVorjahr.DatenAbgleich, EnumStatusDatenabgleich.Neu);
          Auswertung.Db.tabArbeitszeitAuswertungSet.Add(auswVorjahr);
        }
        bediener.eArbeitszeitHelper = auswVorjahr;
      }
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var vsFeiertage = ((CollectionViewSource)(this.FindResource("vsFeiertage")));
      vsFeiertage.Source = _Auswertung.ListeFeiertageJahr;
      var vsSollStunden = ((CollectionViewSource)(this.FindResource("vsSollStunden")));
      vsSollStunden.Source = _Auswertung.ListeSollstundenJahr;
      var vsBediener = ((CollectionViewSource)(this.FindResource("vsBediener")));
      vsBediener.Source = _Auswertung.ListeBediener;
      var vsPausen = ((CollectionViewSource)(this.FindResource("vsPausen")));
      vsPausen.Source = _Auswertung.ListePausen;
      var vsRunde = ((CollectionViewSource)(this.FindResource("vsRunden")));
      vsRunde.GroupDescriptions.Add(new PropertyGroupDescription("eStandort.Bezeichnung"));
      vsRunde.Source = _Auswertung.ListeRundenJahr;
    }

    private void btnNeuerFeiertag_Click(object sender, RoutedEventArgs e)
    {
      var feiertag = new tabFeiertage()
      {
        Id = Guid.NewGuid(),
        Datum = DateTime.Now.Date
      };
      DbSichern.AbgleichEintragen(feiertag.DatenAbgleich, EnumStatusDatenabgleich.Neu);
      _Auswertung.Db.tabFeiertageSet.Add(feiertag);
      _Auswertung.ListeFeiertageJahr.Add(feiertag);
    }

    private void btnNeuePause_Click(object sender, RoutedEventArgs e)
    {
      var pause = new tabPausenzeit()
      {
        Id = Guid.NewGuid(),
        ZeitVon = new TimeSpan(5, 0, 0),
        ZeitBis = new TimeSpan(13, 0, 0),
        Pausenzeit = new TimeSpan(1, 0, 0)
      };
      DbSichern.AbgleichEintragen(pause.DatenAbgleich, EnumStatusDatenabgleich.Neu);
      _Auswertung.Db.tabPausenzeitSet.Add(pause);
      _Auswertung.ListePausen.Add(pause);
    }

    private void btnRundungswert_Click(object sender, RoutedEventArgs e)
    {
      var vorg = Convert.ToByte((sender as Button).Tag);  // 0 - Neu, 1 - Bearbeiten 
      var vsRunden = (CollectionViewSource)FindResource("vsRunden");
      var runden = (tabArbeitszeitRunden)vsRunden.View.CurrentItem;

      var fo = new FormArbeitszeitRunden(_Auswertung, runden, vorg == 0);
      if (!(fo.ShowDialog() ?? false))
        vsRunden.View.Refresh();
      vsRunden.View.MoveCurrentTo(fo.AktuellerWert);
    }
  }
}
