using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using JgMaschineData;

namespace JgMaschineLib
{
  public static class JgList
  {
    public static async Task ListeAktualisieren<T>(JgModelContainer Db, IEnumerable<T> Liste)
      where T : class
    {
      var propId = typeof(T).GetProperty("Id");
      var propDateAbleich = typeof(T).GetProperty("DatenAbgleich");

      var dicDs = new SortedDictionary<Guid, T>();
      foreach (var ds in Liste)
        dicDs.Add((Guid)propId.GetValue(ds), ds);

      // geänderte Datensätze aktualisieren, Wenn mehr als 500 Datensätze dann keine Aktualisierung
      if ((dicDs.Count != 0) && (dicDs.Count < 2000))
      {
        var sbAbfrageText = new StringBuilder();
        foreach (var ds in dicDs)
          sbAbfrageText.AppendLine($"  ('{ds.Key.ToString()}','{(propDateAbleich.GetValue(ds.Value) as DatenAbgleich).Datum.ToString("dd.MM.yyyy HH:mm:ss")}'),");

        var sqlText = "IF OBJECT_ID(N'tempdb..#TempDs', N'U') IS NOT NULL DROP TABLE #TempDs \n"
                    + "CREATE TABLE #TempDs (Id uniqueidentifier NOT NULL, Datum char(19) NOT NULL) \n"
                    + "INSERT INTO #TempDs VALUES \n"
                    + sbAbfrageText.ToString().Substring(0, sbAbfrageText.ToString().Length - 3) + "\n"
                    + "SELECT Id FROM #TempDs as t \n"
                    + "  WHERE EXISTS(SELECT * FROM " + typeof(T).Name + "Set WHERE (Id = t.Id) AND (FORMAT(DatenAbgleich_Datum , 'dd/MM/yyyy HH:mm:ss') <> t.Datum))";

        var idisAendern = new List<Guid>();
        var sc = Db.Database.Connection.ConnectionString;
        using (var con = new SqlConnection(sc))
        {
          await con.OpenAsync();
          var cl = new SqlCommand(sqlText, con);
          using (var reader = await cl.ExecuteReaderAsync())
          {
            while (reader.Read())
              idisAendern.Add((Guid)reader[0]);
          }
        }

        foreach (var ds in idisAendern)
          await Db.Entry<T>(dicDs[ds]).ReloadAsync();
      }
    }
  }
}
