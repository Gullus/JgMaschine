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

      string s1 = "BF2D@HjTestPDF@r417@ia@p1@l1000@n10@e0.888@d12@g500S@s48@v@Gl400@w90@l600@w0@C88@" + System.Environment.NewLine;

      JgMaschineLib.Stahl.BvbsDatenaustausch bvs = new JgMaschineLib.Stahl.BvbsDatenaustausch(s1);


      Console.WriteLine(bvs.ObjectInBvsStream());


      Console.ReadKey();

      return;



      var ds = new JgMaschineData.tabDaten();



      var dd = new DateTime();
      Console.WriteLine(dd);

      Console.ReadKey();
      return;


      Nullable<Int32> na = 1852;

      Console.WriteLine(na);


      var s = "20151012";
      Console.WriteLine(s.Substring(0, 4) + s.Substring(4, 2) + s.Substring(6, 2));

      var dt = (new DateTime(Convert.ToInt32(s.Substring(0, 4)), Convert.ToInt32(s.Substring(4, 2)), Convert.ToInt32(s.Substring(6, 2)))).AddSeconds(24156);

      Console.WriteLine(dt.ToString());


      Console.ReadKey();
      return;


      string pfad = @"c:\entwicklung\";  //  @"C:\Entwicklung\JgMaschine\Versuchsdaten\file_prod_coilcan\";
      string test = "Report.txt";
      

      try
      {
        var files = Directory.EnumerateFiles(pfad, "*.*", SearchOption.TopDirectoryOnly);

        foreach (var f in files)
        {
          Console.WriteLine(f);
          Console.WriteLine(f.IndexOf(test));

          //FileInfo info = new FileInfo(f);
          //string ff = info.Extension;

          //Console.WriteLine("{0}",  info.Name.Substring(0, info.Name.LastIndexOf(ff)));
        }
        Console.WriteLine("{0} files found.",
          files.Count().ToString());
      }
      catch (UnauthorizedAccessException UAEx)
      {
        Console.WriteLine(UAEx.Message);
      }
      catch (PathTooLongException PathEx)
      {
        Console.WriteLine(PathEx.Message);
      }

      Console.ReadKey();
      return;


      tabMaschine ma = null;

      using (var db = new JgMaschineData.JgModelContainer())
      {
        ma = db.tabMaschineSet.Include(i => i.eStandort).FirstOrDefault();
        Console.WriteLine(ma.MaschinenName);
      }

      ma.eStandort.Bezeichnung = "Hallo";

      using (var db = new JgMaschineData.JgModelContainer())
      {
        db.tabMaschineSet.Attach(ma);
        Console.WriteLine(ma.MaschinenName);
        db.Entry(ma).State = System.Data.Entity.EntityState.Modified;
        db.SaveChanges();
      }

      using (var db = new JgMaschineData.JgModelContainer())
      {
        ma = db.tabMaschineSet.FirstOrDefault();
        Console.WriteLine(ma.MaschinenName);
      }

      Console.WriteLine("Fertsch");
      Console.ReadKey();
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
