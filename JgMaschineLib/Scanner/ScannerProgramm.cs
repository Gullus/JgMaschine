using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
      public JgMaschineData.tabMaschine Maschine { get; set; }
    }

    private DataLogicScanner _DlScanner;
    private DataLogicScanner.VorgangProgram[] _IstStart = { DataLogicScanner.VorgangProgram.WARTSTART, DataLogicScanner.VorgangProgram.REPASTART, DataLogicScanner.VorgangProgram.COILSTART };

    public ScannerProgramm()
    { }

    public void Start()
    {
      Protokoll($"Scanneradresse: {ScannnerAdresse}");
      Protokoll($"Scannerport {ScannerPortNummer}");
      Protokoll($"Db Verbindungsstring {DbVerbindungsString}");

      using (var db = new JgMaschineData.JgModelContainer())
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

    private JgMaschineData.tabMaschine SucheMaschine(JgMaschineData.JgModelContainer Db, DataLogicScannerText e)
    {
      var maschine = Db.tabMaschineSet.FirstOrDefault(f => f.ScannerNummer == e.ScannerKennung);
      if (maschine == null)
        e.FehlerAusgabe("Scanner nicht regist.!", " ", "Adresse: " + e.ScannerKennung);
      else
        e.MitDisplay = maschine.ScannerMitDisplay;
      return maschine;
    }

    private void DatenAnMaschineSenden(JgMaschineData.tabMaschine Maschine, string SendString)
    {
      if ((Maschine.ProtokollName == JgMaschineData.EnumProtokollName.Handbiegung) || (string.IsNullOrWhiteSpace(Maschine.MaschineAdresse)))
        return;

      switch (Maschine.ProtokollName)
      {
        case JgMaschineData.EnumProtokollName.Schnell:
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

        case JgMaschineData.EnumProtokollName.Evg:

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

        case JgMaschineData.EnumProtokollName.Progress:

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

    private void Bf2dEintragen(JgMaschineData.JgModelContainer Db, JgMaschineData.tabMaschine Maschine, DataLogicScannerText e)
    {
      var btNeu = new JgMaschineLib.Stahl.BvbsDatenaustausch(e.ScannerVorgangScan + e.ScannerKoerper);

      Protokoll("Maschine: {0}", Maschine.MaschinenName);
      Protokoll("Project:  {0}  Anzahl: {1} Gewicht: {2}", btNeu.ProjektNummer, btNeu.Anzahl, btNeu.Gewicht * 1000);
      Protokoll(" ");

      if (Maschine.eLetztesBauteil != null)
      {
        var letztesBt = Maschine.eLetztesBauteil;
        letztesBt.DatumEnde = DateTime.Now;
        DbSichern.AbgleichEintragen(letztesBt.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);

        if ((letztesBt.NummerBauteil == btNeu.ProjektNummer) && (letztesBt.BtDurchmesser == btNeu.Durchmesser)
          && (letztesBt.BtGewicht == btNeu.GewichtInKg) && (letztesBt.BtLaenge == btNeu.Laenge))
        {
          Protokoll("Bauteil erledigt");
          e.SendeText(" ", " - Bauteil erledigt -");
          Maschine.eLetztesBauteil = null;
          DbSichern.DsSichern<JgMaschineData.tabMaschine>(Db, Maschine, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
          return;
        }
      }

      DatenAnMaschineSenden(Maschine, btNeu.BvbsString);

      var btInDatenBank = Db.tabBauteilSet.FirstOrDefault(f => ((f.fMaschine == Maschine.Id)
        && (f.NummerBauteil == btNeu.ProjektNummer)
        && (f.BtDurchmesser == btNeu.Durchmesser)
        && (f.BtGewicht == btNeu.GewichtInKg)
        && (f.BtLaenge == btNeu.Laenge)));

      if (btInDatenBank != null)
      {
        Protokoll($"Bauteil bereits am {btInDatenBank.DatumStart.ToString("dd.MM.yy HH:mm")} gefertigt.");
        e.FehlerAusgabe("Bauteil bereits am", btInDatenBank.DatumStart.ToString("dd.MM.yy HH:mm"), "gefertigt.");
      }
      else
      {
        e.SendeText(" ", "  - Bauteil O K -");

        var btNeuErstellt = new JgMaschineData.tabBauteil()
        {
          Id = Guid.NewGuid(),
          eMaschine = Maschine,
          BtAnzahl = Convert.ToInt32(btNeu.Anzahl),
          BtDurchmesser = Convert.ToInt32(btNeu.Durchmesser),
          BtGewicht = btNeu.GewichtInKg,
          BtLaenge = Convert.ToInt32(btNeu.Laenge),
          NummerBauteil = btNeu.ProjektNummer,

          IstHandeingabe = false,
          IstVorfertigung = Maschine.IstStangenschneider && ((byte)btNeu.ListeGeometrie.Count == 0),

          IdStahlPosition = 1,
          IdStahlBauteil = 1,

          AnzahlBediener = (byte)Maschine.sAktuelleBediener.Count(),
          AnzahlBiegungen = (byte)btNeu.ListeGeometrie.Count,

          DatumStart = DateTime.Now,
          DatumEnde = DateTime.Now.AddMinutes(10)
        };

        foreach (var bed in Maschine.sAktuelleBediener)
          btNeuErstellt.sBediener.Add(bed);

        Maschine.eLetztesBauteil = btNeuErstellt;
        Protokoll("Neues Bauteil erstellt.");
        DbSichern.DsSichern<JgMaschineData.tabBauteil>(Db, btNeuErstellt, JgMaschineData.EnumStatusDatenabgleich.Neu);
      }
    }

    private void BedienerEintragen(JgMaschineData.JgModelContainer Db, JgMaschineData.tabMaschine Maschine, JgMaschineData.tabBediener Bediener, DataLogicScannerText e)
    {
      var anmeldung = Db.tabAnmeldungMaschineSet.FirstOrDefault(f => (f.fBediener == Bediener.Id) && f.IstAktiv);

      if (e.VorgangProgramm == DataLogicScanner.VorgangProgram.ANMELDUNG)
      {
        if (anmeldung != null)
        {
          if (anmeldung.eMaschine == Maschine)
          {
            Protokoll($"Bediener {Bediener.Name} bereits an Maschine {Maschine.MaschinenName} angemeldet.");
            e.FehlerAusgabe("Sie sind bereits an", $"MA: {Maschine.MaschinenName}", "angemeldet !");
            return;
          }
          else // Wenn nicht, an der angmeldeten Maschine abmelden.
          {
            Protokoll($"Bediener {Bediener.Name} an Maschine {Maschine.MaschinenName} abmelden.");
            Protokoll("Benutzer abmelden.");
            anmeldung.Abmeldung = DateTime.Now;
            anmeldung.ManuelleAbmeldung = false;
            anmeldung.IstAktiv = false;
            DbSichern.AbgleichEintragen(anmeldung.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
            Db.SaveChanges();
          }
        }

        e.SendeText(" ", "- A N M E L D U N G -", Maschine.MaschinenName, Bediener.Name);

        Bediener.eAktuelleAnmeldungMaschine = Maschine;
        Protokoll($"Bediener {Bediener.Name} an Maschine {Maschine.MaschinenName} anmelden.");

        anmeldung = new JgMaschineData.tabAnmeldungMaschine()
        {
          Id = Guid.NewGuid(),
          Anmeldung = DateTime.Now,
          eBediener = Bediener,
          eMaschine = Maschine,
          IstAktiv = true,
          ManuelleAnmeldung = false,
          Abmeldung = DateTime.Now,
          ManuelleAbmeldung = false
        };

        DbSichern.DsSichern<JgMaschineData.tabAnmeldungMaschine>(Db, anmeldung, JgMaschineData.EnumStatusDatenabgleich.Neu);
      }
      else // Abmeldung
      {
        if (anmeldung == null)
          e.FehlerAusgabe(" ", "Sie sind an keiner", "Maschine angemeldet !");
        else
        {
          e.SendeText(" ", "- A B M E L D U N G -", " ", Bediener.Name, $"MA: {Maschine.MaschinenName}");

          Bediener.eAktuelleAnmeldungMaschine = null;

          anmeldung.Abmeldung = DateTime.Now;
          anmeldung.ManuelleAbmeldung = false;
          anmeldung.IstAktiv = false;
          DbSichern.DsSichern<JgMaschineData.tabAnmeldungMaschine>(Db, anmeldung, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
        }

        Protokoll($"Bediener {Bediener.Name} an Maschine {Maschine.MaschinenName} abmelden.");
      }
    }

    private void ProgrammeEintragen(JgMaschineData.JgModelContainer Db, JgMaschineData.tabMaschine Maschine, DataLogicScannerText e)
    {
      if (_IstStart.Contains(e.VorgangProgramm))  // Wenn ein Start ausgelöst wurde
      {
        var ereignis = JgMaschineData.EnumReperaturEreigniss.Reparatur;

        switch (e.VorgangProgramm)
        {
          case DataLogicScanner.VorgangProgram.REPASTART: ereignis = JgMaschineData.EnumReperaturEreigniss.Reparatur; break;
          case DataLogicScanner.VorgangProgram.WARTSTART: ereignis = JgMaschineData.EnumReperaturEreigniss.Wartung; break;
          case DataLogicScanner.VorgangProgram.COILSTART: ereignis = JgMaschineData.EnumReperaturEreigniss.Coilwechsel; break;
        }

        var reparatur = Db.tabReparaturSet.FirstOrDefault(f => (f.fMaschine == Maschine.Id) && f.IstAktiv && (f.Ereigniss == ereignis));

        if (reparatur != null)
          e.FehlerAusgabe("Vorgang: ", $"- {ereignis} -", "bereits angemeldet !");
        else
        {
          e.SendeText("Beginn Vorgang: ", $"- {ereignis} -", $"MA: {Maschine.MaschinenName}");

          reparatur = new JgMaschineData.tabReparatur()
          {
            Id = Guid.NewGuid(),
            eMaschine = Maschine,
            IstAktiv = true,
            VorgangBeginn = DateTime.Now,
            VorgangEnde = DateTime.Now,
            Ereigniss = ereignis
          };

          if (e.VorgangProgramm == DataLogicScanner.VorgangProgram.COILSTART)
            reparatur.CoilwechselAnzahl = Convert.ToByte(e.ScannerKoerper);

          DbSichern.DsSichern<JgMaschineData.tabReparatur>(Db, reparatur, JgMaschineData.EnumStatusDatenabgleich.Neu);
        }
      }
      else
      {
        var ereignis = JgMaschineData.EnumReperaturEreigniss.Reparatur;

        switch (e.VorgangProgramm)
        {
          case DataLogicScanner.VorgangProgram.REPA_ENDE: ereignis = JgMaschineData.EnumReperaturEreigniss.Reparatur; break;
          case DataLogicScanner.VorgangProgram.WART_ENDE: ereignis = JgMaschineData.EnumReperaturEreigniss.Wartung; break;
          case DataLogicScanner.VorgangProgram.COIL_ENDE: ereignis = JgMaschineData.EnumReperaturEreigniss.Coilwechsel; break;
        }

        var reparatur = Maschine.sReparaturen.FirstOrDefault(f => f.IstAktiv && (f.Ereigniss == ereignis));
        if (reparatur == null)
          e.FehlerAusgabe("Kein Vorgang", $"- {ereignis} -", "angemeldet !");
        else
        {
          e.SendeText("Vorgang beendet: ", $"- {ereignis} -", $"MA: {Maschine.MaschinenName}");

          reparatur.IstAktiv = false;
          reparatur.VorgangEnde = DateTime.Now;

          DbSichern.DsSichern<JgMaschineData.tabReparatur>(Db, reparatur, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
        };

        Protokoll("Vorgang:     {0}", e.VorgangProgramm);
        Protokoll("Maschine:    {0}", Maschine.MaschinenName);
        Protokoll(" ");
      }
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

      using (var db = new JgMaschineData.JgModelContainer())
      {
        var maschine = SucheMaschine(db, e);

        if (maschine == null)
          Protokoll("Maschine nicht gefunden.");
        else
        {
          switch (e.VorgangScan)
          {
            case DataLogicScanner.VorgangScanner.BF2D:

              if (maschine.sAktuelleBediener.FirstOrDefault() == null)
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