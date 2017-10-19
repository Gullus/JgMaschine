using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Data.Entity;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineLib.Arbeitszeit;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceArbeitszeit
{
  public class ArbeitszeitErfassen
  {
    private OptionenArbeitszeit _OptDatafox = null;
    private Timer _SteuerungsTimer;

    public ArbeitszeitErfassen(OptionenArbeitszeit OptDatafox)
    {
      _OptDatafox = OptDatafox;
    }

    public void Start()
    {
      _SteuerungsTimer = new Timer(OnTimedEvent, _OptDatafox, new TimeSpan(0, 0, 1), new TimeSpan(0, 0, _OptDatafox.AuslesIntervallTerminal));
      var msg = "Timer gestartet";
      Logger.Write(msg, "Service", 0, 0, TraceEventType.Start);
    }

    public void TimerStop()
    {
      _SteuerungsTimer.Change(Timeout.Infinite, Timeout.Infinite);
      var msg = "Timer gestoppt";
      Logger.Write(msg, "Service", 0, 0, TraceEventType.Stop);
    }

    public void TimerContinue()
    {
      _SteuerungsTimer.Change(new TimeSpan(0, 0, 1), new TimeSpan(0, 0, _OptDatafox.AuslesIntervallTerminal));
      var msg = "Timer Continue";
      Logger.Write(msg, "Service", 0, 0, TraceEventType.Resume);
    }

    public static void OnTimedEvent(object state)
    {
      var msg = "Starte Berechnungsdurchlauf";
      Logger.Write(msg, "Service", 0, 0, TraceEventType.Information);

      var Optionen = (OptionenArbeitszeit)state;
      Optionen.ListeAnmeldungen.Clear();

      try
      {
        DatenAusDatenbankLaden(Optionen);

        if (Optionen.ListeTerminals.Count == 0)
        {
          msg = "Keine Terminals in Datenbank eingetragen !";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Error);
          return;
        }

        Optionen.ZaehlerZeitErhoehen();

        // Einlesen Bediener in Terminal vorbereiten

        if (Optionen.UpdateBenutzerAusfuehren)
        {
          try
          {
            ProgDatafox.BedienerInDatafoxDatei(Optionen);
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
          msg = $"Start Terminal: {datTerminal.Bezeichnung} / {datTerminal.eStandort.Bezeichnung}\n  IP: {datTerminal.IpNummer} Port: {datTerminal.PortNummer}\n-------------------------------------------------";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Information);

          var termAktuell = new OptionenTerminal(datTerminal.IpNummer, datTerminal.PortNummer, Optionen.Terminal_TimeOut);

          if (!Helper.IstPingOk(termAktuell.IpAdresse, out msg))
          {
            Logger.Write(msg, "Service", 0, 0, TraceEventType.Warning);
            datTerminal.FehlerTerminalAusgeloest = true;
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
              datTerminal.FehlerTerminalAusgeloest = true;
              continue;
            }

            // Zeit mit Termimal abgleichem
            if (Optionen.DatumZeitAktualisieren)
            {
              if (ProgDatafox.ZeitEinstellen(termAktuell, DateTime.Now))
                Logger.Write("Zeit Terminal gestellt", "Service", 0, 0, TraceEventType.Information);
              else
                Logger.Write("Zeit konnte nicht gestellt werden", "Service", 0, 0, TraceEventType.Warning);
            }

            // Kontrolle, ob Benutzer im Terminal geändert werden müssen

            if (Optionen.UpdateBenutzerAusfuehren && datTerminal.UpdateTerminal)
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
            datTerminal.FehlerTerminalAusgeloest = true;
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

              var lsAnmeldung = anmeldungen.Select(s => $"  {s.MatchCode}   {s.GehGrund} - {s.Datum} /  {s.Vorgang}").ToArray();
              Logger.Write($"Ausgelesene Anmeldungen\n{Helper.ListeInString(lsAnmeldung)}", "Service", 0, 0, TraceEventType.Information);

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

          Optionen.ListeTerminals = Db.tabArbeitszeitTerminalSet.Where(w => !w.DatenAbgleich.Geloescht).Include(i => i.eStandort).ToList();
          msg = $"{Optionen.ListeTerminals.Count} Terminals aus DB eingelesen";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);

          if (Optionen.ErsterDurchlauf)
          {
            Optionen.ErsterDurchlauf = false;
            var anzFehlerNichtNull = Optionen.ListeTerminals.Where(w => w.AnzahlFehler > 0).ToList();
            if (anzFehlerNichtNull.Count > 0)
            {
              foreach (var nn in anzFehlerNichtNull)
                nn.AnzahlFehler = 0;
              Db.SaveChanges();
            }
          }

          Optionen.UpdateBenutzerAusfuehren = (Optionen.ListeTerminals.Any(w => w.UpdateTerminal));
          if (Optionen.UpdateBenutzerAusfuehren)
          {
            Optionen.ListeBediener = Db.tabBedienerSet.Where(w => (w.Status != EnumStatusBediener.Stillgelegt) && (!w.DatenAbgleich.Geloescht)).ToList();
            msg = $"{Optionen.ListeBediener.Count} Bediener für Update Terminal aus DB eingelesen";
            Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);
          }
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

          msg = "Beginne Eintragungen in Datenbank.";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);

          if (Optionen.ListeAnmeldungen.Count > 0)
          {

            var azImport = new ArbeitszeitImport();
            azImport.ImportStarten(Db, Optionen.ListeAnmeldungen);

            Db.SaveChanges();
            msg = $"{azImport.AnzahlAnmeldungen} Anmeldungen erfolgreich in Datenbank gespeichert.\n\n{azImport.ProtokollOk}";
            Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);

            if (azImport.ProtokollFehler != null)
            {
              msg = $"Anmeldungen ohne Benutzerzuordnung!\n\n{azImport.ProtokollFehler}";
              Logger.Write(msg, "Service", 0, 0, TraceEventType.Warning);
            }
          }

          foreach (var term in Optionen.ListeTerminals)
          {

            if ((term.TerminaldatenWurdenAktualisiert)
              || (term.FehlerTerminalAusgeloest)
              || ((!term.FehlerTerminalAusgeloest) && (term.AnzahlFehler != 0)))
            {

              Db.tabArbeitszeitTerminalSet.Attach(term);
              Db.Entry(term).State = EntityState.Modified;

              // Terminals die erfolgreich geUpdatet wurden eintragen
              if (term.TerminaldatenWurdenAktualisiert)
                term.UpdateTerminal = false;

              // Wenn Fehleranzahl erreicht wurde, Fehler anzeigen und FehlerAnzahl auf 0 setzen
              if (term.FehlerTerminalAusgeloest)
              {
                term.AnzahlFehler = (short)(term.AnzahlFehler + 1);

                if (term.AnzahlFehler >= Optionen.AnzahlBisFehlerAusloesen)
                {
                  term.AnzahlFehler = 0;
                  msg = $"Fehleranzahl Verbindungsaufbau Terminal {term.Bezeichnung} / {term.eStandort.Bezeichnung} größer {Optionen.AnzahlBisFehlerAusloesen}.";
                  Logger.Write(msg, "Service", 0, 0, TraceEventType.Error);
                }
              }
              else
              {
                if (term.AnzahlFehler != 0)
                  term.AnzahlFehler = 0;
              }

              msg = $"Terminal {term.Bezeichnung} / {term.eStandort.Bezeichnung} aktualisiert.";
              Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);
            }
          }

          Db.SaveChanges();
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