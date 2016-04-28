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

namespace JgMaschineTest
{
  class Program
  {
    static void Main(string[] args)
    {

      int i = 1;
      Console.WriteLine(i.ToString("D2"));



      using (var db = new JgModelContainer())
      {
        Console.WriteLine(Convert.ToChar(13));
       
      }

      Console.WriteLine("Fertsch");
      Console.ReadKey();
    }

    private static void AddDs<T>(T Datensatz)
      where T : class
    {
      using (var db = new JgModelContainer())
      {
        var g = db.Set<T>();
        var jj = db.Entry<T>(Datensatz);
        g.Add(jj.Entity);
        db.SaveChanges();
      }
    }


    private void XmlDateiEinlesen()
    {
      string datXml = @"C:\Entwicklung\JgMaschine\JgMaschineTest\bin\Debug\20151102_produzione.xml";
      XDocument xdoc = XDocument.Load(datXml);

      XElement root = xdoc.Root;
      var dataName = "{" + root.GetDefaultNamespace().NamespaceName + "}" + "DATA";
      XElement rootStart = root.Elements().FirstOrDefault(z => z.Name == dataName);

      foreach (var t in rootStart.Elements())
      {
        Console.WriteLine(t.Attribute("date").Value + "  " + t.Attribute("time_on").Value);
        foreach (var w in t.Elements())
        {
          Console.WriteLine("   " + w.Attribute("time_work").Value);
        }
      }
    }
  }
}




//string s = "081015";

//var t = Convert.ToInt32(s.Substring(0, 2));
//var m = Convert.ToInt32(s.Substring(2, 2));
//var j = Convert.ToInt32("20" + s.Substring(4, 2));

//Console.WriteLine(j + "  " + m + "  " + t);


//var dt = new DateTime(Convert.ToInt32("20" + s.Substring(4, 2)), Convert.ToInt32(s.Substring(2, 2)), Convert.ToInt32(s.Substring(0, 2)));

//Console.WriteLine(dt.ToString());

//int von = 0;
//string[] ar = new string[] { "Hallo", "Ballo" };

//von = 5;
//ar = new string [] { "Hallo", "Ballo", "Juhu", "Arsch", "Mutti", "Oma" };
//if (ar.Length > von)
//{
//  string[] temp = new string[ar.Length - von];
//  for (int i = 0; i < temp.Length; i++)
//    temp[i] = ar[i + von];

//  ar = new string[temp.Length];
//  temp.CopyTo(ar, 0);

//  foreach (var s in ar)
//    Console.WriteLine(s);
//}
//else
//  Console.WriteLine("Ffler");

//using(var db = new JgMaschineData.JgModelContainer())
//{
//  try
//  {

//  }
//  catch (Exception f)
//  {
//    Console.WriteLine(f.Message);
//  }
//};
