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
    private static void DbSichernVorbereitung<T>(JgMaschineData.JgModelContainer Db, object MeinObjekt, JgMaschineData.EnumStatusDatenabgleich Status) where T : class
    {
      var ent = Db.Set<T>();
      var ds = Db.Entry<T>((T)MeinObjekt);

      var abgl = ds.Member<JgMaschineData.DatenAbgleich>("DatenAbgleich");
      AbgleichEintragen(abgl.CurrentValue, Status);

      if (Status == JgMaschineData.EnumStatusDatenabgleich.Neu)
        ent.Add(ds.Entity);
    }
    public static void DsSichern<T>(JgMaschineData.JgModelContainer Db, object MeinObjekt, JgMaschineData.EnumStatusDatenabgleich Status)
      where T : class
    {
      DbSichernVorbereitung<T>(Db, MeinObjekt, Status);
      Db.SaveChanges();
    }

    public static async Task DsSichernAsync<T>(JgMaschineData.JgModelContainer Db, object MeinObjekt, JgMaschineData.EnumStatusDatenabgleich Status)
      where T : class
    {
      DbSichernVorbereitung<T>(Db, MeinObjekt, Status);
      await Db.SaveChangesAsync();
    }
  }
}
