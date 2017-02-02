using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main(string[] args)
    {

      var t = new TimeSpan(148, 16, 00);
      var d = DateTime.Now;

      using (var db = new JgMaschineData.JgModelContainer())
      {
        XDocument xDoc = new XDocument(
          new XComment("This is a comment"),
          new XElement("Root",

            from z in db.tabBedienerSet.ToList()
            select new XElement("Datensatz", 
              new XElement("Nachname", z.NachName),
              new XElement("Vorname", z.VorName),
              new XElement("Zeit", t.ToString() + ".000"),
              //new XElement("Datum1", d.ToString("yyyy-MM-dd")),
              new XElement("Datum2", d.ToString("yyyy-MM-ddTHH:mm:ss"))
            )
          )
        );


        Console.WriteLine(xDoc);
      }

      Console.ReadKey();
    }
  }

}

