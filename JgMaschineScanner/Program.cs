using JgMaschineLib.Scanner;
using System;
using System.Linq;
using System.IO;

namespace JgMaschineScanner
{
  class Program
  {
    static void Main(string[] args)
    {
      var st = new ScannerTest();
      st.Start();
      Console.ReadKey();
    }
  }

  public class ScannerTest
  {
    private JgMaschineData.JgModelContainer _Db;
    private DataLogicScanner.VorgangProgram[] _IstStart = { DataLogicScanner.VorgangProgram.WARTSTART, DataLogicScanner.VorgangProgram.REPASTART, DataLogicScanner.VorgangProgram.COILSTART };

    public void Start()
    {
      string adresse = Properties.Settings.Default.Adresse;
      int port = Properties.Settings.Default.Portnummer;

      _Db = new JgMaschineData.JgModelContainer();
      _Db.tabStandortSet.FirstOrDefault();

      var scDataLogic = new DataLogicScanner(adresse, port, ScannerCommunication, true);
      scDataLogic.Start();
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

      var maschine = SucheMaschine(_Db, e);

      if (maschine != null)
      {

        switch (e.VorgangScan)
        {
          case DataLogicScanner.VorgangScanner.BF2D:

            if (maschine.sAktuelleBediener.FirstOrDefault() == null)
              e.FehlerAusgabe(" ", "Es sind keine Bediener", "angemeldet !");
            else
            {
              e.SendeText(" ", "  - Bauteil O K -");

              var bauteil = new JgMaschineLib.Stahl.BvbsDatenaustausch(e.ScannerVorgangScan + e.ScannerKoerper);

              Console.WriteLine("Maschine: {0}", maschine.MaschinenName);
              Console.WriteLine("Project:  {0}  Anzahl: {1} Gewicht: {2}", bauteil.ProjektNummer, bauteil.Anzahl, bauteil.Gewicht * 1000);
              Console.WriteLine();

              var datensatz = new JgMaschineData.tabBauteil()
              {
                Id = Guid.NewGuid(),
                eMaschine = maschine,
                BtAnzahl = Convert.ToInt32(bauteil.Anzahl),
                BtDurchmesser = Convert.ToInt32(bauteil.Durchmesser),
                BtGewicht = Convert.ToInt32(bauteil.Gewicht * 1000),
                BtLaenge = Convert.ToInt32(bauteil.Laenge),
                NummerBauteil = bauteil.ProjektNummer,

                IstHandeingabe = false,

                IdStahlPosition = 1,
                IdStahlBauteil = 1,

                DatumStart = DateTime.Now,
                DatumEnde = DateTime.Now
              };

              JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabBauteil>(_Db, datensatz, JgMaschineData.EnumStatusDatenabgleich.Neu);
            }
            break;
          case DataLogicScanner.VorgangScanner.MITA:

            var mitarb = _Db.tabBedienerSet.Find(Guid.Parse(e.ScannerKoerper));
            if (mitarb == null)
              e.FehlerAusgabe("Mitarbeiter falsch!", " ", "MA: " + maschine.MaschinenName);
            else
            {
              if (e.VorgangProgramm == DataLogicScanner.VorgangProgram.ANMELDUNG)
              {
                bool anmeldungErstellen = true;
                if (mitarb.fAktuellAngemeldet != null)
                {
                  if (mitarb.fAktuellAngemeldet == maschine.Id)
                  {
                    e.FehlerAusgabe("Sie sind bereits an", "MA: " + maschine.MaschinenName, "angemeldet !");
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

                      JgMaschineLib.DbSichern.AbgleichEintragen(anmeldung.DatenAbgleich, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
                      _Db.SaveChanges();
                    }
                  }
                }

                if (anmeldungErstellen)
                {
                  e.SendeText(" ", "- A N M E L D U N G -", string.Format("{0}", maschine.MaschinenName), mitarb.Name);

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

                  JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabAnmeldungMaschine>(_Db, anmeldung, JgMaschineData.EnumStatusDatenabgleich.Neu);
                }
              }
              else // Abmeldung
              {
                if (mitarb.eAktuelleAnmeldungMaschine == null)
                  e.FehlerAusgabe(" ", "Sie sind an keiner", "Maschine angemeldet !");
                else
                {
                  e.SendeText(" ", "- A B M E L D U N G -", " ", mitarb.Name,  "MA: " + maschine.MaschinenName);

                  mitarb.eAktuelleAnmeldungMaschine = null;

                  var anmeldung = _Db.tabAnmeldungMaschineSet.FirstOrDefault(f => (f.fMaschine == maschine.Id) && (f.fBediener == mitarb.Id) && f.IstAktiv);
                  if (anmeldung != null)
                  {
                    anmeldung.Abmeldung = DateTime.Now;
                    anmeldung.ManuelleAbmeldung = false;
                    anmeldung.IstAktiv = false;

                    JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabAnmeldungMaschine>(_Db, anmeldung, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
                  }
                }
              }
              Console.WriteLine("Vorgang:     {0}", e.VorgangProgramm);
              Console.WriteLine("Maschine:    {0}", maschine.MaschinenName);
              Console.WriteLine("Mitarbeiter: {0} {1}", mitarb.VorName, mitarb.NachName);
              Console.WriteLine();
            }

            break;
          case DataLogicScanner.VorgangScanner.PROG:
            if (_IstStart.Contains(e.VorgangProgramm))
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
                e.FehlerAusgabe("Vorgang: ", string.Format("- {0} -", ereignis), "bereits angemeldet !");
              else
              {
                e.SendeText("Beginn Vorgang: ", string.Format("- {0} -", ereignis), "MA: " + maschine.MaschinenName);

                reparatur = new JgMaschineData.tabReparatur()
                {
                  Id = Guid.NewGuid(),
                  eMaschine = maschine,
                  IstAktiv = true,
                  VorgangBeginn = DateTime.Now,
                  VorgangEnde = DateTime.Now,
                  Ereigniss = ereignis
                };

                JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabReparatur>(_Db, reparatur, JgMaschineData.EnumStatusDatenabgleich.Neu);
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
              {
                e.FehlerAusgabe("Kein Vorgang", string.Format("- {0} -", ereignis), "angemeldet !");
              }
              else
              {
                e.SendeText("Vorgang beendet: ", string.Format("- {0} -", ereignis), "MA: " + maschine.MaschinenName);

                reparatur.IstAktiv = false;
                reparatur.VorgangEnde = DateTime.Now;

                JgMaschineLib.DbSichern.DsSichern<JgMaschineData.tabReparatur>(_Db, reparatur, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
              };
            }
            break;

          default: // Fehler
            break;
        }
      }

      Console.WriteLine();
    }
  }
}