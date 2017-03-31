using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using JgMaschineData;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineLib.Scanner
{
  public class ScannerProgramm
  {
    private ScannerOptionen _Optionen = null;
    public ScannerOptionen Optionen { get { return _Optionen; } }

    private class MaschinenDaten
    {
      public string BvbsString { get; set; }
      public tabMaschine Maschine { get; set; }
    }

    private DataLogicScanner _DlScanner;

    public ScannerProgramm(ScannerOptionen NeuOptionen)
    {
      _Optionen = NeuOptionen;
    }

    public void Start()
    {
      var msg = $"Start Scannerprogramm\nScanneradresse: {_Optionen.CradleIpAdresse}\nScannerport: {_Optionen.CradlePortNummer}";
      Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Information);

      try
      {
        using (var db = new JgModelContainer())
        {
          if (_Optionen.DbVerbindungsString != "")
            db.Database.Connection.ConnectionString = _Optionen.DbVerbindungsString;

          try
          {
            var con = new SqlConnection(db.Database.Connection.ConnectionString);
            con.Open();
            msg = $"Datenbankverbindung OK.\nConnection: {con.ConnectionString}";
            Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Information);
            con.Close();
          }
          catch (Exception f)
          {
            msg = $"Verbindungsaufbau zur Datenbank {db.Database.Connection.ConnectionString} schlug fehl.\nGrund: {f.Message}";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Critical);
            return;
          }

          try
          {
            db.tabStandortSet.FirstOrDefault();
            msg = "Abrfrage Entity Framework OK.";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
          }
          catch (Exception f)
          {
            msg = $"1. Entity Framworkabfrage schlug fehl.\nGrund: {f.Message}";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Critical);
            return;
          }
        }
      }
      catch (Exception f)
      {
        msg = $"Initialisierung des Entitiy Framework fehlgeschlagen !";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Critical);
        ExceptionPolicy.HandleException(f, "Policy");
      }

      msg = "Starte Scanner.";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);

      _DlScanner = new DataLogicScanner(_Optionen, ScannerKommunikation);
      _DlScanner.Start();
    }

    public void Close()
    {
      _DlScanner.Close();
    }

    private tabMaschine SucheMaschine(JgModelContainer Db, DataLogicScannerText e)
    {
      var maschine = Db.tabMaschineSet.FirstOrDefault(f => f.ScannerNummer == e.ScannerKennung);
      if (maschine == null)
      {
        e.FehlerAusgabe("Scanner nicht regist.!", " ", "Adresse: " + e.ScannerKennung);

        var msg = $"Scanner nicht gefunden. Scanner: {e.ScannerKennung}";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
      }
      else
        e.MitDisplay = maschine.ScannerMitDisplay;

      return maschine;
    }

    private void DatenAnMaschineSenden(tabMaschine Maschine, string SendString)
    {
      if ((Maschine.MaschinenArt == EnumMaschinenArt.Handbiegung) || (string.IsNullOrWhiteSpace(Maschine.MaschineAdresse)))
        return;

      string msg = "";

      switch (Maschine.MaschinenArt)
      {
        case EnumMaschinenArt.Schnell:
          if (Maschine.MaschinePortnummer != null)
          {
            var datenAnMaschine = Task.Factory.StartNew((mDaten) =>
            {
              var md = (MaschinenDaten)mDaten;
              try
              {
                msg = $"Verbindung mit: {md.Maschine.MaschinenName} auf Port: {md.Maschine.MaschinePortnummer}.";
                Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
                using (var client = new TcpClient(md.Maschine.MaschineAdresse, (int)md.Maschine.MaschinePortnummer))
                {
                  client.NoDelay = true;
                  client.SendTimeout = 1000;
                  client.ReceiveTimeout = 1000;

                  var nwStr = client.GetStream();
                  var buffer = Encoding.ASCII.GetBytes(md.BvbsString + Convert.ToChar(13) + Convert.ToChar(10));
                  nwStr.Write(buffer, 0, buffer.Length);

                  // Auf Antwort von Maschine warten 

                  buffer = new byte[client.ReceiveBufferSize];
                  try
                  {
                    int anzEmpfang = nwStr.Read(buffer, 0, (int)client.ReceiveBufferSize);
                    var empfangen = Encoding.ASCII.GetString(buffer, 0, anzEmpfang);
                    var antwortMaschine = AntwortMaschineSchnellAuswerten(empfangen);
                    if (antwortMaschine != "")
                      throw new Exception($"Rückmeldung von Maschine {md.Maschine.MaschinenName} ist {antwortMaschine}!");
                  }
                  catch (Exception f)
                  {
                    throw new Exception($"Keine Rückantwort von Maschine ! {f.Message}");
                  }

                  client.Close();

                  msg = $"Verbindung mit: {md.Maschine.MaschinenName} abgeschlossen.";
                  Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
                }
              }
              catch (Exception f)
              {
                msg = $"Fehler beim senden der Bvbs Daten an die Maschine: {Maschine.MaschinenName}\nDaten: {md.BvbsString}\nGrund: {f.Message}";
                Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Error);
              }
            }, new MaschinenDaten() { BvbsString = SendString, Maschine = Maschine });
          }
          break;

        case EnumMaschinenArt.Evg:

          var datenAnEvg = Task.Factory.StartNew((mDaten) =>
          {
            var md = (MaschinenDaten)mDaten;

            var datAuftrag = "Auftrag1.txt";
            var datProdListe = string.Format(@"\\{0}\{1}\{2}", md.Maschine.MaschineAdresse, _Optionen.EvgPfadProduktionsListe, datAuftrag);

            // Produktionsliste schreiben 

            try
            {
              File.WriteAllText(datProdListe, md.BvbsString);
            }
            catch (Exception f)
            {
              msg = $"Fehler beim schreiben der EVG Produktionsliste in die Maschine {md.Maschine.MaschinenName}!\nDatei: {datProdListe}.\nGrund: {f.Message}";
              Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
            }

            // Produktionsauftrag

            var datProtAuftrag = string.Format(@"\\{0}\{1}", md.Maschine.MaschineAdresse, _Optionen.EvgDateiProduktionsAuftrag);
            try
            {
              File.WriteAllText(datProtAuftrag, datAuftrag);
            }
            catch (Exception f)
            {
              msg = $"Fehler beim schreiben des EVG Produktionsauftrages in die Maschine {md.Maschine.MaschinenName}!\nDatei: {datProtAuftrag}.\nGrund: {f.Message}";
              Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
            }

          }, new MaschinenDaten() { BvbsString = SendString, Maschine = Maschine });
          break;

        case EnumMaschinenArt.Progress:

          var datenAnProgress = Task.Factory.StartNew((mDaten) =>
            {
              var md = (MaschinenDaten)mDaten;
              var datei = string.Format(@"\\{0}\{1}\{2}", md.Maschine.MaschineAdresse, _Optionen.ProgressPfadProduktionsListe, "Auftrag.txt");

              // Produktionsliste schreiben 
              try
              {
                File.WriteAllText(datei, md.BvbsString, Encoding.UTF8);
              }
              catch (Exception f)
              {
                msg = $"Fehler beim schreiben der Progress Produktionsliste Maschine: {Maschine.MaschinenName} \nDatei: {datei}.\nGrund: {f.Source}";
                Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
              }
            }, new MaschinenDaten() { BvbsString = SendString, Maschine = Maschine });

          break;
      }
    }

    private string AntwortMaschineSchnellAuswerten(string Antwort)
    {
      string msg = "";

      if ((Antwort.Length >= 3) && (Antwort[0] == Convert.ToChar(15)))
      {
        var dat = Helper.StartVerzeichnis() + @"FehlerCode\JgMaschineFehlerSchnell.txt";
        if (!File.Exists(dat))
        {
          msg = $"Fehlerdatei: {dat} für Maschine 'Schnell' existiert nicht.";
          Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
        }
        else
        {
          try
          {
            string nummer = Antwort.Substring(1, 2);

            var zeilen = File.ReadAllLines(dat);
            foreach (var zeile in zeilen)
            {
              if (zeile.Substring(0, 2) == nummer)
                return zeile;
            }
          }
          catch (Exception f)
          {
            msg = $"Fehler beim auslesen der Fehlerdatei Firma Schnell.\nGrund: {f.Message}";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
          }
        }
      }

      return "";
    }

    private void Bf2dEintragen(JgModelContainer Db, tabMaschine Maschine, DataLogicScannerText e)
    {
      string msg = "";
      BvbsDatenaustausch btNeu = null;

      try
      {
        btNeu = new BvbsDatenaustausch(e.ScannerVorgangScan + e.ScannerKoerper, false);
      }
      catch (Exception f)
      {
        e.FehlerAusgabe("BVBS Code konnte nicht", "gelesen werden !");
        msg = $"Bvbs Code konnte icht gelesen Werden.\nGrund: {f.Message}";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
        return;
      }

      if (Maschine.eAktivBauteil != null)
      {
        var letztesBt = Maschine.eAktivBauteil;
        letztesBt.DatumEnde = DateTime.Now;

        if (btNeu.BvbsString == letztesBt.BvbsCode)
        {
          msg = $"BauteilMaschine: {Maschine.MaschinenName}\nProject: {btNeu.ProjektNummer} erledigt";
          Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);

          e.SendeText(" ", " - Bauteil erledigt -");
          Maschine.eAktivBauteil = null;
          return;
        }
      }

      DatenAnMaschineSenden(Maschine, btNeu.BvbsString);

      var btInDatenBank = Db.tabBauteilSet.FirstOrDefault(f => ((f.fMaschine == Maschine.Id) && (f.BvbsCode == btNeu.BvbsString)));

      if (btInDatenBank != null)
      {
        msg = $"Bauteil an Maschine {Maschine.MaschinenName} bereits am {btInDatenBank.DatumStart.ToString("dd.MM.yy HH:mm")} gefertigt.";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);

        e.FehlerAusgabe("Bauteil bereits am", btInDatenBank.DatumStart.ToString("dd.MM.yy HH:mm"), "gefertigt.");
      }
      else
      {
        e.SendeText(" ", "  - Bauteil O K -");

        var bvbs = new BvbsDatenaustausch(btNeu.BvbsString, false);

        var btNeuErstellt = new tabBauteil()
        {
          Id = Guid.NewGuid(),
          DatumStart = DateTime.Now,
          
          eMaschine = Maschine,

          BvbsCode = btNeu.BvbsString,
          BvbsDaten = bvbs,

          BtGewicht = StahlGewichte.GetGewichtKg((int)btNeu.Durchmesser, (int)btNeu.Laenge) * (int)btNeu.Anzahl,

          IstHandeingabe = false,
          IstVorfertigung = Maschine.IstStangenschneider && ((byte)btNeu.ListeGeometrie.Count == 0),

          AnzahlBediener = (byte)Maschine.sAktiveAnmeldungen.Count(),
        };

        foreach (var bed in Maschine.sAktiveAnmeldungen)
          btNeuErstellt.sBediener.Add(bed.eBediener);

        Maschine.eAktivBauteil = btNeuErstellt;

        msg = $"BT erstellt. Maschine: {Maschine.MaschinenName}\n  Project: {btNeu.ProjektNummer} Anzahl: {btNeu.Anzahl} Gewicht: {btNeuErstellt.BtGewicht}";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
      }
    }

    private void BedienerEintragen(JgModelContainer Db, tabMaschine Maschine, tabBediener Bediener, DataLogicScannerText e)
    {
      string msg = "";

      var anmeldungVorhanden = Maschine.sAktiveAnmeldungen.FirstOrDefault(f => (f.fBediener == Bediener.Id));
      if (anmeldungVorhanden == null)
        anmeldungVorhanden = Db.tabAnmeldungMaschineSet.FirstOrDefault(f => (f.fBediener == Bediener.Id) && (f.fAktivMaschine != null));

      if (e.VorgangProgramm == DataLogicScanner.VorgangProgram.ANMELDUNG)
      {
        if (anmeldungVorhanden != null)
        {
          if (anmeldungVorhanden.eMaschine == Maschine)
          {
            e.FehlerAusgabe("Sie sind bereits an", $"MA: {Maschine.MaschinenName}", "angemeldet !");

            msg = $"Bediener {Bediener.Name} ist bereits an Maschine {Maschine.MaschinenName} angemeldet.";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
            return;
          }
          else // Wenn nicht, an der angmeldeten Maschine abmelden.
          {
            BedienerVonMaschineAbmelden(Maschine, Bediener, anmeldungVorhanden);
            BedienerReparaturAbmelden(Maschine, Bediener);
          }
        }

        e.SendeText(" ", "- A N M E L D U N G -", Maschine.MaschinenName, Bediener.Name);

        var anmeldungNeu = new tabAnmeldungMaschine()
        {
          Id = Guid.NewGuid(),
          Anmeldung = DateTime.Now,
          eBediener = Bediener,
          eMaschine = Maschine,
          ManuelleAnmeldung = false,
          ManuelleAbmeldung = false,
          eAktivMaschine = Maschine
        };
        Db.tabAnmeldungMaschineSet.Add(anmeldungNeu);

        // Wenn eine Reparatur läuft, an dieser anmelden

        if (Maschine.eAktivReparatur != null)
        {
          var anmeldungReparatur = new tabAnmeldungReparatur()
          {
            Id = Guid.NewGuid(),
            Anmeldung = DateTime.Now,
            fBediener = Bediener.Id,
            fReparatur = (Guid)Maschine.fAktivReparatur,
          };
          Db.tabAnmeldungReparaturSet.Add(anmeldungReparatur);
        }

        Db.SaveChanges();

        msg = $"Bediener {Bediener.Name} an Maschine {Maschine.MaschinenName} angemeldet.";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
      }
      else // Abmeldung
      {
        if (anmeldungVorhanden == null)
          e.FehlerAusgabe(" ", "Sie sind an dieser", "Maschine nicht angemeldet !");
        else
        {
          e.SendeText(" ", "- A B M E L D U N G -", " ", Bediener.Name, $"MA: {Maschine.MaschinenName}");

          BedienerVonMaschineAbmelden(Maschine, Bediener, anmeldungVorhanden);
          BedienerReparaturAbmelden(Maschine, Bediener);

          Db.SaveChanges();
        }
      }
    }

    private void BedienerVonMaschineAbmelden(tabMaschine Maschine, tabBediener Bediener, tabAnmeldungMaschine anmeldungVorhanden)
    {
      anmeldungVorhanden.Abmeldung = DateTime.Now;
      anmeldungVorhanden.ManuelleAbmeldung = false;
      anmeldungVorhanden.eAktivMaschine = null;

      var msg = $"Bediener {Bediener.Name} an Maschine {Maschine.MaschinenName} abgemeldet.";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
    }

    private static void BedienerReparaturAbmelden(tabMaschine Maschine, tabBediener Bediener)
    {
      if (Maschine.eAktivReparatur != null)
      {
        var anmeldungReparatur = Maschine.eAktivReparatur.sAnmeldungen.Where(w => w.IstAktiv).FirstOrDefault(f => f.eBediener == Bediener);
        if (anmeldungReparatur != null)
          anmeldungReparatur.Abmeldung = DateTime.Now;
      }
    }

    private void ProgrammeEintragen(JgModelContainer Db, tabMaschine Maschine, DataLogicScannerText e)
    {
      string msg = "";
      var reparatur = Maschine.eAktivReparatur;

      if (e.VorgangProgramm == DataLogicScanner.VorgangProgram.REPA_ENDE)
      {
        if (reparatur == null)
          e.FehlerAusgabe("Kein Reparatur", "angemeldet !");
        else
        {
          e.SendeText("Vorgang beendet: ", $"- {reparatur.Vorgang} -", $"MA: {Maschine.MaschinenName}");

          reparatur.VorgangEnde = DateTime.Now;
          Maschine.eAktivReparatur = null;

          var aktiveBediener = reparatur.sAnmeldungen.Where(w => w.IstAktiv).ToList();
          foreach (var bediener in aktiveBediener)
            bediener.Abmeldung = reparatur.VorgangEnde;

          Db.SaveChanges();

          msg = $"Reparatur {reparatur.Vorgang} an Maschine {Maschine.MaschinenName} beendet.";
          Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
        };
      }
      else // Anmeldung einer Reparatur
      {
        if (reparatur != null)
          e.FehlerAusgabe("Vorgang:", $"- {reparatur.Vorgang} -", "bereits angemeldet !");
        else
        {
          reparatur = new tabReparatur()
          {
            Id = Guid.NewGuid(),
            VorgangBeginn = DateTime.Now,
            fMaschine = Maschine.Id,
          };

          switch (e.VorgangProgramm)
          {
            case DataLogicScanner.VorgangProgram.REPASTART: reparatur.Vorgang = EnumReperaturVorgang.Reparatur; break;
            case DataLogicScanner.VorgangProgram.WARTSTART: reparatur.Vorgang = EnumReperaturVorgang.Wartung; break;
            case DataLogicScanner.VorgangProgram.COILSTART: reparatur.Vorgang = EnumReperaturVorgang.Coilwechsel; break;
          }

          if (e.VorgangProgramm == DataLogicScanner.VorgangProgram.COILSTART)
            reparatur.CoilwechselAnzahl = Convert.ToByte(e.ScannerKoerper);
          Db.tabReparaturSet.Add(reparatur);

          e.SendeText("Beginn Vorgang: ", $"- {reparatur.Vorgang} -", $"MA: {Maschine.MaschinenName}");

          Maschine.eAktivReparatur = reparatur;

          foreach (var anmeldungen in Maschine.sAktiveAnmeldungen)
          {
            var anmeldungReparatur = new tabAnmeldungReparatur()
            {
              Id = Guid.NewGuid(),
              Anmeldung = reparatur.VorgangBeginn,
              fBediener = anmeldungen.fBediener,
              fReparatur = reparatur.Id,
            };
            Db.tabAnmeldungReparaturSet.Add(anmeldungReparatur);
          }

          Db.SaveChanges();

          msg = $"Reparatur {reparatur.Vorgang} an Maschine {Maschine.MaschinenName} gestartet.";
          Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
        }
      }
    }

    private void ScannerKommunikation(DataLogicScannerText e)
    {
      // Sannertext in Datei eintragen

      Logger.Write(e.TextEmpfangen, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);

      string msg = "";

      if (e.TextEmpfangen.ToUpper().Contains(_Optionen.CradleTextAnmeldung.ToUpper()))
      {
        msg = $"Anmeldung Cradle, Antwort: {e.TextEmpfangen}";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
        return;
      }

      using (var db = new JgModelContainer())
      {
        var maschine = SucheMaschine(db, e);

        if (maschine != null)
        {
          switch (e.VorgangScan)
          {
            case DataLogicScanner.VorgangScanner.BF2D:

              if (maschine.sAktiveAnmeldungen.FirstOrDefault() == null)
              {
                e.FehlerAusgabe(" ", "Es ist keine Bediener", "angemeldet !");
              }
              else
                Bf2dEintragen(db, maschine, e);
              break;

            case DataLogicScanner.VorgangScanner.MITA:

              var mitarb = db.tabBedienerSet.FirstOrDefault(f => f.MatchCode == e.ScannerKoerper);
              if (mitarb == null)
              {
                e.FehlerAusgabe("Bediener unbekannt!", $"MA: {maschine.MaschinenName}", e.ScannerKoerper);
                msg = $"Bediener unbekannt: {e.ScannerKoerper}";
                Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
              }
              else
                BedienerEintragen(db, maschine, mitarb, e);

              break;

            case DataLogicScanner.VorgangScanner.PROG:
              ProgrammeEintragen(db, maschine, e);
              break;
          }
        }
      }
    }
  }
}