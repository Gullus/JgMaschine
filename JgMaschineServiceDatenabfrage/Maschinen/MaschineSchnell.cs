using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineServiceDatenabfrage.Maschinen
{
  internal class MaschineSchnell : MaschinenStamm
  {
    public MaschineSchnell(JgModelContainer Db, string StartPfad)
    {
      _Db = Db;
      _StartPfad = StartPfad;
    }

    public override void Start(tabMaschine Maschine)
    {
      var merkeLetzteDatum = Maschine.eProtokoll.LetzteDateiDatum;
      var merkeLetzteZeile = Maschine.eProtokoll.LetzteZeile;

      var pfadStart = @"\\" + Maschine.MaschineAdresse + @"\" + _StartPfad;
      Helper.SetzeBackflash(ref pfadStart);

      if (KontrolleDirectory(Maschine, pfadStart))
      {
        var dateienAuswertung = Directory.EnumerateFiles(pfadStart, "20*_produzione.xml", SearchOption.TopDirectoryOnly).ToList();

        foreach (var datei in dateienAuswertung)
        {
          var datumDat = Helper.DatumAusYyyyMMdd(Path.GetFileName(datei));

          if (datumDat < Maschine.eProtokoll.LetzteDateiDatum)
            continue;

          XDocument xdoc = XDocument.Load(datei);
          XElement root = xdoc.Root;
          var dataName = "{" + root.GetDefaultNamespace().NamespaceName + "}" + "DATA";
          XElement rootStart = root.Elements().FirstOrDefault(z => z.Name == dataName);

          foreach (var elementDatum in rootStart.Elements())
          {
            var datum = Helper.DatumAusYyyyMMdd(elementDatum.Attribute("date").Value);

            if (datum <= Maschine.eProtokoll.LetzteDateiDatum)
              continue;

            int zaehler = 0;
            foreach (var elementBauteil in elementDatum.Elements())
            {
              zaehler++;
              if ((datum == Maschine.eProtokoll.LetzteDateiDatum) && (zaehler < Maschine.eProtokoll.LetzteZeile))
                continue;

              _Ergebnisse.Add(new ErgebnisAbfrage()
              {
                // Schluessel =  Convert.ToInt32(w.Attribute("time_work").Value),
                Dauer = new TimeSpan(0, 0, Convert.ToInt32(elementBauteil.Attribute("time_work").Value))
              });
            }

            if (datum >= merkeLetzteDatum)
            {
              merkeLetzteDatum = datum;
              merkeLetzteZeile = zaehler;
            }
          }
        }

        Maschine.eProtokoll.LetzteDateiDatum = merkeLetzteDatum;
        Maschine.eProtokoll.LetzteZeile = merkeLetzteZeile;
      }
    }
  }
}
