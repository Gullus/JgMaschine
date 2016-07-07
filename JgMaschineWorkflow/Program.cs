using System;
using System.Activities;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Data.Entity;

namespace JgMaschineWorkflow
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Pogramm gestartet !");

      var erg = WorkflowStarten(args);

      Console.WriteLine("Programm " + (erg ? "erfolgreich beendet." : "mit Fehlern beendet !"));
      Console.ReadKey();
    }

    private static bool WorkflowStarten(string[] args)
    {
      string ss = "Durch Leerzeichen getrennt, können verschiedene Standorte für Abfrage eingegegeben werden.";

      if (args.Length == 0)
      {
        Console.WriteLine("Es wurden keine Standorte Angegeben !");
        Console.WriteLine(ss);
        return false;
      }

      if (args[0].Contains("?"))
      {
        Console.WriteLine("Hilfe zu Workflow");
        Console.WriteLine(ss);
        return false;
      }

      var standorte = new string[args.Length];
      for (int i = 0; i < standorte.Length; i++)
        standorte[i] = args[i].ToUpper();

      var maschinen = new List<JgMaschineData.tabMaschine>();

      try
      {
        using (var db = new JgMaschineData.JgModelContainer())
        {
          maschinen = db.tabMaschineSet
            .Where(w => (standorte.Contains(w.eStandort.Bezeichnung.ToUpper())) && (w.ProtokollName != JgMaschineData.EnumProtokollName.Handbiegung) 
              && (w.Status == JgMaschineData.EnumStatusMaschine.InBetrieb))
            .Include(i => i.eProtokoll).Include(e => e.eStandort)
            .OrderBy(o => o.eStandort.Bezeichnung).ThenBy(t => t.MaschinenName).ToList();

          foreach (var maschine in maschinen)
            maschine.eProtokoll.ProtokollText = string.Format("{0} Start Workflow\n", DateTime.Now.ToString("dd.MM HH:mm:ss"));
        }
      }
      catch (Exception f)
      {
        Console.WriteLine("Fehler beim Initialisieren der Maschinendaten aus der Datenbank!\nGrund: " + f.Message);
        return false;
      }

      if (maschinen.Count == 0)
      {
        Console.WriteLine("Keine Maschinen mit den Standorten vorhanden !");
        foreach(var so in args)
          Console.WriteLine("  " + so);
        return false;
      }

      Console.WriteLine("Folgende Standorte und Maschinen wurden gefunden:");
      foreach (var ma in maschinen)
        Console.WriteLine("  {0,-30} {1}", ma.eStandort.Bezeichnung, ma.MaschinenName);

      Console.WriteLine();
      Console.WriteLine("Workflow wird gestartet");

      Dictionary<string, object> wfArgs = new Dictionary<string, object>();
      wfArgs.Add("ListeMaschinen", maschinen.ToArray());

      Activity wf = new JgWorkflow();
      try
      {
        WorkflowInvoker.Invoke(wf, wfArgs);
      }
      catch (Exception f)
      {
        Console.WriteLine("Im Workflow ist ein Fehler aufgetreten !");
        Console.WriteLine(f.Message);
        return false;
      }

      return true;
    }
  }
}
