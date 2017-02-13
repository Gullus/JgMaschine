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
    private OptionenDatafox _OptDatafox = null;
    public OptionenDatafox OptDatafox { get { return _OptDatafox; } }
    private Timer _SteuerungsTimer;

    public ArbeitszeitErfassen(OptionenDatafox OptDatafox)
    {
      _OptDatafox = OptDatafox;
    }

    public void Start()
    {
      _SteuerungsTimer = new Timer(OnTimedEvent, _OptDatafox, new TimeSpan(0, 0, 1), new TimeSpan(0, 0, _OptDatafox.TimerIntervall));
    }

    public void TimerStop()
    {
      _SteuerungsTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public void TimerContinue()
    {
      _SteuerungsTimer.Change(new TimeSpan(0, 0, 1), new TimeSpan(0, 0, _OptDatafox.TimerIntervall));
    }

    private static void OnTimedEvent(object state)
    {
      var zo = (OptionenDatafox)state;
      zo.Protokoll.Set("Start Durchlauf!", Proto.ProtoArt.Kommentar);
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

          List<string> dsVomTerminal = null;

          try
          {
            ProgDatafox.DatafoxOeffnen(zo);

            // Zeit mit Termimal abgeleichem
            if (zo.ZaehlerDatumAktualisieren > 50)
            {
              zo.ZaehlerDatumAktualisieren = 0;
              ProgDatafox.ZeitEinstellen(zo, DateTime.Now);
              zo.Protokoll.Set("Zeit Datafox gestellt!", Proto.ProtoArt.Kommentar);
            }

            // Kontrolle, ob Benutzer im Termanl geändert werden müssen
            if (standort.UpdateBedienerDatafox)
            {
              standort.UpdateBedienerDatafox = false;
              DbSichern.AbgleichEintragen(standort.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
              Db.SaveChanges();

              var bediener = Db.tabBedienerSet.Where(w => (w.Status != EnumStatusBediener.Stillgelegt)).ToList();
              ProgDatafox.BedienerInDatafoxDatei(zo, bediener);
              ProgDatafox.ListenInTerminalSchreiben(zo);
            }

            // Anmeldungen aus Terminal auslesen
            dsVomTerminal = ProgDatafox.ListeAusTerminalAuslesen(zo);
          }
          catch (Exception exep)
          {
            throw new Exception(exep.Message);
          }
          finally
          {
            ProgDatafox.DatafoxSchliessen(zo);
          }

          if (dsVomTerminal?.Count > 0)
          {
            ArbeitszeitInDatenbank(Db, dsVomTerminal, standort.Id, zo.Protokoll);
            var msg = $"Es wurden {dsVomTerminal.Count} Arbeitszeiten von Terminal übertragen.";
            zo.Protokoll.Set(msg, Proto.ProtoArt.Kommentar);
          }
        }
      }
      catch (Exception f)
      {
        zo.Protokoll.Set("Fehler bei Abeitszeiterfassung !", f);
      }
    }

    public static void ArbeitszeitInDatenbank(JgModelContainer Db, List<string> ListeArbeitszeitvomTerminal, Guid IdStandort, Proto MyProtokoll)
    {
      ListeArbeitszeitvomTerminal = new List<string>();
      ListeArbeitszeitvomTerminal.Add("EVO 2.8\t1810\tG\tMITA0104\t08.02.2017 12:20:08\t0\t");

      var anmeldTermial = ProgDatafox.KonvertDatafoxExport(ListeArbeitszeitvomTerminal, "MITA_");

      var matchCodes = "'" + string.Join("','", anmeldTermial.Select(s => s.MatchCode).Distinct().ToArray()) + "'";
      var alleBediener = Db.tabBedienerSet.Where(w => matchCodes.Contains(w.MatchCode)).ToList();

      foreach (var anmeld in anmeldTermial)
      {
        var bedienerTextAusgabe = $"{anmeld.MatchCode} {anmeld.Vorgang} {anmeld.Datum}";
        var bediener = alleBediener.FirstOrDefault(f => f.MatchCode == anmeld.MatchCode);
        if (bediener == null)
        {
          MyProtokoll.Set($"Bediner {bedienerTextAusgabe} nicht bekannt!", Proto.ProtoArt.Warnung);
          continue;
        }
        else
          MyProtokoll.Set($"Bediener: {bediener.Name}, {bedienerTextAusgabe}", Proto.ProtoArt.Info);

        if (anmeld.Vorgang == DatafoxDsExport.EnumVorgang.Komme)
        {
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
          if (bediener.eAktivArbeitszeit != null)
          {
            bediener.eAktivArbeitszeit.Abmeldung = anmeld.Datum;
            bediener.eAktivArbeitszeit.ManuelleAbmeldung = false;
            DbSichern.AbgleichEintragen(bediener.eAktivArbeitszeit.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
            bediener.eAktivArbeitszeit = null;
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

      try
      {
        Db.SaveChanges();
      }
      catch (Exception f)
      {
        MyProtokoll.Set("Fehler beim speichern der Anmeldedaten in der Datenbank", f);
      }

      MyProtokoll.Set($"{ListeArbeitszeitvomTerminal.Count} Arbeitszeiten in DB gespeichert!", Proto.ProtoArt.Kommentar);
    }
  }
}