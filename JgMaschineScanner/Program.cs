using JgMaschineLib.DataLogicScanner;
using System;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;

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
    public void Start()
    {
      string adresse = "192.168.1.59";
      int port = 51000;
      using (var db = new JgMaschineData.JgModelContainer())
      {
        db.tabStandortSet.FirstOrDefault();
      }
      Scanner sc = new Scanner(adresse, port, ScannerCommunication, true);
    }

    private JgMaschineData.tabMaschine SucheMaschine(JgMaschineData.JgModelContainer Db, ScannerText e)
    {
      var maschine = Db.tabMaschineSet.FirstOrDefault(f => f.ScannerNummer == e.ScannerKennung);
      if (maschine == null)
        e.FehlerAusgabe("Maschine falsch !", "Scanner: " + e.ScannerKennung);
      else
        e.MitDisplay = maschine.ScannerMitDisplay;
      return maschine;
    }

    private async void ScannerCommunication(ScannerText e)
    {
      switch (e.VorgangScan)
      {
        case Scanner.VorgangScanner.BF2D:
          using (var db = new JgMaschineData.JgModelContainer())
          {
            var maschine = SucheMaschine(db, e);
            if (maschine != null)
            {
              var bauteil = new JgMaschineLib.Stahl.BvbsDatenaustausch(e.ScannerVorgangScan + e.ScannerKoerper);
              Console.WriteLine("Maschine: {0}", maschine.MaschinenName);
              Console.WriteLine("Project:  {0}  Anzahl: {1} Gewicht: {2}", bauteil.ProjektNummer, bauteil.Anzahl, bauteil.Gewicht * 1000);
              Console.WriteLine();

              var datensatz = new JgMaschineData.tabDaten()
              {
                Id = Guid.NewGuid(),
                eMaschine = maschine,
                BtAnzahl = Convert.ToInt32(bauteil.Anzahl),
                BtDurchmesser = Convert.ToInt32(bauteil.Durchmesser),
                BtGewicht = Convert.ToInt32(bauteil.Gewicht),
                BtLaenge = Convert.ToInt32(bauteil.Laenge),
                NummerBauteil = bauteil.ProjektNummer,

                IstHandeingabe = false,

                IdStahlPosition = 1,
                IdStahlBauteil = 1,

                DatumEnde = DateTime.Now,
                DatumStart = DateTime.Now
              };
              db.tabDatenSet.Add(datensatz);

              e.SendeText(" ", "  - Bauteil O K -");

              await db.SaveChangesAsync();
            }
          }
          break;

        case Scanner.VorgangScanner.MITA:
          using (var db = new JgMaschineData.JgModelContainer())
          {
            var maschine = SucheMaschine(db, e);
            if (maschine != null)
            {
              var mitarb = db.tabBedienerSet.Find(Guid.Parse(e.ScannerKoerper));
              if (mitarb == null)
                e.FehlerAusgabe("Mitarbeiter falsch ha!", "Scanner: " + e.ScannerKennung, "hhhh asdasdasasd 23967");
              else
              {
                Console.WriteLine("Maschine:    {0}", maschine.MaschinenName);
                Console.WriteLine("Mitarbeiter: {0} {1}", mitarb.VorName, mitarb.NachName);
                Console.WriteLine();

                e.SendeText(" ", "- A N M E L D U N G -", string.Format("{0}", maschine.MaschinenName), string.Format("{0} {1}", mitarb.VorName, mitarb.NachName));
              }
            }
          }
          break;
        case Scanner.VorgangScanner.PROG:

          using (var db = new JgMaschineData.JgModelContainer())
          {
            var maschine = SucheMaschine(db, e);

            if (maschine != null)
            {
              //JgMaschineData.tabReparatur reparatur = null;

              switch (e.VorgangProgramm)
              {
                case Scanner.VorgangProgram.ABMELDUNG:
                  Console.WriteLine("Abmelung von Maschine {0}", maschine.MaschinenName);
                  break;

                case Scanner.VorgangProgram.WARTSTART:
                  Console.WriteLine("Wartung Start");

                  //reparatur = new JgMaschineData.tabReparatur()
                  //{
                  //  Id = Guid.NewGuid(),
                  //  eMaschine = maschine,
                  //  InBearbeitung = true,
                  //  ReparaturVon = DateTime.Now,
                  //  Ereigniss = JgMaschineData.EnumReperaturEreigniss.Coilwechsel
                  //};
                  //db.tabReparaturSet.Add(reparatur);

                  break;
                case Scanner.VorgangProgram.WART_ENDE:
                  Console.WriteLine("Wartung Ende");

                  //reparatur = maschine.sReparaturen.FirstOrDefault(f => f.InBearbeitung && (f.Ereigniss == JgMaschineData.EnumReperaturEreigniss.Coilwechsel));
                  //if (reparatur != null)
                  //{
                  //  reparatur.InBearbeitung = false;
                  //  reparatur.ReparaturBis = DateTime.Now;
                  //};

                  break;


                case Scanner.VorgangProgram.COILSTART:
                  Console.WriteLine("Coilwechsel Start");

                  //reparatur = new JgMaschineData.tabReparatur()
                  //{
                  //  Id = Guid.NewGuid(),
                  //  eMaschine = maschine,
                  //  InBearbeitung = true,
                  //  ReparaturVon = DateTime.Now,
                  //  Ereigniss = JgMaschineData.EnumReperaturEreigniss.Coilwechsel
                  //};
                  //db.tabReparaturSet.Add(reparatur);

                  break;
                case Scanner.VorgangProgram.COIL_ENDE:
                  Console.WriteLine("Coilwechsel Ende");

                  //reparatur = maschine.sReparaturen.FirstOrDefault(f => f.InBearbeitung && (f.Ereigniss == JgMaschineData.EnumReperaturEreigniss.Coilwechsel));
                  //if (reparatur != null)
                  //{
                  //  reparatur.InBearbeitung = false;
                  //  reparatur.ReparaturBis = DateTime.Now;
                  //};

                  break;
                case Scanner.VorgangProgram.REPASTART:
                  Console.WriteLine("Reparatur Start");

                  //reparatur = new JgMaschineData.tabReparatur()
                  //{
                  //  Id = Guid.NewGuid(),
                  //  eMaschine = maschine,
                  //  InBearbeitung = true,
                  //  ReparaturVon = DateTime.Now,
                  //  Ereigniss = JgMaschineData.EnumReperaturEreigniss.Reparatur
                  //};
                  //db.tabReparaturSet.Add(reparatur);

                  break;
                case Scanner.VorgangProgram.REPA_ENDE:
                  Console.WriteLine("Reparatur Ende");

                  //reparatur = maschine.sReparaturen.FirstOrDefault(f => f.InBearbeitung && (f.Ereigniss == JgMaschineData.EnumReperaturEreigniss.Reparatur));
                  //if (reparatur != null)
                  //{
                  //  reparatur.InBearbeitung = false;
                  //  reparatur.ReparaturBis = DateTime.Now;
                  //};

                  break;
                default:
                  break;
              }

              //await db.SaveChangesAsync();

              break;
            }
          }
          break;

        default: // Fehler
          break;
      }

      Console.WriteLine();
    }
  }
}