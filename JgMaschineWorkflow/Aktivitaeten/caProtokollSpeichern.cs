using System;
using System.Activities;

namespace JgMaschineWorkflow.Aktivitaeten
{
  public sealed class caProtokollSpeichern : CodeActivity
  {
    public InArgument<JgMaschineData.tabMaschine[]> ListeMaschinen { get; set; }

    protected override void Execute(CodeActivityContext context)
    {
      JgMaschineData.tabMaschine[] maschinen = context.GetValue(ListeMaschinen);

      try
      {
        using (var db = new JgMaschineData.JgModelContainer())
        {
          foreach (var maschine in maschinen)
          {
            db.tabProtokollSet.Attach(maschine.eProtokoll);
            db.Entry(maschine.eProtokoll).State = System.Data.Entity.EntityState.Modified;
          }

          db.SaveChanges();
        }
        Console.WriteLine("Protokoll gespeichert.");
      }
      catch (Exception f)
      {
        Console.WriteLine(string.Format("Fehler beim speichern der Protokolle in die Datenbank\n{0}", f.Message));
      }
    }
  }
}
