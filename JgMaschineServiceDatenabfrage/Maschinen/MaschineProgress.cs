using System;
using System.IO;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineServiceDatenabfrage.Maschinen
{
  internal class MaschineProgress : MaschinenStamm
  {
    public MaschineProgress(JgModelContainer Db, Proto Protokoll, string PfadStart)
    {
      _Db = Db;
      _Protokoll = Protokoll;
      _StartPfad = PfadStart;
    }

    public override void Start(tabMaschine Maschine)
    {
      var pfadStart = @"\\" + Maschine.MaschineAdresse + @"\" + _StartPfad;
      Helper.SetzeBackflash(ref pfadStart);

      if (KontrolleDirectory(Maschine, pfadStart))
      {
        var datTemp = pfadStart + "JgDaten.Txt";
        if (File.Exists(datTemp))
          DatenAuslesen(Maschine, datTemp);

        if (!File.Exists(datTemp))
        {
          var datOrginal = pfadStart + "StatExport.txt";
          if (File.Exists(datOrginal))
          {
            File.Move(datOrginal, datTemp);
            if (File.Exists(datTemp))
              DatenAuslesen(Maschine, datTemp);
          }
        }
      }
    }

    private void FehlerAusloesen(string FeldName, int NummerZeile, string Zeile, int FeldIndext, string Wert, Exception Fehler)
    {
      var msg = $"Fehler bei Konvertierung der Importdaten Feld '{FeldName} in Zeile {NummerZeile}."
        + $"\nIndex: {FeldIndext} Wert: {Wert}\nDatensatz: {Zeile}\nFehler: {Fehler.Message}";
      throw new Exception(msg);
    }

    private void DatenAuslesen(tabMaschine Maschine, string DateiName)
    {
      int zaehler = 0;
      string msg = "";
      _Ergebnisse.Clear();

      var lZeilen = StringListenLaden(DateiName);

      if (lZeilen != null)
      {
        foreach (var zeile in lZeilen)
        {
          zaehler++;
          var felder = zeile.Split(new char[] { ';' }, StringSplitOptions.None);
          var erg = new ErgebnisAbfrage();

          try
          {
            erg.Start = Convert.ToDateTime(felder[6]);
          }
          catch (Exception f)
          {
            FehlerAusloesen("DatumStart", zaehler, zeile, 6, felder[6], f);
          }

          try
          {
            erg.Dauer = Convert.ToDateTime(felder[7]) - (DateTime)erg.Start;
          }
          catch (Exception f)
          {
            FehlerAusloesen("DatumEnde", zaehler, zeile, 7, felder[7], f);
          }

          try
          {
            erg.Schluessel = felder[2];
          }
          catch (Exception f)
          {
            FehlerAusloesen("NummerPosition", zaehler, zeile, 2, felder[2], f);
          }

          _Ergebnisse.Add(erg);
        }

        ErgebnissInDatenbank(Maschine, _Ergebnisse);

        try
        {
          File.Delete(DateiName);
        }
        catch (Exception f)
        {
          msg = $"Datei {DateiName} konnte nicht gelöscht werden !\nGrund: {f.Message}";
          throw new Exception(msg);
        }
      }
    }
  }
}