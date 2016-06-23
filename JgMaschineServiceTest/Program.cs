using JgMaschineLib.Scanner;
using System;

namespace JgMaschineServiceTest
{
  class Program
  {
    static void Main(string[] args)
    {
      var pr = Properties.Settings.Default;
      var st = new ScannerProgramm()
      {
        ScannnerAdresse = pr.ScannerAdresse,
        ScannerPortNummer = pr.ScannerPort,
        DbVerbindungsString = pr.DbVerbindung,
        ProtokollAnzeigen = true,
        EvgPfadProduktionsListe = pr.EvgPfadProduktionsListe,
        EvgDateiProduktionsAuftrag = pr.EvgFileProduktionsAuftrag
      };

      Console.ReadKey();
      st.Close();
    }
  }
}
