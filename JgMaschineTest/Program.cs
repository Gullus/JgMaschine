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

namespace JgMaschineTest
{
  class Programm
  {
    static void Main()
    {
      var db = new JgModelContainer();
      var cs = db.Database.Connection.ConnectionString;

      //IQueryable iq = new JgMaschineLib.JgList<tabStandort>()


      //List<JgMaschineData.tabBediener> lb = null;

      //using (var db = new JgMaschineData.JgModelContainer())
      //{
      //  lb = db.tabBedienerSet.ToList();

      //  foreach (var d1 in lb)
      //    Console.WriteLine($"{d1.Id} {d1.NachName}");
      //}

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
  }
}

