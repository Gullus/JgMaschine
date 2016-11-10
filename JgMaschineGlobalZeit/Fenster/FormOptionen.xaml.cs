using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
        if (_Auswertung.ListeSollStunden.FirstOrDefault(f => f.Monat == i) == null)
        {
          var neuSoll = new tabSollStunden()
          {
            Id = Guid.NewGuid(),
            Jahr = Auswertung.Jahr,
            Monat = i,
          };
          DbSichern.AbgleichEintragen(neuSoll.DatenAbgleich, EnumStatusDatenabgleich.Neu);
          _Auswertung.Db.tabSollStundenSet.Add(neuSoll);
          _Auswertung.ListeSollStunden.Add(neuSoll);
        };
      }
      _Auswertung.Db.SaveChanges();

      var auswertungenVorjahr = Auswertung.Db.tabArbeitszeitAuswertungSet.Where(w => (Auswertung.IdisBediener.Contains(w.fBediener)) &&(w.Jahr == Auswertung.Jahr) && (w.Monat == 0)).ToList();
      foreach(var bediener in _Auswertung.ListeBediener)
      {
        var auswVorjahr = auswertungenVorjahr.FirstOrDefault(f => f.fBediener == bediener.Id);
        if (auswVorjahr == null)
        {
          auswVorjahr = new tabArbeitszeitAuswertung()
          {
            Id = Guid.NewGuid(),
            Jahr = Auswertung.Jahr,
            Monat = 0,
            eBediener = bediener,

            Urlaub = 0,
            Status = EnumStatusArbeitszeitAuswertung.Erledigt,
          };
          JgMaschineLib.DbSichern.AbgleichEintragen(auswVorjahr.DatenAbgleich, EnumStatusDatenabgleich.Neu);
          Auswertung.Db.tabArbeitszeitAuswertungSet.Add(auswVorjahr);          
        }
        bediener.ErgebnisVorjahr = auswVorjahr;
      }
      Auswertung.Db.SaveChanges();
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var vsFeiertage = ((CollectionViewSource)(this.FindResource("vsFeiertage")));
      vsFeiertage.Source = _Auswertung.ListeFeiertage;
      var vsSollStunden = ((CollectionViewSource)(this.FindResource("vsSollStunden")));
      vsSollStunden.Source = _Auswertung.ListeSollStunden;
      var vsBediener = ((CollectionViewSource)(this.FindResource("vsBediener")));
      vsBediener.Source = _Auswertung.ListeBediener;
      var vsPausen = ((CollectionViewSource)(this.FindResource("vsPausen")));
      vsPausen.Source = _Auswertung.ListePausenzeiten;
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
      _Auswertung.Db.SaveChanges();
      _Auswertung.ListeFeiertage.Add(feiertag);
      //_Db.tabFeiertageSet.Add(feiertag);
      //      }
      //      _Db.SaveChanges();
      //      break;
      //    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
      //      foreach (var ds in erg.OldItems)
      //      {
      //        var feiertag = (tabFeiertage)ds;
      //        feiertag.DatenAbgleich.Geloescht = true;
      //        DbSichern.AbgleichEintragen(feiertag.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      //      }
      //      break;
      //  }
      //};
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
      _Auswertung.Db.SaveChanges();
      _Auswertung.ListePausenzeiten.Add(pause);
    }
  }
}
