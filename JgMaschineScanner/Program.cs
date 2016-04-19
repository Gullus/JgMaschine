using JgMaschineLib.DataLogicScanner;
using System;

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
      Scanner sc = new Scanner(adresse, port, ScannerCommunication, true);
    }

    private static void ScannerCommunication(ScannerText e)
    {
      Console.WriteLine("Rückmeldung Sanner -> Vorgang: {0}", e.VorgangScan);

      switch (e.VorgangScan)
      {
        case Scanner.VorgangScanner.BF2D:
          Console.WriteLine("{0}{1}", e.ScannerVorgangScan, e.ScannerKoerper);
          e.SendeText("Angemeldet:", "Alex Groß", " ", "        - O K -");
          break;
        case Scanner.VorgangScanner.PROG:
          switch (e.VorgangProgramm)
          {
            case Scanner.VorgangProgram.FEHLER:
              break;
            case Scanner.VorgangProgram.ISTBAUTEIL:
              break;
            case Scanner.VorgangProgram.MITARBEIT:
              using (var db = new JgMaschineData.JgModelContainer())
              {
                var mitarb = db.tabBedienerSet.Find(Guid.Parse(e.ScannerKoerper));
                Console.WriteLine("{0} {1}", mitarb.VorName, mitarb.NachName);

                e.SendeText(" ", " - A N M E L D U N G -", " ", string.Format("{0} {1}", mitarb.VorName, mitarb.NachName));
              }
                break;
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