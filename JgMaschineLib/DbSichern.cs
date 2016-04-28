using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JgMaschineLib
{
  public static class DbSichern
  {
    public static string Benutzer = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

    public static void AbgleichEintragen(JgMaschineData.DatenAbgleich DatenAbgl, JgMaschineData.EnumStatusDatenabgleich Status)
    {
      DatenAbgl.Status = Status;
      DatenAbgl.Datum = DateTime.Now;
      DatenAbgl.Bearbeiter = Benutzer;
    }

    public static async void DsSichern<T>(JgMaschineData.JgModelContainer Db, object MeinObjekt, JgMaschineData.EnumStatusDatenabgleich Status, bool Speichern = true)
      where T : class
    {
      var ent = Db.Set<T>();
      var ds = Db.Entry<T>((T)MeinObjekt);

      var abgl = ds.Member<JgMaschineData.DatenAbgleich>("DatenAbgleich");
      abgl.CurrentValue.Datum = DateTime.Now;
      abgl.CurrentValue.Status = Status;
      abgl.CurrentValue.Bearbeiter = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

      var fehler = ds.GetValidationResult();

      if (Status == JgMaschineData.EnumStatusDatenabgleich.Neu)
        ent.Add(ds.Entity);

      await Db.SaveChangesAsync();
    }
  }
}
