using System;
using System.Collections.Generic;
using System.Linq;
using JgMaschineData;
using JgZeitHelper;

namespace JgMaschineGlobalZeit
{
  public class ArbeitszeitRunden
  {
    private int _Jahr = -1;
    private int _Monat = -1;
    private JgModelContainer _Db;

    private List<tabArbeitszeitRunden> _ListeJahr;
    public List<tabArbeitszeitRunden> ListeRunden { get => _ListeJahr; }

    // Liste für Monatsdaten
    private List<tabArbeitszeitRunden> _ListeAnmeldung = null;
    private List<tabArbeitszeitRunden> _ListeAbmeldung = null;

    public int Jahr { get => _Jahr; set => DatenAktualisieren(value, _Monat); }
    public int Monat { get => _Monat; set => DatenAktualisieren(_Jahr, value); }

    public ArbeitszeitRunden(JgModelContainer Db, int AktuellesJahr)
    {
      _Db = Db;
      Jahr = AktuellesJahr;
    }

    private void DatenAktualisieren(int JahrNeu, int MonatNeu)
    {
      if ((JahrNeu != _Jahr) || (MonatNeu != _Monat))
      {
        if (JahrNeu != _Jahr)
        {
          _Jahr = JahrNeu;
          _Monat = -1;
          _ListeJahr = _Db.tabArbeitszeitRundenSet.Where(w => w.Jahr == _Jahr).ToList();
        }

        if (MonatNeu != _Monat)
        {
          _Monat = MonatNeu;
          _ListeAnmeldung = _ListeJahr.Where(w => (w.Zeitpunkt == EnumZeitpunkt.Anmeldung) && (w.Monat == _Monat) && !w.DatenAbgleich.Geloescht).ToList();
          _ListeAbmeldung = _ListeJahr.Where(w => (w.Zeitpunkt == EnumZeitpunkt.Abmeldung) && (w.Monat == _Monat) && !w.DatenAbgleich.Geloescht).ToList();
        }
      }
    }

    private DateTime GetZeitAusTabelle(List<tabArbeitszeitRunden> ListeRunden, Guid idStandort, DateTime DatumZeit)
    {
      var zeit = JgZeit.DatumInZeitMinute(DatumZeit);
      var dsGerundet = ListeRunden.FirstOrDefault(w => (w.fStandort == idStandort) && (zeit >= w.ZeitVon) && (zeit <= w.ZeitBis));
      if (dsGerundet != null)
        return DatumZeit.Date + dsGerundet.RundenArbeitszeit;
      return DatumZeit;
    }

    public DateTime? GetZeitGerundet(EnumZeitpunkt Zeitpunkt, Guid IdStandort, DateTime? DatumZeit)
    {
      if (DatumZeit != null)
      {
        DatenAktualisieren(DatumZeit.Value.Year, DatumZeit.Value.Month);
        if (Zeitpunkt == EnumZeitpunkt.Anmeldung)
          return GetZeitAusTabelle(_ListeAnmeldung, IdStandort, DatumZeit.Value);
        else
          return GetZeitAusTabelle(_ListeAbmeldung, IdStandort, DatumZeit.Value);
      }
      return null;
    }
  }
}