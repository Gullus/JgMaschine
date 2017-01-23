using System;
using System.Linq;
using System.Windows;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineGlobalZeit.Fenster
{
  public partial class FormArbeitszeitRunden : Window
  {
    private bool _Erstellen;
    private AnmeldungAuswertung _Auswertung;

    public tabArbeitszeitRunden AktuellerWert { get { return (tabArbeitszeitRunden)gridRunden.DataContext; } }

    public FormArbeitszeitRunden(AnmeldungAuswertung Auswertung, tabArbeitszeitRunden WertBearbeiten, bool Erstellen)
    {
      InitializeComponent();

      _Erstellen = Erstellen;
      _Auswertung = Auswertung;

      tabArbeitszeitRunden runden = WertBearbeiten;
      if (Erstellen)
      {
        runden = new tabArbeitszeitRunden()
        {
          Jahr = _Auswertung.Jahr,
          ZeitVon = (WertBearbeiten != null) ? WertBearbeiten.ZeitVon : new TimeSpan(5, 30, 0),
          ZeitBis = (WertBearbeiten != null) ? WertBearbeiten.ZeitBis : new TimeSpan(6, 0, 0),
          RundenAufZeit = (WertBearbeiten != null) ? WertBearbeiten.RundenAufZeit : new TimeSpan(6, 0, 0),

          Monat = (WertBearbeiten != null) ? (byte)WertBearbeiten.Monat : (byte)1,
          eStandort = WertBearbeiten?.eStandort,
        };
      }
      gridRunden.DataContext = runden;

      cmbNiederlassung.ItemsSource = Auswertung.Db.tabStandortSet.Where(w => (!w.DatenAbgleich.Geloescht)).OrderBy(o => o.Bezeichnung).ToList();
      cmbMonat.ItemsSource = Enum.GetValues(typeof(ZeitHelper.Monate));
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      var runden = AktuellerWert;
      if (_Erstellen)
      {
        runden.Id = Guid.NewGuid();
        DbSichern.AbgleichEintragen(runden.DatenAbgleich, EnumStatusDatenabgleich.Neu);

        _Auswertung.Db.tabArbeitszeitRundenSet.Add(runden);
        _Auswertung.ListeRunden.Add(runden);
      }

      _Auswertung.Db.SaveChanges();
      this.DialogResult = true;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      if (!(this.DialogResult ?? false) && (!_Erstellen))
        _Auswertung.Db.Entry(AktuellerWert).Reload();
    }
  }
}
