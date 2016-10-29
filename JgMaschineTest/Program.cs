﻿using System;
using System.Linq;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main(string[] args)
    {
      using (var db = new JgMaschineData.JgModelContainer())
      {
        var dsAuswertungVormonat = db.tabArbeitszeitAuswertungSet.FirstOrDefault();

        Console.WriteLine(dsAuswertungVormonat.Urlaub);
      }


        Console.ReadKey();
    }

    private static double NachtZeitBerechnen(int NachtschichtStundeVon, int NachtschichtMinuteVon, int LaengeNachtschichtStunde, int LaengeNachtschichtMinute,
      DateTime DatumVon, DateTime DatumBis)
    {
      var sumNachtschicht = 0d;

      // Damit Frühschicht berücksichtigt wird, beginnt Nchtschicht ein Tag vorher 
      var mDatum = DatumVon.Date.AddDays(-1);

      while (true)
      {
        var nsBeginn = new DateTime(mDatum.Year, mDatum.Month, mDatum.Day, NachtschichtStundeVon, NachtschichtMinuteVon, 0);
        var nsEnde = nsBeginn.AddHours(LaengeNachtschichtStunde).AddMinutes(LaengeNachtschichtMinute);
        mDatum = mDatum.AddDays(1);

        if (DatumBis < nsBeginn)
          break;

        if (DatumVon < nsBeginn)
          DatumVon = nsBeginn;

        if ((DatumVon >= nsBeginn) && (DatumVon < nsEnde))
        {
          if (DatumBis <= nsEnde)
          {
            sumNachtschicht += (DatumBis - DatumVon).TotalMinutes;
            break;
          }
          sumNachtschicht += (nsEnde - DatumVon).TotalMinutes;
        }
      };

      return sumNachtschicht;
    }
  }
}


