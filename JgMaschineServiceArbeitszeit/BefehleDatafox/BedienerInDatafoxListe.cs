using System;
using System.IO;
using System.Text;
using JgMaschineLib;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceArbeitszeit
{
  public static partial class ProgDatafox
  {
    public static void BedienerInDatafoxDatei(OptionenArbeitszeit Optionen)
    {
      string msg;
      var sb = new StringBuilder();
      foreach (var bediener in Optionen.ListeBediener)
      {
        var bed = (bediener.Name.Length > 25) ? bediener.Name.Substring(0, 25) : bediener.Name;
        sb.AppendLine($"MITA0{bediener.MatchCode}\tMITA1{bediener.MatchCode}\t{bed}");
      }

      if (!Directory.Exists(Optionen.PfadUpdateBediener))
      {
        var tempPfad = Path.GetTempPath();
        msg = $"Pfad '{Optionen.PfadUpdateBediener}' zum schreiben der Bedienerdatei ist nicht vorhanden. Schreibe Datei in Tempfad {tempPfad}";
        Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Warning);
        Optionen.PfadUpdateBediener = tempPfad;

        if (!Directory.Exists(tempPfad))
          throw new MyException("Temporärer Pfad zum schreibe der Bedienerdatei nicht vorhanden.");
      }

      var dat = Optionen.PfadUpdateBediener + @"\PersonalStamm.txt";
      try
      {
        File.WriteAllText(dat, sb.ToString());
        msg = $"{dat} zum Einlesen der Bedienerdatei erfolgreich erstellt !";
        Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Information);
      }
      catch (Exception f)
      {
        msg = $"Fehler beim schreiben der Bedienerdatei {dat}!\nGrund: {f.Message}";
        throw new MyException(msg);
      }
    }
  }
}
