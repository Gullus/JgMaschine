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

namespace JgMaschineTest
{

  class TimerExample
  {

    static void Main()
    {
      var adresse = Properties.Settings.Default.Adresse;
      var port = Properties.Settings.Default.PortNummer;

      try
      {
        Console.WriteLine("Client starten");

        var cl = new TcpClient(adresse, port);
        var ns = cl.GetStream();

        Console.WriteLine("Client gestartet");

        while (true)
        {
          var buffer = new byte[4096];
          var anz = ns.Read(buffer, 0, buffer.Length);

          var returndata = Encoding.UTF8.GetString(buffer, 0, anz);

          Console.WriteLine(returndata);

          var sendBytes = Encoding.UTF8.GetBytes(Convert.ToChar(13).ToString());
          ns.Write(sendBytes, 0, sendBytes.Length);
        }
      }
      catch (Exception f)
      {
        Console.WriteLine(f.Message);
      }

      Console.ReadKey();
    }


    //static void Main()
    //{
    //  var adresse = Properties.Settings.Default.Adresse;
    //  var port = Properties.Settings.Default.PortNummer;

    //  var dl = new DataLogicScanner(adresse, port, null, true);
    //  dl.Start();

    //  Console.ReadKey();
    //}
  }


  //class Program
  //{
  //  static void Main(string[] args)
  //  {

  //    int i = 1;
  //    Console.WriteLine(i.ToString("D2"));



  //    using (var db = new JgModelContainer())
  //    {
  //      Console.WriteLine(Convert.ToChar(13));

  //    }

  //    Console.WriteLine("Fertsch");
  //    Console.ReadKey();
  //  }

  //  private static void AddDs<T>(T Datensatz)
  //    where T : class
  //  {
  //    using (var db = new JgModelContainer())
  //    {
  //      var g = db.Set<T>();
  //      var jj = db.Entry<T>(Datensatz);
  //      g.Add(jj.Entity);
  //      db.SaveChanges();
  //    }
  //  }

  //  private void XmlDateiEinlesen()
  //  {
  //    string datXml = @"C:\Entwicklung\JgMaschine\JgMaschineTest\bin\Debug\20151102_produzione.xml";
  //    XDocument xdoc = XDocument.Load(datXml);

  //    XElement root = xdoc.Root;
  //    var dataName = "{" + root.GetDefaultNamespace().NamespaceName + "}" + "DATA";
  //    XElement rootStart = root.Elements().FirstOrDefault(z => z.Name == dataName);

  //    foreach (var t in rootStart.Elements())
  //    {
  //      Console.WriteLine(t.Attribute("date").Value + "  " + t.Attribute("time_on").Value);
  //      foreach (var w in t.Elements())
  //      {
  //        Console.WriteLine("   " + w.Attribute("time_work").Value);
  //      }
  //    }
  //  }
  //}
}

