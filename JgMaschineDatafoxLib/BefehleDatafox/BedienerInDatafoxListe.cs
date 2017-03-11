﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineDatafoxLib
{
  public static partial class ProgDatafox
  {
    public static void BedienerInDatafoxDatei(OptionenDatafox Optionen)
    {
      var sb = new StringBuilder();
      foreach (var bediener in Optionen.ListeBediener)
      {
        var bed = (bediener.Name.Length > 25) ? bediener.Name.Substring(0, 25) : bediener.Name;
        sb.AppendLine($"MITA0{bediener.MatchCode}\tMITA1{bediener.MatchCode}\t{bed}");
      }

      var dat = Optionen.PfadUpdateBediener;
      if (!Directory.Exists(Optionen.PfadUpdateBediener))
      {
        var msg = $"Pfad '{dat}' zum schreiben der Bedienerdatei ist nicht vorhanden.";
        throw new Exception(msg);
      }

      dat += @"\PersonalStamm.txt";
      try
      {
        File.WriteAllText(dat, sb.ToString());
      }
      catch (Exception f)
      {
        var msg = $"Fehler beim schreiben der Bedienerdatei {dat}!\nGrund: {f.Message}";
        throw new MyException(msg);
      }
    }
  }
}
