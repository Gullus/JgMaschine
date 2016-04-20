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

    private async void ScannerCommunication(ScannerText e)
    {
      switch (e.VorgangScan)
      {
        case Scanner.VorgangScanner.BF2D:
          using (var db = new JgMaschineData.JgModelContainer())
          {
            var maschine = await db.tabMaschineSet.FirstOrDefaultAsync(f => f.ScannerNummer == e.ScannerKennung);
            if (maschine == null)
              e.FehlerAusgabe("Maschine nicht gefunden !", "Scanner: " + e.ScannerKennung);
            else
            {
              var bauteil = new JgMaschineLib.Stahl.BvbsDatenaustausch(e.ScannerVorgangScan + e.ScannerKoerper);
              Console.WriteLine("Maschine: {0}", maschine.MaschinenName);
              Console.WriteLine("Bauteil:  {0}  Anzahl: {1} Gewicht: {2}", bauteil.Position, bauteil.Anzahl, bauteil.Gewicht);
              Console.WriteLine();
            }
          }
          break;

        case Scanner.VorgangScanner.MITA:
          using (var db = new JgMaschineData.JgModelContainer())
          {
            var maschine = await db.tabMaschineSet.FirstOrDefaultAsync(f => f.ScannerNummer == e.ScannerKennung);
            if (maschine == null)
              e.FehlerAusgabe("Maschine nicht gefunden !", "Scanner: " + e.ScannerKennung);
            else
            {
              var mitarb = await db.tabBedienerSet.FindAsync(Guid.Parse(e.ScannerKoerper));
              if (mitarb == null)
                e.FehlerAusgabe("Mtarbeiter nicht gefunden !", "Scanner: " + e.ScannerKennung);
              else
              {
                Console.WriteLine("Maschine:    {0}", maschine.MaschinenName);
                Console.WriteLine("Mitarbeiter: {0} {1}", mitarb.VorName, mitarb.NachName);
                Console.WriteLine();
              
                e.SendeText(" ", " - A N M E L D U N G -", string.Format("{0}", maschine.MaschinenName), string.Format("{0} {1}", mitarb.VorName, mitarb.NachName));
              }
            }
          }
          break;
        case Scanner.VorgangScanner.PROG:
          switch (e.VorgangProgramm)
          {
            case Scanner.VorgangProgram.COILSTART:
              break;
            case Scanner.VorgangProgram.COIL_ENDE:
              break;
            case Scanner.VorgangProgram.REPASTART:
              break;
            case Scanner.VorgangProgram.REPA_ENDE:
              break;
            default:
              break;
          }
          break;
        default: // Fehler
          break;
      }
    }
  }
}