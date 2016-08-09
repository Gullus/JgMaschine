using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineDatafoxLib
{
  public class ArbeitszeitErfassen
  {
    private ZeitsteuerungDatafox _ZeitsteuerungOptionen = null;
    public ZeitsteuerungDatafox ZeitsteuerungOptionen { get { return _ZeitsteuerungOptionen; } }
    private Timer _SteuerungsTimer;

    public ArbeitszeitErfassen(ZeitsteuerungDatafox ZeitsteuerungOptionen)
    {
      _ZeitsteuerungOptionen = ZeitsteuerungOptionen;
      _SteuerungsTimer = new Timer(OnTimedEvent, _ZeitsteuerungOptionen, 1000, _ZeitsteuerungOptionen.TimerIntervall);
    }

    public void TimerStop()
    {
      _SteuerungsTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public void TimerContinue()
    {
      _SteuerungsTimer.Change(1000, _ZeitsteuerungOptionen.TimerIntervall);
    }

    private static void OnTimedEvent(object state)
    {
      var zo = (ZeitsteuerungDatafox)state;
      zo.ZaehlerDatumAktualisieren++;

      try
      {
        using (var Db = new JgModelContainer())
        {
          if (zo.VerbindungsString != "")
            Db.Database.Connection.ConnectionString = zo.VerbindungsString;

          var standort = Db.tabStandortSet.FirstOrDefault(f => f.Bezeichnung == zo.Standort);
          if (standort == null)
            throw new Exception($"Es wurde kein oder ein falscher Standort bei der Arbeitszeit eingegeben!\nStandort: {zo.Standort}");
          var idStandort = standort.Id;

          ProgDatafox.DatafoxOeffnen(zo);

          // Zeit mit Termimal abgeleichem
          if (zo.ZaehlerDatumAktualisieren > 50)
          {
            zo.ZaehlerDatumAktualisieren = 0;
            ProgDatafox.ZeitEinstellen(zo, DateTime.Now);
            zo.Protokoll.Set("Zeit Datafox gestellt!", Proto.ProtoArt.Info);
          }

          // Kontrolle, ob Benutzer im Termanl geändert werden müssen
          if (standort.UpdateBedienerDatafox)
          {
            standort.UpdateBedienerDatafox = false;
            DbSichern.AbgleichEintragen(standort.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
            Db.SaveChanges();

            var bediener = Db.tabBedienerSet.Where(w => (w.Status != EnumStatusBediener.Stillgelegt) && (w.fStandort == idStandort)).ToList();
            ProgDatafox.BedienerInDatafoxDatei(zo, bediener);
            ProgDatafox.ListenInTerminalSchreiben(zo);
          }

          // Anmeldungen aus Terminal auslesen
          var dsVomTerminal = ProgDatafox.ListeAusTerminalAuslesen(zo);

          ProgDatafox.DatafoxSchliessen(zo);

          if (dsVomTerminal.Count > 0)
            ArbeitszeitInDatenbank(Db, dsVomTerminal, standort.Id, zo.Protokoll);
        }
      }
      catch (Exception f)
      {
        zo.Protokoll.Set("Fehler bei Abeitszeiterfassung !", f);
      }
    }

    private static void ArbeitszeitInDatenbank(JgModelContainer Db, List<string> ListeArbeitszeitvomTerminal, Guid IdStandort, Proto MyProtokoll)
    {
      var anmeldTermial = ProgDatafox.KonvertDatafoxExport(ListeArbeitszeitvomTerminal, "MITA_");

      var idisAktiveAnmeldungen = Db.tabBedienerSet.Where(w => w.fAktivArbeitszeit != null).Select(s => s.fAktivArbeitszeit).ToArray();
      var aktAnmeldungen = Db.tabArbeitszeitSet.Where(w => idisAktiveAnmeldungen.Contains(w.Id)).ToList();

      var matchCodes = "'" + string.Join("','", anmeldTermial.Select(s => s.MatchCode).Distinct().ToArray()) + "'";
      var alleBediener = Db.tabBedienerSet.Where(w => matchCodes.Contains(w.MatchCode)).ToList();

      foreach (var anmeld in anmeldTermial)
      {
        var bediener = alleBediener.FirstOrDefault(f => f.MatchCode == anmeld.MatchCode);
        if (bediener == null)
        {
          MyProtokoll.Set($"Bediner {anmeld.MatchCode} aus Terminal nicht bekannt!", Proto.ProtoArt.Warnung);
          continue;
        }

        var arbeitzeitVorhanden = aktAnmeldungen.FirstOrDefault(f => f.eBediener.MatchCode == anmeld.MatchCode);

        if (anmeld.Vorgang == DatafoxDsExport.EnumVorgang.Komme)
        {
          if (arbeitzeitVorhanden != null)
          {
            if (anmeld.Datum.AddHours(1) < DateTime.Now)
              continue;
          }

          var arbZeit = new tabArbeitszeit()
          {
            Id = Guid.NewGuid(),
            fBediener = bediener.Id,
            fStandort = IdStandort,
            Anmeldung = anmeld.Datum,
            ManuelleAnmeldung = false,
            ManuelleAbmeldung = false,
          };
          DbSichern.AbgleichEintragen(arbZeit.DatenAbgleich, EnumStatusDatenabgleich.Neu);
          Db.tabArbeitszeitSet.Add(arbZeit);
          bediener.fAktivArbeitszeit = arbZeit.Id;
          DbSichern.AbgleichEintragen(bediener.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
        else if (anmeld.Vorgang == DatafoxDsExport.EnumVorgang.Gehen)
        {
          if (arbeitzeitVorhanden != null)
          {
            arbeitzeitVorhanden.Abmeldung = anmeld.Datum;
            arbeitzeitVorhanden.ManuelleAbmeldung = true;
            DbSichern.AbgleichEintragen(arbeitzeitVorhanden.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
            bediener.fAktivArbeitszeit = null;
            DbSichern.AbgleichEintragen(bediener.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
          }
          else
          {
            var arbZeit = new tabArbeitszeit()
            {
              Id = Guid.NewGuid(),
              fBediener = bediener.Id,
              fStandort = IdStandort,
              Abmeldung = anmeld.Datum,
              ManuelleAnmeldung = false,
              ManuelleAbmeldung = false,
            };
            DbSichern.AbgleichEintragen(arbZeit.DatenAbgleich, EnumStatusDatenabgleich.Neu);
            Db.tabArbeitszeitSet.Add(arbZeit);
          }
        }
      }

      Db.SaveChanges();
      MyProtokoll.Set($"{ListeArbeitszeitvomTerminal.Count} Arbeitszeiten ins System übertragen!", Proto.ProtoArt.Info);
    }
  }
}

