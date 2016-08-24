using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JgMaschineData;

namespace JgMaschineDatafoxLib
{
  public static partial class ProgDatafox
  {
    public static void BedienerInDatafoxDatei(OptionenDatafox Optionen, List<tabBediener> ListeBediener)
    {
      var sb = new StringBuilder();
      foreach (var bediener in ListeBediener)
      {
        var bed = (bediener.Name.Length > 25) ? bediener.Name.Substring(0, 25) : bediener.Name;
        sb.AppendLine($"MITA0{bediener.MatchCode}\tMITA1{bediener.MatchCode}\t{bed}");
      }

      var dat = Optionen.PfadUpdateBediener + @"\PersonalStamm.txt";

      try
      {
        File.WriteAllText(dat, sb.ToString());
      }
      catch (Exception f)
      {
        var msg = $"Fehler beim schreiben der Bedienerdatei {dat}!\nGrund: {f.Message}";
        throw new Exception(msg);
      }
    }
  }
}
