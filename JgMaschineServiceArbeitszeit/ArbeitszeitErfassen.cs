using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data.Entity;
using System.Threading;
using JgMaschineData;
using JgMaschineLib;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceArbeitszeit
{
  public class ArbeitszeitErfassen
  {
    private OptionenArbeitszeit _OptDatafox = null;
    public OptionenArbeitszeit OptDatafox { get { return _OptDatafox; } }
    private Timer _SteuerungsTimer;

    public ArbeitszeitErfassen(OptionenArbeitszeit OptDatafox)
    {
      _OptDatafox = OptDatafox;
    }

    public void Start()
    {
      _SteuerungsTimer = new Timer(OnTimedEvent, _OptDatafox, new TimeSpan(0, 0, 1), new TimeSpan(0, 0, _OptDatafox.TimerIntervall));
      var msg = "Timer gestartet";
      Logger.Write(msg, "Service", 0, 0, TraceEventType.Start);
    }

    public void TimerStop()
    {
      _SteuerungsTimer.Change(Timeout.Infinite, Timeout.Infinite);
      var msg = "Timer gestopt";
      Logger.Write(msg, "Service", 0, 0, TraceEventType.Stop);
    }

    public void TimerContinue()
    {
      _SteuerungsTimer.Change(new TimeSpan(0, 0, 1), new TimeSpan(0, 0, _OptDatafox.TimerIntervall));
      var msg = "Timer Continue";
      Logger.Write(msg, "Service", 0, 0, TraceEventType.Resume);
    }

    public static void OnTimedEvent(object state)
    {
      var msg = "Starte Berechnungsdurchlauf";
      Logger.Write(msg, "Service", 0, 0, TraceEventType.Information);

      var Optionen = (OptionenArbeitszeit)state;

      try
      {
        DatenAusDatenbankLaden(Optionen);

        if (Optionen.ListeTerminals.Count == 0)
          throw new Exception("Keine Terminaleinträge vorhanden !");

        Optionen.ZaehlerDatumAktualisieren++;
        if (Optionen.ZaehlerDatumAktualisieren > 10)
        {
          Optionen.ZaehlerDatumAktualisieren = 0;
          Optionen.DatumZeitAktualisieren = true;
        }

        // Einlesen Bediener in Terminal vorbereiten

        Optionen.UpdateBenutzerOk = false;
        if (Optionen.ListeTerminals.Any(w => w.UpdateTerminal))
        {
          try
          {
            ProgDatafox.BedienerInDatafoxDatei(Optionen);
            Optionen.UpdateBenutzerOk = true;
          }
          catch (Exception ex)
          {
            msg = $"Fehler beim erstellen der Benutzerdatei!\nGrund: {ex.Message}";
            Logger.Write(msg, "Service", 0, 0, TraceEventType.Error);
          }
        }

        TerminaldatenEinUndAuslesen(Optionen);

        ArbeitszeitenInDatenbank(Optionen);

        msg = $"Fertig !\n{Optionen.ListeAnmeldungen.Count} Anmeldungen verarbeitet";
        Logger.Write(msg, "Service", 0, 0, TraceEventType.Information);
      }
      catch (Exception f)
      {
        ExceptionPolicy.HandleException(f, "Policy");
      }
    }

    public static void TerminaldatenEinUndAuslesen(OptionenArbeitszeit Optionen)
    {
      string msg;
      foreach (var datTerminal in Optionen.ListeTerminals)
      {
        try
        {
          msg = $"Start Terminal: {datTerminal.Bezeichnung} / {datTerminal.eStandort.Bezeichnung}\nIp: {datTerminal.IpNummer} Port: {datTerminal.PortNummer}";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Information);

          var termAktuell = new OptionenTerminal(datTerminal.IpNummer, datTerminal.PortNummer, Optionen.Terminal_TimeOut);

          if (!Helper.IstPingOk(termAktuell.IpAdresse, out msg))
          {
            Logger.Write(msg, "Service", 0, 0, TraceEventType.Warning);
            continue;
          }

          List<string> DatensaetzeVomTerminal = null;

          try
          {
            var offen = ProgDatafox.DatafoxOeffnen(termAktuell);
            if (!offen)
            {
              msg = $"Verbindung zum Terminal konnte nicht geöffnet werden.";
              Logger.Write(msg, "Service", 0, 0, TraceEventType.Warning);
              continue;
            }

            // Zeit mit Termimal abgeleichem
            if (Optionen.DatumZeitAktualisieren)
            {
              if (ProgDatafox.ZeitEinstellen(termAktuell, DateTime.Now))
                Logger.Write("Zeit Termal gestellt", "Service", 0, 0, TraceEventType.Information);
              else
                Logger.Write("Zeit konnte nicht gestellt werden", "Service", 0, 0, TraceEventType.Warning);
            }

            // Kontrolle, ob Benutzer im Terminal geändert werden müssen

            if (Optionen.UpdateBenutzerOk && datTerminal.UpdateTerminal)
            {
              datTerminal.TerminaldatenWurdenAktualisiert = true;
              ProgDatafox.ListenInTerminalSchreiben(termAktuell, Optionen.PfadUpdateBediener);
            }

            // Anmeldungen aus Terminal auslesen
            DatensaetzeVomTerminal = ProgDatafox.ListeAusTerminalAuslesen(termAktuell);
            if (DatensaetzeVomTerminal.Count == 0)
            {
              msg = "Keine Datensätze vorhanden.";
              Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);
            }
            else
            {
              msg = $"Es wurden {DatensaetzeVomTerminal.Count} Arbeitszeiten von Terminal übertragen.";
              Logger.Write(msg, "Service", 0, 0, TraceEventType.Information);
            }
          }
          catch (Exception f)
          {
            msg = "Fehler bei Kommunikation mit Terminal";
            throw new MyException(msg, f);
          }
          finally
          {
            msg = "Verbindung Terminal schliesen.";
            Logger.Write(msg, "Service", 0, 0, TraceEventType.Information);
            ProgDatafox.DatafoxSchliessen(termAktuell);
          }

          if (DatensaetzeVomTerminal.Count > 0)
          {
            try
            {
              var anmeldungen = ProgDatafox.KonvertDatafoxImport(DatensaetzeVomTerminal, datTerminal.fStandort, "MITA_");
              var dicAnmeldungen = anmeldungen.Select(s => new { Key = s.MatchCode, Value = $"{ s.GehGrund}; {s.Datum}; {s.Vorgang}" }).ToDictionary(d => d.Key, d => (object)d.Value);
              Logger.Write("Ausgelesene Anmeldungen", "Service", 0, 0, TraceEventType.Information, "", dicAnmeldungen);

              Optionen.ListeAnmeldungen.AddRange(anmeldungen);
            }
            catch (Exception f)
            {
              msg = "Fehler beim Konvertieren der Daten.";
              throw new Exception(msg, f);
            }
          }
        }
        catch (Exception f)
        {
          ExceptionPolicy.HandleException(f, "Policy");
        }
      }
    }

    public static void DatenAusDatenbankLaden(OptionenArbeitszeit Optionen)
    {
      string msg;
      try
      {
        using (var Db = new JgModelContainer())
        {
          if (Optionen.VerbindungsString != "")
            Db.Database.Connection.ConnectionString = Optionen.VerbindungsString;

          msg = $"Datenbank öffnen -> {Db.Database.Connection.ConnectionString}";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Information);

          Optionen.ListeTerminals = Db.tabArbeitszeitTerminalSet.Where(w => !w.DatenAbgleich.Geloescht).ToList();
          msg = $"{Optionen.ListeTerminals.Count} Terminals aus DB eingelesen";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);

          Optionen.ListeStandorte = Db.tabStandortSet.Where(w => !w.DatenAbgleich.Geloescht).ToList();
          msg = $"{Optionen.ListeStandorte.Count} Standorte aus DB eingelesen";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);

          Optionen.ListeBediener = Db.tabBedienerSet.Where(w => (w.Status != EnumStatusBediener.Stillgelegt) && (!w.DatenAbgleich.Geloescht))
            .Include(i => i.eAktivArbeitszeit)
            .ToList();
          msg = $"{Optionen.ListeBediener.Count} Bediener aus DB eingelesen";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);
        }
        msg = $"Daten aus Datenbank erfolgreich geladen.";
        Logger.Write(msg, "Service", 0, 0, TraceEventType.Information);
      }
      catch (Exception f)
      {
        msg = $"Fehler beim Laden der Daten aus der Datenbank.";
        throw new MyException(msg, f);
      }
    }

    public static tabArbeitszeit ArbeitszeitErstellen(Guid IdBediener, Guid IdStandort, DateTime DatumAnmeldung)
    {
      return new tabArbeitszeit()
      {
        Id = Guid.NewGuid(),
        fBediener = IdBediener,
        fStandort = IdStandort,
        Anmeldung = DatumAnmeldung,
        ManuelleAnmeldung = false,
        ManuelleAbmeldung = false,
      };
    }

    public static void ArbeitszeitenInDatenbank(OptionenArbeitszeit Optionen)
    {
      var msg = "";
      try
      {
        using (var Db = new JgModelContainer())
        {
          if (Optionen.VerbindungsString != "")
            Db.Database.Connection.ConnectionString = Optionen.VerbindungsString;

          var anmeldungen = from z in Optionen.ListeAnmeldungen
                            group z by z.MatchCode into erg
                            select new { Matchcode = erg.Key, Anmeldungen = erg.OrderBy(o => o.Datum) };

          foreach (var matchcode in anmeldungen)
          {
            var bediener = Optionen.ListeBediener.FirstOrDefault(f => f.MatchCode == matchcode.Matchcode);
            var dicAnmeldungen = matchcode.Anmeldungen.Select(s => new { Key = s.Vorgang.ToString(), Value = $"{Optionen.GetStandort(s.IdStandort)} - { s.GehGrund}; {s.Datum}; {s.Vorgang}" }).ToDictionary(d => d.Key, d => (object)d.Value);

            if (bediener == null)
            {
              msg = $"Bediener {matchcode.Matchcode} nicht bekannt";
              Logger.Write(msg, "Service", 0, 0, TraceEventType.Warning, "", dicAnmeldungen);
              continue;
            }

            msg = $"Bediener: {bediener.Name} erfasst.";
            Logger.Write(msg, "Service", 0, 0, TraceEventType.Information, "", dicAnmeldungen);

            foreach (var anmeld in matchcode.Anmeldungen)
            {
              if (anmeld.Vorgang == DatafoxDsImport.EnumVorgang.Komme)
              {
                var arbZeit = ArbeitszeitErstellen(bediener.Id, anmeld.IdStandort, anmeld.Datum);
                Db.tabArbeitszeitSet.Add(arbZeit);
                bediener.eAktivArbeitszeit = arbZeit;
              }
              else if (anmeld.Vorgang == DatafoxDsImport.EnumVorgang.Gehen)
              {
                if (bediener.eAktivArbeitszeit != null)
                {
                  bediener.eAktivArbeitszeit.Abmeldung = anmeld.Datum;
                  bediener.eAktivArbeitszeit.ManuelleAbmeldung = false;
                  bediener.eAktivArbeitszeit = null;
                }
                else
                {
                  var arbZeit = ArbeitszeitErstellen(bediener.Id, anmeld.IdStandort, anmeld.Datum);
                  Db.tabArbeitszeitSet.Add(arbZeit);
                }
              }
            }
          }

          Db.SaveChanges();
          msg = $"Anmeldungen in Datenbank gespeichert.";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);

          // Terminals die erfolgreich geUpdatet eintragen

          var listeTerminals = Optionen.ListeTerminals.Where(w => w.TerminaldatenWurdenAktualisiert).ToList();
          foreach (var tu in listeTerminals)
          {
            Db.tabArbeitszeitTerminalSet.Attach(tu);
            tu.UpdateTerminal = false;
          }

          Db.SaveChanges();
          msg = $"{listeTerminals.Count} Terminal(s) in Datenbank gespeichert";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);

        }
      }
      catch (Exception f)
      {
        msg = "Fehler beim eintragen der Anmeldedaten in die Datenbank.";
        throw new MyException(msg, f);
      }
    }
  }
}