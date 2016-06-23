using System;
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

    public bool ProtokollAnzeigen { get; set; } = false;

    private class MaschinenDaten
    {
      public string BvbsString { get; set; }
      public JgMaschineData.tabMaschine Maschine { get; set; }
    }

    private DataLogicScanner _DlScanner;
    private JgMaschineData.JgModelContainer _Db;
    private DataLogicScanner.VorgangProgram[] _IstStart = { DataLogicScanner.VorgangProgram.WARTSTART, DataLogicScanner.VorgangProgram.REPASTART, DataLogicScanner.VorgangProgram.COILSTART };

    public ScannerProgramm()
    {
      _Db = new JgMaschineData.JgModelContainer();
      _Db.Database.Connection.ConnectionString = DbVerbindungsString;
      _Db.tabStandortSet.FirstOrDefault();

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

    private void ScannerCommunication(DataLogicScannerText e)
    {
      #region Sannertext in Datei eintragen

      //var dat = (@"C:\Users\jg\Desktop\ScannerBeispiele.txt");
      //using (StreamWriter sw = File.AppendText(dat))
      //{
      //  sw.WriteLine(e.TextEmpfangen);
      //}

      #endregion

      _Db = new JgMaschineData.JgModelContainer();
      var maschine = SucheMaschine(_Db, e);

      if (maschine != null)
      {
        switch (e.VorgangScan)
        {
          case DataLogicScanner.VorgangScanner.BF2D:

            if (maschine.sAktuelleBediener.FirstOrDefault() == null)
              e.FehlerAusgabe(" ", "Es ist keine Bediener", "angemeldet !");
            else
            {
              var btNeu = new JgMaschineLib.Stahl.BvbsDatenaustausch(e.ScannerVorgangScan + e.ScannerKoerper);

              #region Scannerdaten an Maschine senden

              if (string.IsNullOrWhiteSpace(maschine.MaschineAdresse) && (maschine.MaschinePortnummer != null))
              {
                var datenAnMaschine = Task.Factory.StartNew((mDaten) =>
                {
                  var md = (MaschinenDaten)mDaten;

                  try
                  {
                    using (var client = new TcpClient(md.Maschine.MaschineAdresse, (int)md.Maschine.MaschinePortnummer))
                    {
                      var nwStr = client.GetStream();
                      switch (md.Maschine.ProtokollName)
                      {
                        case JgMaschineData.EnumProtokollName.Progress:
                          var buffer = Encoding.ASCII.GetBytes(md.BvbsString);
                          nwStr.Write(buffer, 0, buffer.Length);
                          break;
                        default:
                          return;
                      }
                      nwStr.Close();
                    }
                  }
                  catch (Exception f)
                  {
                    JgMaschineLib.Helper.InWinProtokoll($"Fehler beim senden der Bvbs Daten an die Maschine: {maschine.MaschinenName}\n\rDaten: {md.BvbsString}\n\rFehler: {f.Message}", System.Diagnostics.EventLogEntryType.Error);
                  }
                }, new MaschinenDaten() { BvbsString = btNeu.BvbsString, Maschine = maschine });
              }

              #endregion

              Protokoll("Maschine: {0}", maschine.MaschinenName);
              Protokoll("Project:  {0}  Anzahl: {1} Gewicht: {2}", btNeu.ProjektNummer, btNeu.Anzahl, btNeu.Gewicht * 1000);
              Protokoll(" ");

              var neuesBauteilErstellen = true;

              if (maschine.eLetztesBauteil != null)
              {
                var letztesBt = maschine.eLetztesBauteil;
                letztesBt.DatumEnde = DateTime.Now;
                DbSichern.AbgleichEintragen(letztesBt.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);

                neuesBauteilErstellen = (letztesBt.NummerBauteil != btNeu.ProjektNummer)
                  || (letztesBt.BtDurchmesser != btNeu.Durchmesser)
                  || (letztesBt.BtGewicht != (btNeu.Gewicht * 1000))
                  || (letztesBt.BtLaenge != btNeu.Laenge);

                if (!neuesBauteilErstellen)
                  e.SendeText(" ", " - Bauteil erledigt -");
              }

              if (neuesBauteilErstellen)
              {
                var btInDatenBank = _Db.tabBauteilSet.FirstOrDefault(f => ((f.fMaschine == maschine.Id)
                  && (f.NummerBauteil == btNeu.ProjektNummer)
                  && (f.BtDurchmesser == btNeu.Durchmesser)
                  && (f.BtGewicht == btNeu.Gewicht)
                  && (f.BtLaenge == btNeu.Laenge)));

                if (btInDatenBank != null)
                  e.FehlerAusgabe("Bauteil bereits am", btInDatenBank.DatumStart.ToString("dd.MM.yy HH:mm"), "gefertigt.");
                else
                {
                  e.SendeText(" ", "  - Bauteil O K -");

                  var btNeuErstellt = new JgMaschineData.tabBauteil()
                  {
                    Id = Guid.NewGuid(),
                    eMaschine = maschine,
                    BtAnzahl = Convert.ToInt32(btNeu.Anzahl),
                    BtDurchmesser = Convert.ToInt32(btNeu.Durchmesser),
                    BtGewicht = Convert.ToInt32(btNeu.Gewicht * 1000),
                    BtLaenge = Convert.ToInt32(btNeu.Laenge),
                    NummerBauteil = btNeu.ProjektNummer,

                    IstHandeingabe = false,

                    IdStahlPosition = 1,
                    IdStahlBauteil = 1,

                    AnzahlBediener = (byte)maschine.sAktuelleBediener.Count(),
                    AnzahlBiegungen = (byte)btNeu.ListeGeometrie.Count,

                    DatumStart = DateTime.Now,
                    DatumEnde = DateTime.Now
                  };

                  foreach (var bed in maschine.sAktuelleBediener)
                    btNeuErstellt.sBediener.Add(bed);

                  maschine.eLetztesBauteil = btNeuErstellt;
                  DbSichern.DsSichern<JgMaschineData.tabBauteil>(_Db, btNeuErstellt, JgMaschineData.EnumStatusDatenabgleich.Neu);
                }
              }
            }
            break;

          case DataLogicScanner.VorgangScanner.MITA:

            var mitarb = _Db.tabBedienerSet.Find(Guid.Parse(e.ScannerKoerper));
            if (mitarb == null)
              e.FehlerAusgabe("Mitarbeiter unbekannt!", " ", $"MA: {maschine.MaschinenName}", e.ScannerKoerper);
            else
            {
              if (e.VorgangProgramm == DataLogicScanner.VorgangProgram.ANMELDUNG)
              {
                bool anmeldungErstellen = true;
                if (mitarb.fAktuellAngemeldet != null)
                {
                  if (mitarb.fAktuellAngemeldet == maschine.Id)
                  {
                    e.FehlerAusgabe("Sie sind bereits an", $"MA: {maschine.MaschinenName}", "angemeldet !");
                    anmeldungErstellen = false;
                  }
                  else
                  {
                    var anmeldung = _Db.tabAnmeldungMaschineSet.FirstOrDefault(f => (f.fMaschine == mitarb.fAktuellAngemeldet) && (f.fBediener == mitarb.Id) && f.IstAktiv);
                    if (anmeldung != null)
                    {
                      anmeldung.Abmeldung = DateTime.Now;
                      anmeldung.ManuelleAbmeldung = false;
                      anmeldung.IstAktiv = false;

                      // speichervorgang muss vor nächster Speicherung abgeschlossen sein.

                      DbSichern.AbgleichEintragen(anmeldung.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
                      _Db.SaveChanges();
                    }
                  }
                }

                if (anmeldungErstellen)
                {
                  e.SendeText(" ", "- A N M E L D U N G -", maschine.MaschinenName, mitarb.Name);

                  mitarb.eAktuelleAnmeldungMaschine = maschine;

                  var anmeldung = new JgMaschineData.tabAnmeldungMaschine()
                  {
                    Id = Guid.NewGuid(),
                    Anmeldung = DateTime.Now,
                    eBediener = mitarb,
                    eMaschine = maschine,
                    IstAktiv = true,
                    ManuelleAnmeldung = false,
                    Abmeldung = DateTime.Now,
                    ManuelleAbmeldung = false
                  };

                  DbSichern.DsSichern<JgMaschineData.tabAnmeldungMaschine>(_Db, anmeldung, JgMaschineData.EnumStatusDatenabgleich.Neu);
                }
              }
              else // Abmeldung
              {
                if (mitarb.eAktuelleAnmeldungMaschine == null)
                  e.FehlerAusgabe(" ", "Sie sind an keiner", "Maschine angemeldet !");
                else
                {
                  e.SendeText(" ", "- A B M E L D U N G -", " ", mitarb.Name, $"MA: {maschine.MaschinenName}");

                  mitarb.eAktuelleAnmeldungMaschine = null;

                  var anmeldung = _Db.tabAnmeldungMaschineSet.FirstOrDefault(f => (f.fMaschine == maschine.Id) && (f.fBediener == mitarb.Id) && f.IstAktiv);
                  if (anmeldung != null)
                  {
                    anmeldung.Abmeldung = DateTime.Now;
                    anmeldung.ManuelleAbmeldung = false;
                    anmeldung.IstAktiv = false;

                    DbSichern.DsSichern<JgMaschineData.tabAnmeldungMaschine>(_Db, anmeldung, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
                  }
                }
              }
              Protokoll("Vorgang:     {0}", e.VorgangProgramm);
              Protokoll("Maschine:    {0}", maschine.MaschinenName);
              Protokoll("Mitarbeiter: {0} {1}", mitarb.VorName, mitarb.NachName);
              Protokoll(" ");
            }

            break;
          case DataLogicScanner.VorgangScanner.PROG:
            if (_IstStart.Contains(e.VorgangProgramm))  // Wenn ein Start ausgelöst wurde
            {
              var ereignis = JgMaschineData.EnumReperaturEreigniss.Reparatur;

              switch (e.VorgangProgramm)
              {
                case DataLogicScanner.VorgangProgram.REPASTART: ereignis = JgMaschineData.EnumReperaturEreigniss.Reparatur; break;
                case DataLogicScanner.VorgangProgram.WARTSTART: ereignis = JgMaschineData.EnumReperaturEreigniss.Wartung; break;
                case DataLogicScanner.VorgangProgram.COILSTART: ereignis = JgMaschineData.EnumReperaturEreigniss.Coilwechsel; break;
              }

              var reparatur = _Db.tabReparaturSet.FirstOrDefault(f => (f.fMaschine == maschine.Id) && f.IstAktiv && (f.Ereigniss == ereignis));

              if (reparatur != null)
                e.FehlerAusgabe("Vorgang: ", $"- {ereignis} -", "bereits angemeldet !");
              else
              {
                e.SendeText("Beginn Vorgang: ", $"- {ereignis} -", $"MA: {maschine.MaschinenName}");

                reparatur = new JgMaschineData.tabReparatur()
                {
                  Id = Guid.NewGuid(),
                  eMaschine = maschine,
                  IstAktiv = true,
                  VorgangBeginn = DateTime.Now,
                  VorgangEnde = DateTime.Now,
                  Ereigniss = ereignis
                };

                if (e.VorgangProgramm == DataLogicScanner.VorgangProgram.COILSTART)
                  reparatur.CoilwechselAnzahl = Convert.ToByte(e.ScannerKoerper);

                DbSichern.DsSichern<JgMaschineData.tabReparatur>(_Db, reparatur, JgMaschineData.EnumStatusDatenabgleich.Neu);
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

              var reparatur = maschine.sReparaturen.FirstOrDefault(f => f.IstAktiv && (f.Ereigniss == ereignis));
              if (reparatur == null)
                e.FehlerAusgabe("Kein Vorgang", $"- {ereignis} -", "angemeldet !");
              else
              {
                e.SendeText("Vorgang beendet: ", $"- {ereignis} -", $"MA: {maschine.MaschinenName}");

                reparatur.IstAktiv = false;
                reparatur.VorgangEnde = DateTime.Now;

                DbSichern.DsSichern<JgMaschineData.tabReparatur>(_Db, reparatur, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
              };

              Protokoll("Vorgang:     {0}", e.VorgangProgramm);
              Protokoll("Maschine:    {0}", maschine.MaschinenName);
              Protokoll(" ");
            }
            break;

          default: // Fehler
            break;
        }
      }
    }
  }
}