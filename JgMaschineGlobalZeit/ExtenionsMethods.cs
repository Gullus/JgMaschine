using System;

namespace JgMaschineGlobalZeit
{
  public static class ExtenionsMethods
  {
    public static string Ausgabe(this TimeSpan Ergebniss)
    {
      return ((int)Ergebniss.TotalHours).ToString("D2") + ":" + Ergebniss.Minutes.ToString("D2");
    }
  }
}

