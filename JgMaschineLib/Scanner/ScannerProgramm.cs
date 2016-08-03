using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using JgMaschineData;

namespace JgMaschineLib.Scanner
{
  public class ScannerProgramm
  {
    private bool _ProtokllAnzeigen = true;

    public string ScannnerAdresse { get; set; } = "";
    public int ScannerPortNummer { get; set; } = 0;
    public string DbVerbindungsString { get; set; } = "";

    public string EvgPfadProduktionsListe { get; set; } = "";
    public string EvgDateiProduktionsAuftrag { get; set; } = "";
    public string ProgressPfadProduktionsListe { get; set; } = "";
    public string ServerTextAnmeldung { get; set; } = "";

    public bool ProtokollAnzeigen { get; set; } = false;

    private class MaschinenDaten
    {
      public string BvbsString { get; set; }
      public tabMaschine Maschine { get; set; }
    }

    private DataLogicScanner _DlScanner;

    public ScannerProgramm()
    { }

    public void Start()
    {
      Protokoll($"Scanneradresse: {ScannnerAdresse}");
      Protokoll($"Scannerport {ScannerPortNummer}");
      Protokoll($"Db Verbindungsstring {DbVerbindungsString}");

      using (var db = new JgModelContainer())
      {
        db.Database.Connection.ConnectionString = DbVerbindungsString;
        db.tabStandortSet.FirstOrDefault();
      }

      _DlScanner = new DataLogicScanner(ScannnerAdresse, ScannerPortNummer, ScannerCommunication, ProtokollAnzeigen);
      _DlScanner.Start();
    }

    public void Close()
    {
      _DlScanner.Close();
    }

    private void Protokoll(string Ausgabe, params object[] Werte)
    {
      if (_ProtokllAnzeigen)
        Console.WriteLine(Ausgabe, Werte);
    }

    private tabMaschine SucheMaschine(JgModelContainer Db, DataLogicScannerText e)
    {
      var maschine = Db.tabMaschineSet.FirstOrDefault(f => f.ScannerNummer == e.ScannerKennung);
      if (maschine == null)
        e.FehlerAusgabe("Scanner nicht regist.!", " ", "Adresse: " + e.ScannerKennung);
      else
        e.MitDisplay = maschine.ScannerMitDisplay;
      return maschine;
    }

    private void DatenAnMaschineSenden(tabMaschine Maschine, string SendString)
    {
      if ((Maschine.ProtokollName == EnumProtokollName.Handbiegung) || (string.IsNullOrWhiteSpace(Maschine.MaschineAdresse)))
        return;

      switch (Maschine.ProtokollName)
      {
        case EnumProtokollName.Schnell:
          if (Maschine.MaschinePortnummer != null)
          {
            var datenAnMaschine = Task.Factory.StartNew((mDaten) =>
            {
              var md = (MaschinenDaten)mDaten;
              try
              {
                Protokoll($"Verbindung mit: {md.Maschine.MaschinenName} auf Port: {md.Maschine.MaschinePortnummer}.");
                using (var client = new TcpClient(md.Maschine.MaschineAdresse, (int)md.Maschine.MaschinePortnummer))
                {
                  client.NoDelay = true;
                  client.SendTimeout = 1000;
                  client.ReceiveTimeout = 1000;

                  Protokoll("Verbindung zur Maschine hergestellt.");
                  var nwStr = client.GetStream();
                  var buffer = Encoding.ASCII.GetBytes(md.BvbsString + Convert.ToChar(13) + Convert.ToChar(10));
                  nwStr.Write(buffer, 0, buffer.Length);
                  Protokoll("Daten zu Maschine geschickt. Warte auf Antwort");

                  // Auf Antwort von Maschine warten 

                  buffer = new byte[client.ReceiveBufferSize];
                  try
                  {
                    int anzEmpfang = nwStr.Read(buffer, 0, (int)client.ReceiveBufferSize);
                    var empfangen = Encoding.ASCII.GetString(buffer, 0, anzEmpfang);
                    Protokoll($"Daten von Maschine: {empfangen}.");
                    AntwortMaschineSchnellAuswerten(empfangen);
                  }
                  catch (Exception f)
                  {
                    Protokoll($"Keine Daten von Maschine empfangen.\nGrund: {f.Message}");
                  }

                  client.Close();
                  Protokoll("Verbindung geschlossen.");
                }
              }
              catch (Exception f)
              {
                var fehlerText = $"Fehler beim senden der Bvbs Daten an die Maschine: {Maschine.MaschinenName}\nDaten: {md.BvbsString}\nFehler: {f.Message}";
                Protokoll(fehlerText);
                JgMaschineLib.Helper.InWinProtokoll($"Fehler beim senden der Bvbs Daten an die Maschine: {Maschine.MaschinenName}\nDaten: {md.BvbsString}\nFehler: {f.Message}", System.Diagnostics.EventLogEntryType.Error);
              }
            }, new MaschinenDaten() { BvbsString = SendString, Maschine = Maschine });
          }
          break;

        case EnumProtokollName.Evg:

          var datenAnEvg = Task.Factory.StartNew((mDaten) =>
          {
            var md = (MaschinenDaten)mDaten;

            var datAuftrag = "Auftrag1.txt";
            var datProdListe = string.Format(@"\\{0}\{1}\{2}", md.Maschine.MaschineAdresse, EvgPfadProduktionsListe, datAuftrag);

            // Produktionsliste schreiben 

            try
            {
              File.WriteAllText(datProdListe, md.BvbsString);
            }
            catch (Exception f)
            {
              string s = $"Fehler beim schreiben der EVG Produktionsliste in die Maschine {md.Maschine.MaschinenName}\nDatei: {datProdListe}.\nGrund: {f.Message}";
              Protokoll(s);
              Helper.InWinProtokoll(s, System.Diagnostics.EventLogEntryType.Error);
            }

            // Produktionsauftrag

            var datProtAuftrag = string.Format(@"\\{0}\{1}", md.Maschine.MaschineAdresse, EvgDateiProduktionsAuftrag);
            try
            {
              File.WriteAllText(datProtAuftrag, datAuftrag);
            }
            catch (Exception f)
            {
              string s = $"Fehler beim schreiben des EVG Produktionsauftrages in die Maschine {md.Maschine.MaschinenName}\nDatei: {datProtAuftrag}.\nGrund: {f.Message}";
              Protokoll(s);
              Helper.InWinProtokoll(s, System.Diagnostics.EventLogEntryType.Error);
            }

          }, new MaschinenDaten() { BvbsString = SendString, Maschine = Maschine });
          break;

        case EnumProtokollName.Progress:

          var datenAnProgress = Task.Factory.StartNew((mDaten) =>
            {
              var md = (MaschinenDaten)mDaten;
              var dat = string.Format(@"\\{0}\{1}\{2}", md.Maschine.MaschineAdresse, ProgressPfadProduktionsListe, "Auftrag.txt");

              // Produktionsliste schreiben 
              try
              {
                Protokoll($"Bauteil in Datei: {dat} schreiben.");
                File.WriteAllText(dat, md.BvbsString, Encoding.UTF8);
                Protokoll("Datei geschrieben !");
              }
              catch (Exception f)
              {
                string s = $"Fehler beim schreiben der Progress Produktionsliste Maschine: {Maschine.MaschinenName} \nDatei: {dat}.\nGrund: {f.Message}";
                Protokoll(s);
                Helper.InWinProtokoll(s, System.Diagnostics.EventLogEntryType.Error);
              }
            }, new MaschinenDaten() { BvbsString = SendString, Maschine = Maschine });

          break;
      }
    }

    private void AntwortMaschineSchnellAuswerten(string Antwort)
    {
      if ((Antwort.Length >= 3) && (Antwort[0] == Convert.ToChar(15)))
      {
        var dat = Helper.StartVerzeichnis() + @"FehlerCode\JgMaschineFehlerSchnell.txt";
        if (!File.Exists(dat))
          Protokoll($"Fehlerdatei: {dat} existiert nicht.");
        else
        {
          try
          {
            string nummer = Antwort.Substring(1, 2);

            var zeilen = File.ReadAllLines(dat);
            foreach (var zeile in zeilen)
            {
              if (zeile.Substring(0, 2) == nummer)
                Protokoll($"Fehler: {zeile}");
            }
          }
          catch (Exception f)
          {
            Protokoll($"Fehler beim auslesen der Fehlerdatei.\nGrund: {f.Message}");
          }
        }
      }
    }

    private void Bf2dEintragen(JgModelContainer Db, tabMaschine Maschine, DataLogicScannerText e)
    {
      var btNeu = new JgMaschineLib.Stahl.BvbsDatenaustausch(e.ScannerVorgangScan + e.ScannerKoerper);

      Protokoll("Maschine: {0}", Maschine.MaschinenName);
      Protokoll("Project:  {0}  Anzahl: {1} Gewicht: {2}", btNeu.ProjektNummer, btNeu.Anzahl, btNeu.Gewicht * 1000);
      Protokoll(" ");

      if (Maschine.eAktivBauteil != null)
      {
        var letztesBt = Maschine.eAktivBauteil;
        letztesBt.DatumEnde = DateTime.Now;
        DbSichern.AbgleichEintragen(letztesBt.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);

        if ((letztesBt.NummerBauteil == btNeu.ProjektNummer) && (letztesBt.BtDurchmesser == btNeu.Durchmesser)
          && (letztesBt.BtGewicht == btNeu.GewichtInKg) && (letztesBt.BtLaenge == btNeu.Laenge))
        {
          Protokoll("Bauteil erledigt");
          e.SendeText(" ", " - Bauteil erledigt -");
          Maschine.eAktivBauteil = null;
          DbSichern.DsSichern<tabMaschine>(Db, Maschine, EnumStatusDatenabgleich.Geaendert);
          return;
        }
      }

      DatenAnMaschineSenden(Maschine, btNeu.BvbsString);

      var btInDatenBank = Db.tabBauteilSet.FirstOrDefault(f => ((f.fMaschine == Maschine.Id) && (f.BvbsCode == btNeu.BvbsString)));

      if (btInDatenBank != null)
      {
        Protokoll($"Bauteil bereits am {btInDatenBank.DatumStart.ToString("dd.MM.yy HH:mm")} gefertigt.");
        e.FehlerAusgabe("Bauteil bereits am", btInDatenBank.DatumStart.ToString("dd.MM.yy HH:mm"), "gefertigt.");
      }
      else
      {
        e.SendeText(" ", "  - Bauteil O K -");

        // Berechnen des Gewichtes, da aus JgData falsche erte kommen

        decimal gewichtProMeter = 0.00m;
        switch (Convert.ToInt32(btNeu.Durchmesser))
        {
          case 4: gewichtProMeter = 0.099m; break;
          case 5: gewichtProMeter = 0.154m; break;
          case 6: gewichtProMeter = 0.222m; break;
          case 7: gewichtProMeter = 0.302m; break;
          case 8: gewichtProMeter = 0.395m; break;
          case 9: gewichtProMeter = 0.499m; break;
          case 10: gewichtProMeter = 0.617m; break;
          case 11: gewichtProMeter = 0.746m; break;
          case 12: gewichtProMeter = 0.888m; break;
          case 14: gewichtProMeter = 1.21m; break;
          case 16: gewichtProMeter = 1.58m; break;
          case 20: gewichtProMeter = 2.47m; break;
          case 25: gewichtProMeter = 3.85m; break;
          case 28: gewichtProMeter = 4.83m; break;
          case 32: gewichtProMeter = 6.31m; break;
          case 40: gewichtProMeter = 9.86m; break;
        }

        var btNeuErstellt = new tabBauteil()
        {
          Id = Guid.NewGuid(),
          eMaschine = Maschine,
          BtAnzahl = Convert.ToInt32(btNeu.Anzahl),
          BtDurchmesser = Convert.ToInt32(btNeu.Durchmesser),
          //todo Falsches Gewicht in Bvbs Code -> BtGewicht = btNeu.GewichtInKg,
          BtGewicht = Convert.ToInt32(gewichtProMeter * Convert.ToInt32(btNeu.Laenge)),
          BtLaenge = Convert.ToInt32(btNeu.Laenge),
          BtAnzahlBiegungen = (byte)btNeu.ListeGeometrie.Count,
          BvbsCode = btNeu.BvbsString,
          NummerBauteil = btNeu.ProjektNummer,

          IstHandeingabe = false,
          IstVorfertigung = Maschine.IstStangenschneider && ((byte)btNeu.ListeGeometrie.Count == 0),

          IdStahlPosition = 1,
          IdStahlBauteil = 1,

          AnzahlBediener = (byte)Maschine.sAktiveAnmeldungen.Count(),

          DatumStart = DateTime.Now,
          DatumEnde = DateTime.Now.AddMinutes(10)
        };

        foreach (var bed in Maschine.sAktiveAnmeldungen)
          btNeuErstellt.sBediener.Add(bed.eBediener);

        Maschine.eAktivBauteil = btNeuErstellt;
        Protokoll("Neues Bauteil erstellt.");
        DbSichern.DsSichern<tabBauteil>(Db, btNeuErstellt, EnumStatusDatenabgleich.Neu);
      }
    }

    private void BedienerEintragen(JgModelContainer Db, tabMaschine Maschine, tabBediener Bediener, DataLogicScannerText e)
    {
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
            Protokoll($"Bediener {Bediener.Name} bereits an Maschine {Maschine.MaschinenName} angemeldet.");
            return;
          }
          else // Wenn nicht, an der angmeldeten Maschine abmelden.
          {
            BedienerVonMaschineAbmelden(Maschine, Bediener, anmeldungVorhanden);
            BedienerReparaturAbmelden(Maschine, Bediener);
          }
        }

        e.SendeText(" ", "- A N M E L D U N G -", Maschine.MaschinenName, Bediener.Name);
        Protokoll($"Bediener {Bediener.Name} an Maschine {Maschine.MaschinenName} anmelden.");
         
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
        DbSichern.AbgleichEintragen(anmeldungNeu.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
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
          DbSichern.AbgleichEintragen(anmeldungReparatur.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
          Db.tabAnmeldungMaschineSet.Add(anmeldungNeu);
        }

        Db.SaveChanges();
      }

      else // Abmeldung
      {
        if (anmeldungVorhanden == null)
          e.FehlerAusgabe(" ", "Sie sind an keiner", "Maschine angemeldet !");
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
      Protokoll($"Bediener {Bediener.Name} an Maschine {Maschine.MaschinenName} abmelden.");

      anmeldungVorhanden.Abmeldung = DateTime.Now;
      anmeldungVorhanden.ManuelleAbmeldung = false;
      anmeldungVorhanden.eAktivMaschine = null;

      DbSichern.AbgleichEintragen(anmeldungVorhanden.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
    }

    private static void BedienerReparaturAbmelden(tabMaschine Maschine, tabBediener Bediener)
    {
      if (Maschine.eAktivReparatur != null)
      {
        var anmeldungReparatur = Maschine.eAktivReparatur.sAnmeldungen.Where(w => w.IstAktiv).FirstOrDefault(f => f.Id == Bediener.Id);
        if (anmeldungReparatur != null)
        {
          anmeldungReparatur.Abmeldung = DateTime.Now;
          DbSichern.AbgleichEintragen(anmeldungReparatur.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    private void ProgrammeEintragen(JgModelContainer Db, tabMaschine Maschine, DataLogicScannerText e)
    {
      var reparatur = Maschine.eAktivReparatur;

      if (e.VorgangProgramm == DataLogicScanner.VorgangProgram.REPA_ENDE)
      {
        if (reparatur == null)
          e.FehlerAusgabe("Kein Reparatur", "angemeldet !");
        else
        {
          e.SendeText("Vorgang beendet: ", $"- {reparatur.Ereignis} -", $"MA: {Maschine.MaschinenName}");

          reparatur.VorgangEnde = DateTime.Now;
          DbSichern.AbgleichEintragen(reparatur.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
          Maschine.eAktivReparatur = null;
          DbSichern.AbgleichEintragen(Maschine.DatenAbgleich, EnumStatusDatenabgleich.Abgeglichen);

          var aktiveBediener = reparatur.sAnmeldungen.Where(w => w.IstAktiv).ToList();
          foreach (var bediener in aktiveBediener)
          {
            bediener.Abmeldung = reparatur.VorgangEnde;
            DbSichern.AbgleichEintragen(bediener.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
          }

          Db.SaveChanges();
        };
      }
      else // Anmeldung einer Reparatur
      { 
        if (reparatur != null)
          e.FehlerAusgabe("Vorgang:", $"- {reparatur.Ereignis} -", "bereits angemeldet !");
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
            case DataLogicScanner.VorgangProgram.REPASTART: reparatur.Ereignis = EnumReperaturEreignis.Reparatur; break;
            case DataLogicScanner.VorgangProgram.WARTSTART: reparatur.Ereignis = EnumReperaturEreignis.Wartung; break;
            case DataLogicScanner.VorgangProgram.COILSTART: reparatur.Ereignis = EnumReperaturEreignis.Coilwechsel; break;
          }

          if (e.VorgangProgramm == DataLogicScanner.VorgangProgram.COILSTART)
            reparatur.CoilwechselAnzahl = Convert.ToByte(e.ScannerKoerper);
          DbSichern.AbgleichEintragen(reparatur.DatenAbgleich, EnumStatusDatenabgleich.Neu);
          Db.tabReparaturSet.Add(reparatur);

          e.SendeText("Beginn Vorgang: ", $"- {reparatur.Ereignis} -", $"MA: {Maschine.MaschinenName}");

          Maschine.eAktivReparatur = reparatur;
          DbSichern.AbgleichEintragen(Maschine.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);

          foreach(var anmeldungen in Maschine.sAktiveAnmeldungen)
          {
            var anmeldungReparatur = new tabAnmeldungReparatur()
            {
              Id = Guid.NewGuid(),
              Anmeldung = reparatur.VorgangBeginn,
              fBediener = anmeldungen.fBediener,
              fReparatur = reparatur.Id,
            };
            DbSichern.AbgleichEintragen(anmeldungReparatur.DatenAbgleich, EnumStatusDatenabgleich.Neu);
            Db.tabAnmeldungReparaturSet.Add(anmeldungReparatur);
          }

          Db.SaveChanges();
        }
      }

      Protokoll($"Maschine: {Maschine.MaschinenName} - Vorgang: {e.VorgangProgramm}");
    }

    private void ScannerCommunication(DataLogicScannerText e)
    {
      #region Sannertext in Datei eintragen

      //var dat = (@"C:\Users\jg\Desktop\ScannerBeispiele.txt");
      //using (StreamWriter sw = File.AppendText(dat))
      //{
      //  sw.WriteLine(e.TextEmpfangen);
      //}

      #endregion

      if (e.TextEmpfangen.Contains(ServerTextAnmeldung))
      {
        Protokoll("Anmeldung Server: {0}", e.TextEmpfangen);
        return;
      }

      using (var db = new JgModelContainer())
      {
        var maschine = SucheMaschine(db, e);

        if (maschine == null)
          Protokoll("Maschine nicht gefunden.");
        else
        {
          switch (e.VorgangScan)
          {
            case DataLogicScanner.VorgangScanner.BF2D:

              if (maschine.sAktiveAnmeldungen.FirstOrDefault() == null)
              {
                e.FehlerAusgabe(" ", "Es ist keine Bediener", "angemeldet !");
                Protokoll("Kein Bediener angemeldet.");
              }
              else
                Bf2dEintragen(db, maschine, e);
              break;

            case DataLogicScanner.VorgangScanner.MITA:

              var mitarb = db.tabBedienerSet.FirstOrDefault(f => f.MatchCode == e.ScannerKoerper);
              if (mitarb == null)
              {
                e.FehlerAusgabe("Bediener unbekannt!", " ", $"MA: {maschine.MaschinenName}", e.ScannerKoerper);
                Protokoll($"Bediener unbekannt MA: {maschine.MaschinenName} Anz. Zeichen: {e.ScannerKoerper.Length} Matchcode: {e.ScannerKoerper} ");
              }
              else
              {
                BedienerEintragen(db, maschine, mitarb, e);
                Protokoll($"Bediener MA: {maschine.MaschinenName} Anz. Zeichen: {e.ScannerKoerper.Length} Matchcode: {e.ScannerKoerper} ");
              }
              break;

            case DataLogicScanner.VorgangScanner.PROG:
              ProgrammeEintragen(db, maschine, e);
              break;

            default:
              Protokoll("Vorgang Scanner unbekannt.");
              break;
          }
        }
      }
    }
  }
}