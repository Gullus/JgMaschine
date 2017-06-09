using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using JgMaschineData;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceScanner
{
  public class ScannerProgramm
  {
    private ScannerOptionen _Opt = null;
    private DataLogicScanner _DlScanner;

    internal struct DatenAnMaschine
    {
      public ScannerOptionen Optionen { get; set; }
      public tabMaschine Maschine { get; set; }
      public string BvbsString { get; set; }
    }

    public ScannerProgramm(ScannerOptionen Optionen)
    {
      _Opt = Optionen;
    }

    public void Start()
    {
      // Craddle initialisieren

      _DlScanner = new DataLogicScanner(_Opt, async (ScText) =>
      {
        // Dieses Programm wird nach Empfang von Text von einem Scanner aufgerufen

        if (ScText.VorgangScan == VorgangScanner.CRADDLEANMELDUNG)
          return;

        try
        {
          using (_Opt.DbScann = new JgModelContainer())
          {
            var maschine = _Opt.DbScann.tabMaschineSet.FirstOrDefault(f => f.ScannerNummer == ScText.ScannerKennung);

            if (maschine == null)
              ScText.FehlerAusgabe("Maschine nicht in", "Datenbank", "vorhanden !!!");
            else
            {
              ScText.ScannerIstMitDisplay = maschine.ScannerMitDisplay;

              maschine.AktiveAnmeldungen = (from bediener in _Opt.DbScann.tabBedienerSet
                                            join anmeldung in _Opt.DbScann.tabAnmeldungMaschineSet on bediener.fAktivAnmeldung equals anmeldung.Id
                                            where anmeldung.fMaschine == maschine.Id
                                            select anmeldung).Include(i => i.eBediener).ToList();

              switch (ScText.VorgangScan)
              {
                case VorgangScanner.BF3D:
                case VorgangScanner.BF2D:
                  if (maschine.AktiveAnmeldungen.Count == 0)
                    ScText.FehlerAusgabe(" ", "Es ist keine Bediener", "angemeldet !");
                  else
                    Bf2dEintragen(_Opt, maschine, ScText);
                  break;
                case VorgangScanner.MITA:
                  BedienerAnmelden(_Opt, maschine, ScText);
                  break;
                case VorgangScanner.PROG:
                  ProgrammeEintragen(_Opt, maschine, ScText);
                  break;
              }

              await _Opt.DbScann.SaveChangesAsync();
            }
          }
        }
        catch (Exception ex)
        {
          ExceptionPolicy.HandleException(ex, "Policy");
        }
      });

      _DlScanner.Start();
    }

    public void Close()
    {
      _DlScanner.Close();
    }

    // Programme zum Datenverarbeitung der Sacannerdaten **************************************************************

    private static void DatenAnMaschineSenden(ScannerOptionen Optionen, tabMaschine Maschine, string BfbsString)
    {
      if ((Maschine.MaschinenArt == EnumMaschinenArt.Handbiegung) || (string.IsNullOrWhiteSpace(Maschine.MaschineAdresse)))
        return;

      string msg = "";

      switch (Maschine.MaschinenArt)
      {
        case EnumMaschinenArt.Schnell:

          if (Maschine.MaschinePortnummer == null)
          {
            msg = $"Bei Maschine: {Maschine.MaschinenName} wurde keine Portnummer eingetragen!";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
            return;
          }

          Task.Factory.StartNew((SendDaten) =>
          {
            var md = (DatenAnMaschine)SendDaten;

            try
            {
              msg = $"Verbindung mit: {md.Maschine.MaschinenName} Port: {md.Maschine.MaschinePortnummer}.";
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

                try
                {
                  buffer = new byte[client.ReceiveBufferSize];

                  int anzEmpfang = nwStr.Read(buffer, 0, (int)client.ReceiveBufferSize);

                  var empfangen = Encoding.ASCII.GetString(buffer, 0, anzEmpfang);
                  msg = $"Rückantwort von Maschine {md.Maschine.MaschinenName}: {empfangen}";
                  Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);

                  #region Antwort der Maschine auf Fehler prüfen

                  if ((empfangen.Length >= 3) && (empfangen[0] == Convert.ToChar(15)))
                  {
                    var dat = JgMaschineLib.Helper.StartVerzeichnis() + @"FehlerCode\JgMaschineFehlerSchnell.txt";
                    if (!File.Exists(dat))
                    {
                      msg = $"Fehlerdatei: {dat} für Maschine 'Schnell' existiert nicht.";
                      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
                    }
                    else
                    {
                      try
                      {
                        string nummer = empfangen.Substring(1, 2);

                        var zeilen = File.ReadAllLines(dat);
                        foreach (var zeile in zeilen)
                        {
                          if (zeile.Substring(0, 2) == nummer)
                          {
                            msg = $"Fehlermeldung von Maschine {md.Maschine.MaschinenName} ist {zeile}!";
                            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
                            break;
                          }
                        }
                      }
                      catch (Exception f)
                      {
                        msg = $"Fehler beim auslesen der Fehlerdatei Firma Schnell.\nGrund: {f.Message}";
                        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
                      }
                    }
                  }

                  #endregion

                }
                catch (Exception f)
                {
                  throw new Exception($"Fehler bei warten auf Antwort von Maschine ! {f.Message}", f);
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
          }, new DatenAnMaschine() { Optionen = Optionen, Maschine = Maschine, BvbsString = BfbsString });

          break;
        case EnumMaschinenArt.Evg:

          Task.Factory.StartNew((SendDaten) =>
          {
            var md = (DatenAnMaschine)SendDaten;

            var datAuftrag = "Auftrag1.txt";
            var datProdListe = string.Format(@"\\{0}\{1}\{2}", md.Maschine.MaschineAdresse, md.Optionen.EvgPfadProduktionsListe, datAuftrag);

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

            // Produktionsauftrag schreiben

            var datProtAuftrag = string.Format(@"\\{0}\{1}", md.Maschine.MaschineAdresse, md.Optionen.EvgDateiProduktionsAuftrag);
            try
            {
              File.WriteAllText(datProtAuftrag, datAuftrag);
            }
            catch (Exception f)
            {
              msg = $"Fehler beim schreiben des EVG Produktionsauftrages in die Maschine {md.Maschine.MaschinenName}!\nDatei: {datProtAuftrag}.\nGrund: {f.Message}";
              Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
            }

          }, new { Optionen, Maschine, BfbsString });

          break;
        case EnumMaschinenArt.Progress:

          Task.Factory.StartNew((SendDaten) =>
          {
            var md = (DatenAnMaschine)SendDaten;
            var datei = string.Format(@"\\{0}\{1}\{2}", md.Maschine.MaschineAdresse, md.Optionen.ProgressPfadProduktionsListe, "Auftrag.txt");

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
          }, new { Optionen, Maschine, BfbsString });

          break;
      }
    }

    private static void Bf2dEintragen(ScannerOptionen Optionen, tabMaschine Maschine, ScannerText ScText)
    {
      string msg = "";
      BvbsDatenaustausch btNeu = null;

      try
      {
        btNeu = new BvbsDatenaustausch(ScText.ScannerVorgangScan + ScText.ScannerKoerper, false);
      }
      catch (Exception f)
      {
        ScText.FehlerAusgabe("BVBS Code konnte nicht", "gelesen werden !");
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

          ScText.SendeText(" ", " - Bauteil erledigt -");
          Maschine.eAktivBauteil = null;

          return;
        }
      }

      if (ScText.VorgangScan == VorgangScanner.BF2D)
        DatenAnMaschineSenden(Optionen, Maschine, btNeu.BvbsString);

      var btInDatenBank = Optionen.DbScann.tabBauteilSet.FirstOrDefault(f => ((f.fMaschine == Maschine.Id) && (f.BvbsCode == btNeu.BvbsString)));

      if (btInDatenBank != null)
      {
        msg = $"Bauteil an Maschine {Maschine.MaschinenName} bereits am {btInDatenBank.DatumStart.ToString("dd.MM.yy HH:mm")} gefertigt.";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);

        ScText.FehlerAusgabe("Bauteil bereits am", btInDatenBank.DatumStart.ToString("dd.MM.yy HH:mm"), "gefertigt.");
      }
      else
      {
        ScText.SendeText(" ", "- Bauteil OK -");

        var bvbs = new BvbsDatenaustausch(btNeu.BvbsString, false);

        var btNeuErstellt = new tabBauteil()
        {
          Id = Guid.NewGuid(),
          DatumStart = DateTime.Now,

          eMaschine = Maschine,

          BvbsCode = btNeu.BvbsString,
          BvbsDaten = bvbs,

          BtGewicht = StahlGewichte.GetGewichtKg((int)btNeu.Durchmesser, (int)btNeu.Laenge),

          IstHandeingabe = false,
          IstVorfertigung = Maschine.IstStangenschneider && ((byte)btNeu.ListeGeometrie.Count == 0),

          AnzahlBediener = (byte)Maschine.AktiveAnmeldungen.Count(),
        };

        foreach (var bed in Maschine.AktiveAnmeldungen)
          btNeuErstellt.sBediener.Add(bed.eBediener);

        Maschine.eAktivBauteil = btNeuErstellt;

        msg = $"BT erstellt. Maschine: {Maschine.MaschinenName}\n  Project: {btNeu.ProjektNummer} Anzahl: {btNeu.Anzahl} Gewicht: {btNeuErstellt.BtGewicht}";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
      }
    }

    private static tabBediener SuchBediener(ScannerOptionen Optionen, ScannerText ScText)
    {
      var bed = Optionen.DbScann.tabBedienerSet.FirstOrDefault(f => f.MatchCode == ScText.ScannerKoerper);
      if (bed != null)
        return bed;

      ScText.FehlerAusgabe("Bediener mit Matchcode", ScText.ScannerKoerper, "nicht vorhanden!");
      var msg = $"Bediener mit Matchcode {ScText.ScannerKoerper} nicht vorhanden.";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
      return null;
    }

    private static void BedienerAnmelden(ScannerOptionen Optionen, tabMaschine Maschine, ScannerText ScText)
    {
      string msg = "";

      var matchCode = ScText.ScannerKoerper;
      var anmeldungVorhanden = Maschine.AktiveAnmeldungen.FirstOrDefault(f => f.eBediener.MatchCode == matchCode);

      if ((ScText.VorgangProgramm == VorgangProgram.ANMELDUNG) && (anmeldungVorhanden != null))
      {
        ScText.FehlerAusgabe("Sie sind bereits an", Maschine.MaschinenName, "angemeldet !");
        return;
      }

      var bediener = anmeldungVorhanden?.eBediener;

      if (anmeldungVorhanden == null)
      {
        bediener = SuchBediener(Optionen, ScText);
        if (bediener == null)
          return;
        anmeldungVorhanden = bediener.eAktivAnmeldung;
      }

      // Wenn Bediener an anderer Maschine angemeldet -> abmelden
      if (anmeldungVorhanden != null)
      {
        // Kontrolle, ob an anderer Maschine eine Reparatur läuft
        if (anmeldungVorhanden.eMaschine.eAktivReparatur != null)
        {
          var anmeldungReparatur = anmeldungVorhanden.eMaschine.eAktivReparatur.sAnmeldungen.FirstOrDefault(f => f.IstAktiv && (f.fBediener == bediener.Id));
          if (anmeldungReparatur != null)
            anmeldungReparatur.Abmeldung = DateTime.Now;
        }

        anmeldungVorhanden.Abmeldung = DateTime.Now;
        anmeldungVorhanden.ManuelleAbmeldung = false;
        anmeldungVorhanden.eBediener.eAktivAnmeldung = null;
      }

      if (ScText.VorgangProgramm == VorgangProgram.ABMELDUNG)
      {
        if (anmeldungVorhanden == null)
        {
          ScText.FehlerAusgabe("Sie sind an keiner", "Maschine", "angemeldet !");
          return;
        }

        ScText.SendeText(" - Abmeldung -", anmeldungVorhanden.eBediener.Name, anmeldungVorhanden.eMaschine.MaschinenName);
        msg = $"Bediener {anmeldungVorhanden.eBediener.Name} an Maschine {anmeldungVorhanden.eMaschine.MaschinenName} abgemeldet.";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
      }
      else // Anmeldung an Maschine
      {
        ScText.SendeText(" ", "- A N M E L D U N G -", " ", bediener.Name, Maschine.MaschinenName);

        var anmeldungNeu = new tabAnmeldungMaschine()
        {
          Id = Guid.NewGuid(),
          Anmeldung = DateTime.Now,
          fBediener = bediener.Id,
          fMaschine = Maschine.Id,
          ManuelleAnmeldung = false,
          ManuelleAbmeldung = false,
        };
        Optionen.DbScann.tabAnmeldungMaschineSet.Add(anmeldungNeu);
        bediener.fAktivAnmeldung = anmeldungNeu.Id;

        // Wenn eine Reparatur läuft, an dieser anmelden

        if (Maschine.eAktivReparatur != null)
        {
          var anmeldungReparatur = new tabAnmeldungReparatur()
          {
            Id = Guid.NewGuid(),
            Anmeldung = DateTime.Now,
            fBediener = bediener.Id,
            fReparatur = (Guid)Maschine.fAktivReparatur,
          };
          Optionen.DbScann.tabAnmeldungReparaturSet.Add(anmeldungReparatur);
        }

        msg = $"Bediener {bediener.Name} an Maschine {Maschine.MaschinenName} angemeldet.";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
      }
    }

    private static void ProgrammeEintragen(ScannerOptionen Optionen, tabMaschine Maschine, ScannerText e)
    {
      string msg = "";
      var reparatur = Maschine.eAktivReparatur;

      if (e.VorgangProgramm == VorgangProgram.REPA_ENDE)
      {
        if (reparatur == null)
          e.FehlerAusgabe(" ", "Kein Vorgang", "angemeldet !");
        else
        {
          e.SendeText("Vorgang beendet", " ", $"> {reparatur.Vorgang.ToString().ToUpper()} <", " ", Maschine.MaschinenName);

          reparatur.VorgangEnde = DateTime.Now;
          Maschine.eAktivReparatur = null;

          foreach (var anmledungReparatur in reparatur.sAnmeldungen.Where(w => w.IstAktiv).ToList())
            anmledungReparatur.Abmeldung = reparatur.VorgangEnde;

          msg = $"Reparatur {reparatur.Vorgang} an Maschine {Maschine.MaschinenName} beendet.";
          Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
        };
      }
      else // Anmeldung einer Reparatur
      {
        if (reparatur != null)
          e.FehlerAusgabe(" ", $"> {reparatur.Vorgang.ToString().ToUpper()} <", "bereits angemeldet !");
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
            case VorgangProgram.REPASTART: reparatur.Vorgang = EnumReperaturVorgang.Reparatur; break;
            case VorgangProgram.WARTSTART: reparatur.Vorgang = EnumReperaturVorgang.Wartung; break;
            case VorgangProgram.COILSTART: reparatur.Vorgang = EnumReperaturVorgang.Coilwechsel; break;
          }

          var anzCoils = " ";
          if (e.VorgangProgramm == VorgangProgram.COILSTART)
          {
            reparatur.CoilwechselAnzahl = Convert.ToByte(e.ScannerKoerper);
            anzCoils = $"Anzahl {reparatur.CoilwechselAnzahl}";
          }
          e.SendeText("Beginn Vorgang", " ", $"> {reparatur.Vorgang.ToString().ToUpper()} <", anzCoils, Maschine.MaschinenName);

          Maschine.eAktivReparatur = reparatur;

          foreach (var anmeldungen in Maschine.AktiveAnmeldungen)
          {
            var anmeldungReparatur = new tabAnmeldungReparatur()
            {
              Id = Guid.NewGuid(),
              Anmeldung = reparatur.VorgangBeginn,
              fBediener = anmeldungen.fBediener,
              fReparatur = reparatur.Id,
            };
            Optionen.DbScann.tabAnmeldungReparaturSet.Add(anmeldungReparatur);
          }

          msg = $"Reparatur {reparatur.Vorgang} an Maschine {Maschine.MaschinenName} gestartet.";
          Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
        }
      }
    }
  }
}