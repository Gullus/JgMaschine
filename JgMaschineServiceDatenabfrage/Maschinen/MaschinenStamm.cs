using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JgMaschineData;
using JgMaschineLib;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceDatenabfrage.Maschinen
{
  public abstract class MaschinenStamm
  {
    internal JgModelContainer _Db;
    internal string _StartPfad;
    internal List<ErgebnisAbfrage> _Ergebnisse = new List<ErgebnisAbfrage>();

    public abstract void Start(tabMaschine Maschine);

    internal bool KontrolleDirectory(tabMaschine Maschine, string Pfad)
    {
      if (!Directory.Exists(Pfad))
      {
        var msg = $"Directory {Pfad} nicht vorhanden !";
        Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
        Maschine.eProtokoll.FehlerPfadZurMaschine++;
        return false;
      }
      return true;
    }

    internal string[] StringListenLaden(string DateiName)
    {
      try
      {
        return File.ReadAllLines(DateiName);
      }
      catch (Exception f)
      {
        var msg = $"Fehler beim einlesen der Datensätze in Datei: {DateiName}!\nGrund: {f.Message}";
        throw new Exception(msg);
      }
    }

    internal void ErgebnissInDatenbank(tabMaschine Maschine, List<ErgebnisAbfrage> Ergebnisse)
    {
      var vorg = $"DatenAbgleich_Datum = '{DateTime.Now}', DatenAbgleich_Bearbeiter = '{Helper.Benutzer}', DatenAbgleich_Status = 1, IstAbgleichInJgData = 0 ";

      var vorgMitStart = "UPDATE tabBauteilSet SET DatumStart = '{1}', DatumEnde = '{2}', " 
        + vorg  + "WHERE IdStahlPosition = '{0}'\n";
      var vorgNurDauer = "UPDATE tabBauteilSet SET DatumEnde = DatumStart + '{1}', " 
        + vorg + "WHERE IdStahlPosition = '{0}'\n";

      var sb = new StringBuilder();
      foreach (var erg in Ergebnisse)
      {
        if (erg.Start != null)
          sb.AppendFormat(vorgMitStart, erg.Schluessel, erg.Start, erg.Start + erg.Dauer);
        else
          sb.AppendFormat(vorgNurDauer, erg.Schluessel, erg.Dauer.ToString());
      }

      try
      {
        var anzErfolg = _Db.Database.ExecuteSqlCommand(sb.ToString(), null);
        var msg = $"Datensätze aktualisiert: {anzErfolg} von {Ergebnisse.Count}";
        if (anzErfolg == Ergebnisse.Count)
          Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
        else
          Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
      }
      catch (Exception f)
      {
        var msg = $"Fehler beim aktualisieren der Bauteile in der Datenbank!\nGrund: {f.Message}";
        throw new Exception(msg);
      }
    }
  }

  class ErgebnisAbfrage
  {
    public string Schluessel { get; set; }
    public DateTime? Start { get; set; }
    public TimeSpan Dauer { get; set; }
  }
}
