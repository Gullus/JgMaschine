using System;
using System.Collections.Generic;
using System.Diagnostics;
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
      Logger.Write(msg, "Service", 1, 0, TraceEventType.Information);

      var zo = (OptionenDatafox)state;

      try
      {
        using (var Db = new JgModelContainer())
        {
          try
          {
            if (zo.VerbindungsString != "")
              Db.Database.Connection.ConnectionString = zo.VerbindungsString;

            zo.ListeTerminals = Db.tabArbeitszeitTerminalSet.Where(w => !w.DatenAbgleich.Geloescht).ToList();
            msg = $"{zo.ListeTerminals.Count} Terminals eingelesen";
            Logger.Write(msg, "Service", 1, 0, TraceEventType.Verbose);

            zo.ListeStandorte = Db.tabStandortSet.Where(w => !w.DatenAbgleich.Geloescht).ToList();
            msg = $"{zo.ListeStandorte.Count} Standorte eingelesen";
            Logger.Write(msg, "Service", 1, 0, TraceEventType.Verbose);

            zo.ListeBediener = Db.tabBedienerSet.Where(w => (w.Status != EnumStatusBediener.Stillgelegt) && (!w.DatenAbgleich.Geloescht)).ToList();
            msg = $"{zo.ListeBediener.Count} Bediener eingelesen";
            Logger.Write(msg, "Service", 1, 0, TraceEventType.Verbose);

            if (zo.ListeTerminals.Any(w => w.UpdateTerminal))
            {
              try
              {
                ProgDatafox.BedienerInDatafoxDatei(zo);
                zo.UpdateBenutzerOk = true;
              }
              catch (Exception ex)
              {
                msg = $"Fehler beim erstellen der Benutzerdatei!\nGrund: {ex.Message}";
                Logger.Write(msg, "Service", 1, 0, TraceEventType.Error);
              }
            }

            zo.ZaehlerDatumAktualisieren++;
            if (zo.ZaehlerDatumAktualisieren > 10)
            {
              zo.ZaehlerDatumAktualisieren = 0;
              zo.DatumAktualisieren = true;
            }
          }
          catch (Exception f)
          {
            msg = $"Fehler beim Initialisieren der DatafoxOPtionen.";
            throw new MyException(msg, f);
          }

          foreach (var terminal in zo.ListeTerminals)
          {
            try
            {
              msg = $"Start Terminal: {terminal.Bezeichnung} / {terminal.eStandort.Bezeichnung}\nIp: {terminal.IpNummer} Port: {terminal.PortNummer}";
              Logger.Write(msg, "Service", 1, 0, TraceEventType.Information);

              zo.Terminal.IpAdresse = terminal.IpNummer;
              zo.Terminal.Portnummer = terminal.PortNummer;

              if (!Helper.IstPingOk(zo.Terminal.IpAdresse, out msg))
              {
                msg = $"Fehler bei Pingabfrage.\nGrund: {msg}";
                Logger.Write(msg, "Service", 1, 0, TraceEventType.Warning);
                continue;
              }

              List<string> DatensaetzeVomTerminal = null;

              try
              {
                var offen = ProgDatafox.DatafoxOeffnen(zo);
                if (!offen)
                {
                  msg = $"Verbindung zum Terminal konnte nicht geöffnet werden.";
                  Logger.Write(msg, "Service", 1, 0, TraceEventType.Warning);
                  continue;
                }

                // Zeit mit Termimal abgeleichem
                if (zo.DatumAktualisieren)
                {
                  if (ProgDatafox.ZeitEinstellen(zo, DateTime.Now))
                    Logger.Write("Zeit Termal gestellt", "Service", 1, 0, TraceEventType.Information);
                  else
                    Logger.Write("Zeit konnte nicht gestellt werden", "Service", 1, 0, TraceEventType.Warning);
                }

                // Kontrolle, ob Benutzer im Terminal geändert werden müssen

                if (zo.UpdateBenutzerOk && terminal.UpdateTerminal)
                {
                  terminal.UpdateTerminal = false;
                  Db.SaveChanges();
                  ProgDatafox.ListenInTerminalSchreiben(zo);
                }

                // Anmeldungen aus Terminal auslesen
                DatensaetzeVomTerminal = ProgDatafox.ListeAusTerminalAuslesen(zo);
                if (DatensaetzeVomTerminal.Count == 0)
                {
                  msg = "Keine Datensätze vorhanden.";
                  Logger.Write(msg, "Service", 1, 0, TraceEventType.Verbose);
                }
                else
                {
                  msg = $"Es wurden {DatensaetzeVomTerminal.Count} Arbeitszeiten von Terminal übertragen.";
                  Logger.Write(msg, "Service", 1, 0, TraceEventType.Information);
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
                Logger.Write(msg, "Service", 1, 0, TraceEventType.Information);
                ProgDatafox.DatafoxSchliessen(zo);
              }

              if (DatensaetzeVomTerminal.Count > 0)
              {
                try
                {
                  var anmeldungen = ProgDatafox.KonvertDatafoxImport(DatensaetzeVomTerminal, terminal.fStandort, "MITA_");
                  var dicAnmeldungen = anmeldungen.Select(s => new { Key = s.MatchCode, Value = $"{ s.GehGrund}; {s.Datum}; {s.Vorgang}" }).ToDictionary(d => d.Key, d => (object)d.Value);
                  Logger.Write("Ausgelesene Anmeldungen", "Service", 1, 0, TraceEventType.Information, "", dicAnmeldungen);

                  zo.ListeAnmeldungen.AddRange(anmeldungen);
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

          try
          {
            ArbeitszeitenInDatenbankTabelle(zo);
            zo.Db.SaveChanges();
          }
          catch (Exception f)
          {
            msg = "Fehler beim speichern der Anmeldedaten in der Datenbank";
            throw new MyException(msg, f);
          }

          msg = $"Fertig !\n{zo.ListeAnmeldungen.Count} Anmeldungen verarbeitet. Daten in DB gespeichert!";
          Logger.Write(msg, "Service", 1, 0, TraceEventType.Information);
        }
      }
      catch (Exception f)
      {
        ExceptionPolicy.HandleException(f, "Policy");
      }
    }

    public static void ArbeitszeitenInDatenbankTabelle(OptionenDatafox Optionen)
    {
      var msg = "";
      try
      {
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
            Logger.Write(msg, "Service", 1, 0, TraceEventType.Warning, "", dicAnmeldungen);
            continue;
          }

          msg = $"Bediener: {bediener.Name} erfasst.";
          Logger.Write(msg, "Service", 1, 0, TraceEventType.Information, "", dicAnmeldungen);

          foreach (var anmeld in matchcode.Anmeldungen)
          {
            if (anmeld.Vorgang == DatafoxDsImport.EnumVorgang.Komme)
            {
              var arbZeit = new tabArbeitszeit()
              {
                Id = Guid.NewGuid(),
                fBediener = bediener.Id,
                fStandort = anmeld.IdStandort,
                Anmeldung = anmeld.Datum,
                ManuelleAnmeldung = false,
                ManuelleAbmeldung = false,
              };
              Optionen.Db.tabArbeitszeitSet.Add(arbZeit);
              bediener.fAktivArbeitszeit = arbZeit.Id;
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
                var arbZeit = new tabArbeitszeit()
                {
                  Id = Guid.NewGuid(),
                  fBediener = bediener.Id,
                  fStandort = anmeld.IdStandort,
                  Abmeldung = anmeld.Datum,
                  ManuelleAnmeldung = false,
                  ManuelleAbmeldung = false,
                };
                Optionen.Db.tabArbeitszeitSet.Add(arbZeit);
              }
            }
          }
        }
      }
      catch (Exception f)
      {
        msg = "Fehler beim eintragen der Anmeldedaten in die Tabelle Arbeitszeit.";
        throw new MyException(msg, f);
      }
    }
  }
}