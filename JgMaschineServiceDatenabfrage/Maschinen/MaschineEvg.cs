using System;
using System.Collections.Generic;
using System.IO;
using JgMaschineData;
using JgMaschineLib;
using System.Linq;

namespace JgMaschineServiceDatenabfrage.Maschinen
{
  internal class MaschineEvg : MaschinenStamm
  {
    public MaschineEvg(JgModelContainer Db, Proto Protokoll, string StartPfad)
    {
      _Db = Db;
      _Protokoll = Protokoll;
      _StartPfad = StartPfad;
    }

    public override void Start(tabMaschine Maschine)
    {
      var pfadStart = @"\\" + Maschine.MaschineAdresse + @"\" + _StartPfad;
      Helper.SetzeBackflash(ref pfadStart);

      if (KontrolleDirectory(Maschine, pfadStart))
      {
        // alle relevante Dateien aus Verzeichnissen laden -> Stammverzeichnis ...\EVG\Eingabe\Monit\(Jahr)\(HahrMonat)\....

        var heute = DateTime.Now.Date;
        var durchlauf = Maschine.eProtokoll.LetzteDateiDatum;
        var dateienAuswertung = new List<string>();

        while (durchlauf <= heute)
        {
          var dirMonatJahr = string.Format(@"{0}\{1}\{1}{2}", pfadStart, durchlauf.Year, durchlauf.Month.ToString("D2"));
          dateienAuswertung.AddRange(Directory.EnumerateFiles(dirMonatJahr, "F_*.mon").ToList());
          durchlauf.AddMonths(1);
        };

        var merkeLetzteDatum = Maschine.eProtokoll.LetzteDateiDatum;
        var merkeLetzteZeile = Maschine.eProtokoll.LetzteZeile;
        var merkeZeileBauteil = 0;

        foreach (var datei in dateienAuswertung)
        {
          var datum = Helper.DatumAusYyyyMMdd(Path.GetFileName(datei).Substring(2));

          if (datum < Maschine.eProtokoll.LetzteDateiDatum)
            continue;

          var lZeilen = StringListenLaden(datei);

          if (datum > Maschine.eProtokoll.LetzteDateiDatum)
            Maschine.eProtokoll.LetzteDateiDatum = datum;

          ErgebnisAbfrage ergNeu = null;
          var zeileStart = 0;

          if (datum == merkeLetzteDatum)
            zeileStart = merkeLetzteZeile;

          for (int zeile = zeileStart; zeile < lZeilen.Length; zeile++)
          {
            if (lZeilen[zeile][7] == 'A')
            {
              merkeZeileBauteil = zeile;
              ergNeu = new ErgebnisAbfrage() { Start = GetDatum(datum, lZeilen[zeile]) };
              _Ergebnisse.Add(ergNeu);

              var felder = lZeilen[zeile].Split(new char[] { ';' }, StringSplitOptions.None);
              try
              {
                ergNeu.Schluessel = felder[4];
              }
              catch (Exception f)
              {
                var msg = $"Fehler beim konvertieren der Id {felder[4]} in Zeile: {zeile}.\nGrund: {f.Message}";
                throw new Exception(msg);
              }
            }
            else if ((ergNeu != null) && (lZeilen[zeile][7] == 'D'))
              ergNeu.Dauer = (DateTime)ergNeu.Start - GetDatum(datum, lZeilen[zeile]);
          }

          if (datum == Maschine.eProtokoll.LetzteDateiDatum)
            Maschine.eProtokoll.LetzteZeile = merkeZeileBauteil;
        }

        ErgebnissInDatenbank(Maschine, _Ergebnisse);
      }
    }

    private DateTime GetDatum(DateTime Datum, string Zeile)
    {
      return Datum + new TimeSpan(Convert.ToInt32(Zeile.Substring(0, 2)), Convert.ToInt32(Zeile.Substring(2, 2)), Convert.ToInt32(Zeile.Substring(4, 2)));
    }
  }
}