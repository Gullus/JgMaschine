using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JgMaschineData;
using JgMaschineLib;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;

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
      var msg = "Timer gestartet";
      Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Start);
    }

    public void TimerStop()
    {
      _SteuerungsTimer.Change(Timeout.Infinite, Timeout.Infinite);
      var msg = "Timer gestopt";
      Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Stop);
    }

    public void TimerContinue()
    {
      _SteuerungsTimer.Change(new TimeSpan(0, 0, 1), new TimeSpan(0, 0, _OptDatafox.TimerIntervall));
      var msg = "Timer Continue";
      Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Resume);
    }

    public static void OnTimedEvent(object state)
    {
      var msg = "Starte Berechnungsdurchlauf";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);

      var zo = (OptionenDatafox)state;
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
            if (zo.ZaehlerDatumAktualisieren > 20)
            {
              zo.ZaehlerDatumAktualisieren = 0;
              ProgDatafox.ZeitEinstellen(zo, DateTime.Now);

              msg = "Zeit im Termal gestellt";
              Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
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
          catch (Exception f)
          {
            msg = "Fehler bei Kommunikation mit Terminal";
            throw new MyException(msg, f);
          }
          finally
          {
            ProgDatafox.DatafoxSchliessen(zo);
          }

          if (dsVomTerminal?.Count > 0)
          {
            msg = $"Es wurden {dsVomTerminal.Count} Arbeitszeiten von Terminal übertragen.";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);

            ArbeitszeitInDatenbank(Db, dsVomTerminal, standort.Id);
          }
        }
      }
      catch (Exception f)
      {
        ExceptionPolicy.HandleException(f, "Policy");
      }
    }

    public static void ArbeitszeitInDatenbank(JgModelContainer Db, List<string> ListeArbeitszeitvomTerminal, Guid IdStandort)
    {
      var msg = "";
      try
      {
        var anmeldTermial = ProgDatafox.KonvertDatafoxExport(ListeArbeitszeitvomTerminal, "MITA_");

        // Bediener zu MatchCodes laden
        var matchCodes = "'" + string.Join("','", anmeldTermial.Select(s => s.MatchCode).Distinct().ToArray()) + "'";
        var alleBediener = Db.tabBedienerSet.Where(w => matchCodes.Contains(w.MatchCode)).ToList();

        foreach (var anmeld in anmeldTermial)
        {
          var bedienerTextAusgabe = $"{anmeld.MatchCode} | {anmeld.Vorgang} | {anmeld.Datum}";
          var bediener = alleBediener.FirstOrDefault(f => f.MatchCode == anmeld.MatchCode);
          if (bediener == null)
          {
            msg = $"Bediner {bedienerTextAusgabe} nicht bekannt!";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
            continue;
          }
          else
          {
            msg = $"Bediener: {bediener.Name}, {bedienerTextAusgabe}";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
          }

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
      }
      catch (Exception f)
      {
        msg = "Fehler beim eintragen der Anmeldedaten in die Datenbank";
        throw new MyException(msg, f);
      }

      try
      {
        Db.SaveChanges();
      }
      catch (Exception f)
      {
        msg = "Fehler beim speichern der Anmeldedaten in der Datenbank";
        throw new Exception(msg, f);
      }

      msg = $"{ListeArbeitszeitvomTerminal.Count} Arbeitszeiten in DB gespeichert!";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
    }
  }
}