﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineDatafoxLib
{
  public static partial class ProgDatafox
  {
    public static void BedienerInDatafoxDatei(List<tabBediener> ListeBediener , string Pfad)
    {
      var sb = new StringBuilder();
      foreach (var bediener in ListeBediener)
      {
        var bed = (bediener.Name.Length > 25) ? bediener.Name.Substring(0, 25) : bediener.Name;
        sb.AppendLine($"MITA0{bediener.MatchCode}\tMITA1{bediener.MatchCode}\t{bed}");
      }

      var dat = Pfad + @"\PersonalStamm.txt";

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
