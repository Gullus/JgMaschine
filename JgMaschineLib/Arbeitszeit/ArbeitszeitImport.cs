using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using JgMaschineData;

namespace JgMaschineLib.Arbeitszeit
{
  public class ArbeitszeitImport
  {
    private StringBuilder _StringBuilderOk = new StringBuilder();
    public string ProtokollOk { get { return _StringBuilderOk.ToString(); } }

    private StringBuilder _StringBuilderFehler = new StringBuilder();
    public string ProtokollFehler { get { return  (_StringBuilderFehler.ToString() == "") ? null : _StringBuilderFehler.ToString(); } }

    public int AnzahlAnmeldungen = 0;

    public ArbeitszeitImport()
    { }

    public void ImportStarten(JgModelContainer Db, List<ArbeitszeitImportDaten> NeuListeArbeitszeiten)
    {
      var lArbeitzeiten = from z in NeuListeArbeitszeiten
                          group z by z.MatchCode into erg
                          select new { MatchCode = erg.Key, Anmeldungen = erg.OrderBy(o => o.Datum) };

      var listeMatchCodes = lArbeitzeiten.Select(s => s.MatchCode).ToArray();
      var listeBediener = Db.tabBedienerSet.Where(w => listeMatchCodes.Contains(w.MatchCode))
        .Include(i => i.eAktivArbeitszeit)
        .ToDictionary(t => t.MatchCode, t => t);
      var listeStandorte = Db.tabStandortSet.ToDictionary(t => t.Id, t => t);

      foreach (var mc in lArbeitzeiten)
      {
        var bediener = listeBediener[mc.MatchCode];

        if (bediener == null)
        {
          _StringBuilderFehler.AppendLine($" {mc.MatchCode}");
          _StringBuilderFehler.AppendLine($" ---------------------------------------------");
          foreach (var anm in mc.Anmeldungen)
            _StringBuilderFehler.AppendLine($" -> {anm.Datum} {anm.Vorgang}");
          continue;
        }

        _StringBuilderOk.AppendLine($" {mc.MatchCode} - {bediener.Name}");
        _StringBuilderOk.AppendLine($" ---------------------------------------------");

        foreach (var anm in mc.Anmeldungen)
        {
          AnzahlAnmeldungen++;

          var standort = listeStandorte[anm.IdStandort ?? bediener.fStandort];
          var ss = (standort == null) ? " - " : standort.Bezeichnung;
          _StringBuilderOk.AppendLine($" -> {anm.Datum} - {anm.Vorgang} / {ss} ");

          if (anm.Vorgang == ArbeitszeitImportDaten.EnumVorgang.Komme)
          {
            var arbZeit = ArbeitszeitErstellen(bediener.Id, standort.Id, anm.Datum, anm.Baustelle);
            Db.tabArbeitszeitSet.Add(arbZeit);
            bediener.eAktivArbeitszeit = arbZeit;
          }
          else if (anm.Vorgang == ArbeitszeitImportDaten.EnumVorgang.Gehen)
          {
            if (bediener.eAktivArbeitszeit != null)
            {
              bediener.eAktivArbeitszeit.Abmeldung = anm.Datum;
              bediener.eAktivArbeitszeit.ManuelleAbmeldung = false;
              bediener.eAktivArbeitszeit = null;
            }
            else
            {
              var arbZeit = ArbeitszeitErstellen(bediener.Id, standort.Id, anm.Datum, anm.Baustelle);
              Db.tabArbeitszeitSet.Add(arbZeit);
            }
          }
        }
      }
    }

    private tabArbeitszeit ArbeitszeitErstellen(Guid IdBediener, Guid IdStandort, DateTime DatumAnmeldung, string VonBaustelle)
    {
      return new tabArbeitszeit()
      {
        Id = Guid.NewGuid(),
        fBediener = IdBediener,
        fStandort = IdStandort,
        Anmeldung = DatumAnmeldung,
        ManuelleAnmeldung = false,
        ManuelleAbmeldung = false,
        Baustelle = VonBaustelle
      };
    }
  }
}
