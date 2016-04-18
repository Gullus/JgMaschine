using System;
using System.Activities;
using System.Collections.Generic;

namespace JgMaschineWorkflow.Aktivitaeten
{

  public sealed class caDatenSpeichern : CodeActivity
  {
    public InArgument<List<JgMaschineData.tabDaten>> ListeDaten { get; set; }
    public OutArgument<bool> IstFehler { get; set; }
    public OutArgument<string> ProtokollText { get; set; }

    protected override void Execute(CodeActivityContext context)
    {
      string protokoll = "";
      List<JgMaschineData.tabDaten> daten = context.GetValue(this.ListeDaten); 
      
      bool istFeher = false;

      try
      {
        using(var db = new JgMaschineData.JgModelContainer())
        {
          db.tabDatenSet.AddRange(daten);          
          db.SaveChanges();
          protokoll += string.Format("Daten erfolgreich in DB gespeichert !");
        }
      }
      catch (Exception f)
      {
        istFeher = true;
        protokoll += string.Format("Fehler beim eintragen der Daten in die Datenbank\n{0}", f.Message);
      }

      context.SetValue(this.IstFehler, istFeher);
      context.SetValue(this.ProtokollText, protokoll);
    }
  }
}
