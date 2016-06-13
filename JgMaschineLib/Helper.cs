using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JgMaschineLib
{
  public class Helper
  {
    #region Net Version abfragen

    public static string GetNetversion()
    {
      using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
      {
        int releaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));
        return CheckFor45DotVersion(releaseKey);
      }
    }

    private static string CheckFor45DotVersion(int releaseKey)
    {
      if (releaseKey >= 393295)
      {
        return "4.6 or later";
      }
      if ((releaseKey >= 379893))
      {
        return "4.5.2 or later";
      }
      if ((releaseKey >= 378675))
      {
        return "4.5.1 or later";
      }
      if ((releaseKey >= 378389))
      {
        return "4.5 or later";
      }
      // This line should never execute. A non-null release key should mean
      // that 4.5 or later is installed.
      return "No 4.5 or later version detected";
    }

    #endregion

    public static string StartVerzeichnis()
    {
      return AppDomain.CurrentDomain.BaseDirectory;
    }

    #region Windows Log Datei

    private class DatensatzWinlog
    {
      public string pText { get; set; }
      public EventLogEntryType aBild { get; set; }
    }

    public static void InWinProtokoll(string ProtokollText, EventLogEntryType AnzeigeBild = EventLogEntryType.Information)
    {
      var ds = new DatensatzWinlog() { pText = ProtokollText, aBild = AnzeigeBild };
      var t = Task.Factory.StartNew((logDatensatz) =>
      {
        var source = "JgMaschine";
        var logDs = (DatensatzWinlog)logDatensatz;

        if (!EventLog.SourceExists(source))
          EventLog.CreateEventSource(source, "Application");

        EventLog.WriteEntry(source, logDs.pText, logDs.aBild);
      }, ds);
    }

    #endregion

    public static string EntferneLetztesZeichen(string Wert, int Anzahl = 1)
    {
      if (Wert.Length > Anzahl)
        return Wert.Substring(0, Wert.Length - Anzahl);
      else
        return Wert;
    }

    public static string ListInString<T>(IEnumerable<T> Liste, string Trennzeichen, string FeldBegrenzer = "")
    {
      var sb = new StringBuilder();
      foreach (var o in Liste)
        sb.Append($"{FeldBegrenzer}{o.ToString()}{FeldBegrenzer}{Trennzeichen}");
      return EntferneLetztesZeichen(sb.ToString());
    }

    public static string DezimalInString(decimal Wert)
    {
      return Wert.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
  }
}
