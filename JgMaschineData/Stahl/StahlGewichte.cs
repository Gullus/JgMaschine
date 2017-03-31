using System;
using System.Globalization;

namespace JgMaschineData
{
  public static class StahlGewichte
  {
    public static decimal GetGewichtKg(int Durchmesser, int Laenge)
    {
      return Math.Round(Get(Durchmesser) * Laenge / 100, 3);
    }

    public static String GetGewichtKgAsString(int Durchmesser, int Laenge)
    {
      return GetGewichtKg(Durchmesser, Laenge).ToString("N3", CultureInfo.InvariantCulture);
    }

    public static decimal Get(int Durchmesser)
    {
      var gewichtProMeter = 1m;
      switch (Durchmesser)
      {
        case 4: gewichtProMeter = 0.099m; break;
        case 5: gewichtProMeter = 0.154m; break;
        case 6: gewichtProMeter = 0.222m; break;
        case 7: gewichtProMeter = 0.302m; break;
        case 8: gewichtProMeter = 0.395m; break;
        case 9: gewichtProMeter = 0.499m; break;
        case 10: gewichtProMeter = 0.617m; break;
        case 11: gewichtProMeter = 0.746m; break;
        case 12: gewichtProMeter = 0.888m; break;
        case 14: gewichtProMeter = 1.21m; break;
        case 16: gewichtProMeter = 1.58m; break;
        case 20: gewichtProMeter = 2.47m; break;
        case 25: gewichtProMeter = 3.85m; break;
        case 28: gewichtProMeter = 4.83m; break;
        case 32: gewichtProMeter = 6.31m; break;
        case 40: gewichtProMeter = 9.86m; break;
      }

      return gewichtProMeter;
    }
  }
}
