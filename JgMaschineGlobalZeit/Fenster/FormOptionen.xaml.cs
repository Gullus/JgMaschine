using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using JgMaschineData;

namespace JgMaschineGlobalZeit.Fenster
{
  public partial class FormOptionen : Window
  {
    private AnmeldungAuswertung _Auswertung;
    private ObservableCollection<tabSollStunden> _SollStunden;
    private ObservableCollection<tabFeiertage> _Feiertage;


    public FormOptionen(AnmeldungAuswertung Auswertung)
    {
      InitializeComponent();

      _Auswertung = Auswertung;

      tbJahr.Text = Auswertung.Jahr.ToString();

      var sollStunden = Auswertung.Db.tabSollStundenSet.Where(w => w.Jahr == Auswertung.Jahr).ToList();
      for (byte i = 1; i <= 12; i++)
      {
        if (sollStunden.FirstOrDefault(f => f.Monat == i) == null)
        {
          var neuSoll = new tabSollStunden()
          {
            Id = Guid.NewGuid(),
            Jahr = Auswertung.Jahr,
            Monat = i,
          };
          JgMaschineLib.DbSichern.AbgleichEintragen(neuSoll.DatenAbgleich, EnumStatusDatenabgleich.Neu);
          sollStunden.Add(neuSoll);
          Auswertung.Db.tabSollStundenSet.Add(neuSoll);
        };
      }
      _SollStunden = new ObservableCollection<tabSollStunden>(sollStunden.OrderBy(o => o.Monat));
      Auswertung.Db.SaveChanges();

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
            Status = EnumStatusArbeitszeitAuswertung.Gebucht,
          };
          JgMaschineLib.DbSichern.AbgleichEintragen(auswVorjahr.DatenAbgleich, EnumStatusDatenabgleich.Neu);
          Auswertung.Db.tabArbeitszeitAuswertungSet.Add(auswVorjahr);          
        }
        bediener.ErgebnisVorjahr = auswVorjahr;
      }
      Auswertung.Db.SaveChanges();

      var datVom = new DateTime(Auswertung.Jahr, 1, 1);
      var datBis = new DateTime(Auswertung.Jahr, 12, 31);

      var feiertage = Auswertung.Db.tabFeiertageSet.Where(w => (w.Datum >= datVom) && (w.Datum < datBis));
      _Feiertage = new ObservableCollection<tabFeiertage>(feiertage.ToList());
    }

    private void ButtonOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var vsFeiertage = ((CollectionViewSource)(this.FindResource("vsFeiertage")));
      vsFeiertage.Source = _Feiertage;
      var vsSollStunden = ((CollectionViewSource)(this.FindResource("vsSollStunden")));
      vsSollStunden.Source = _SollStunden;
      var vsBediener = ((CollectionViewSource)(this.FindResource("vsBediener")));
      vsBediener.Source = _Auswertung.ListeBediener;
    }
  }
}
