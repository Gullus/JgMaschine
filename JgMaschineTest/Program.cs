using JgMaschineData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Data.Entity;
using System.IO;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using JgMaschineLib.Scanner;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main()
    {

      var s = "123456789";
      Console.WriteLine(s.Substring(5));
      Console.WriteLine(s.Substring(5, s.Length - 6));





      //var cs = db.Database.Connection.ConnectionString;

      //IQueryable iq = new JgMaschineLib.JgList<tabStandort>()


      //List<JgMaschineData.tabBediener> lb = null;



      //if (validationResults.Count > 0 && errors == null)
      //  errors = new Dictionary<string, string>(validationResults.Count);

      //foreach (var validationResult in validationResults)
      //{
      //  errors.Add(validationResult.MemberNames.First(), validationResult.ErrorMessage);
      //}




      //var test = db.Entry(st). .GetValidationResult();

      //foreach (var f in test.ValidationErrors)
      //{
      //  Console.WriteLine(f.GetType() + "  " + f.PropertyName + "  " + f.ErrorMessage);
      //}

      //db.tabStandortSet.Add(st);

      //db.SaveChanges();


      //  //var t = lb[0];
      //lb[0].NachName = "Juhu99999";

      //using (var db = new JgMaschineData.JgModelContainer())
      //{
      //  db.tabBedienerSet.Attach(lb[0]);
      //  //db.Entry(lb[0]).State = EntityState.Modified;
      //  db.SaveChanges();
      //}

      //foreach (var d1 in lb)
      //  Console.WriteLine($"{d1.Id} {d1.NachName}");

      //using (var db = new JgMaschineData.JgModelContainer())
      //{
      //  lb = db.tabBedienerSet.ToList();

      //  foreach (var d1 in lb)
      //    Console.WriteLine($"{d1.Id} {d1.NachName}");
      //}

      Console.ReadKey();
      //st.Close();
    }

    private static string StringInChar(string KommaString)
    {
      string erg = "";
      var ar = KommaString.Split(new char[] { ',', ';' });
      foreach (var s in ar)
        erg += Convert.ToChar(Convert.ToByte(s));
      return erg;
    }


    private static string ByteInChar(params byte[] ByteWerte)
    {
      string erg = "";
      foreach (var b in ByteWerte)
        erg += Convert.ToChar(b).ToString();
      return erg;
    }
  }
}

