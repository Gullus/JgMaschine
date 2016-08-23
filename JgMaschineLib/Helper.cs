using System;
using System.Windows.Forms;
using Microsoft.Win32;

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

    public static string EntferneLetztesZeichen(string Wert, int Anzahl = 1)
    {
      if (Wert.Length > Anzahl)
        return Wert.Substring(0, Wert.Length - Anzahl);
      else
        return Wert;
    }

    public static string ListInString<T>(T[] Liste, string Trennzeichen, string FeldBegrenzer = "")
    {
      return FeldBegrenzer + string.Join(FeldBegrenzer + Trennzeichen + FeldBegrenzer, Liste) + FeldBegrenzer;
    }

    public static string DezimalInString(decimal Wert)
    {
      return Wert.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    #region Anzeige von Protokollen verwalten

    public enum ProtokollArt
    {
      Info,
      Warnung,
      Fehler
    }

    public static void Protokoll(string ProtokollText, ProtokollArt ProtokollArt = ProtokollArt.Fehler)
    {
      string caption = "Fehler!";
      MessageBoxIcon icon = MessageBoxIcon.Error;

      switch (ProtokollArt)
      {
        case ProtokollArt.Info:
          caption = "Information";
          icon = MessageBoxIcon.Information;
          break;
        case ProtokollArt.Warnung:
          caption = "Warnung!";
          icon = MessageBoxIcon.Warning;
          break;
      }
      MessageBox.Show(ProtokollText, caption, MessageBoxButtons.OK, icon);
    }

    public static void Protokoll(string FehlerText, Exception Fehler)
    {
      var msg = $"{FehlerText}\nGrund: {Fehler.Message}";
      var inExcep = Fehler.InnerException;
      int zaehler = 0;
      while (inExcep != null)
      {
        zaehler++;
        msg += $"\nInExcep {zaehler}: {inExcep.Message}";
        inExcep = inExcep.InnerException;
      }

      Protokoll(msg);
    }

    #endregion

    public static void SetzeBackflash(ref string Pfad)
    {
      if (Pfad[Pfad.Length - 1] != '\\')
        Pfad += '\\';
    }

    public static DateTime DatumAusYyyyMMdd(string AusString)
    {
      return new DateTime(Convert.ToInt32(AusString.Substring(0, 4)), Convert.ToInt32(AusString.Substring(4, 2)), Convert.ToInt32(AusString.Substring(6, 2)));
    }
  }
}
