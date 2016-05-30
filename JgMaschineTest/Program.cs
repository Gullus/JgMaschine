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

namespace JgMaschineTest
{
  class Programm
  {
    static void Main()
    {
      var merge = new DbMerge(Properties.Settings.Default.VerbindungClient, Properties.Settings.Default.VerbindungServer);
      merge.Start();

      Console.WriteLine("Fertsch ...");

      //var pr = Properties.Settings.Default;
      //var st = new ScannerProgramm(pr.ScannerAdresse, pr.ScannerPort, pr.Verbindungszeichen, true);
      //Console.ReadKey();
      //st.Close();

      //var adresse = Properties.Settings.Default.ScannerAdresse;
      //var port = Properties.Settings.Default.ScannerPort;

      //try
      //{
      //  Console.WriteLine("Client starten");

      //  var cl = new TcpClient(adresse, port);
      //  var ns = cl.GetStream();

      //  Console.WriteLine("Client gestartet");

      //  while (true)
      //  {
      //    var buffer = new byte[4096];
      //    var anz = ns.Read(buffer, 0, buffer.Length);

      //    var returndata = Encoding.UTF8.GetString(buffer, 0, anz);

      //    Console.WriteLine(returndata);

      //    var sendBytes = Encoding.UTF8.GetBytes(Convert.ToChar(13).ToString());
      //    ns.Write(sendBytes, 0, sendBytes.Length);
      //  }
      //}
      //catch (Exception f)
      //{
      //  Console.WriteLine(f.Message);
      //}

      Console.ReadKey();
    }
  }

  //static void Main()
  //{
  //  var adresse = Properties.Settings.Default.Adresse;
  //  var port = Properties.Settings.Default.PortNummer;

  //  var dl = new DataLogicScanner(adresse, port, null, true);
  //  dl.Start();

  //  Console.ReadKey();
  //}


  public class DbMerge
  {
    private JgModelContainer _DbClient;
    private JgModelContainer _DbServer;

    public DbMerge(string ConnectioStringClient, string ConectionStringServer)
    {
      _DbClient = new JgModelContainer();
      _DbClient.Database.Connection.ConnectionString = ConnectioStringClient;
      _DbServer = new JgModelContainer();
      _DbServer.Database.Connection.ConnectionString = ConectionStringServer;
    }

    public void DatenEintragen<T>(Dictionary<string, T> ListeClients, Dictionary<string, T> ListeServer, params string[] FelderAusschluss)
      where T : class
    {
      var t = typeof(T);

      var lClients = new SortedList<string, T>(ListeClients, StringComparer.CurrentCultureIgnoreCase);
      var lServer = new SortedList<string, T>(ListeServer, StringComparer.CurrentCultureIgnoreCase);

      DatenAbgleich datAbgleichClient = null;
      DatenAbgleich datAbgleichServer = null;
      DbEntityEntry<T> entClient = null;
      DbEntityEntry<T> entServer = null;

      var felder = typeof(T).GetProperties().Select(s => s.Name).Except(FelderAusschluss).Union(new string[] { "Id" }) ;

      foreach (var cl in lClients)
      {
        entClient = _DbClient.Entry<T>(cl.Value);
        datAbgleichClient = entClient.Property<DatenAbgleich>("DatenAbgleich").CurrentValue;

        if (lServer.ContainsKey(cl.Key))
        {
          entServer = _DbServer.Entry<T>(lServer[cl.Key]);
          datAbgleichServer = entServer.Property<DatenAbgleich>("DatenAbgleich").CurrentValue;

          if (datAbgleichClient.Datum > datAbgleichServer.Datum)
          {
            datAbgleichClient.Status = EnumStatusDatenabgleich.Ok;
            foreach (var feld in felder)
            {
              if (entServer.Property(feld).CurrentValue != entClient.Property(feld).CurrentValue)
                entServer.Property(feld).CurrentValue = entClient.Property(feld).CurrentValue;
            }
          }
          else
          {
            datAbgleichServer.Status = EnumStatusDatenabgleich.Ok;
            foreach (var feld in felder)
            {
              if (entClient.Property(feld).CurrentValue != entServer.Property(feld).CurrentValue)
                entClient.Property(feld).CurrentValue = entServer.Property(feld).CurrentValue;
            }
          }
        }
        else
        {
          datAbgleichClient.Status = EnumStatusDatenabgleich.Ok;
          var dsNeu =  _DbServer.Set<T>().Create<T>();
          var entNeu = _DbServer.Entry(dsNeu);
          entNeu.Property("Id").CurrentValue = entClient.Property("Id").CurrentValue;
          foreach (var feld in felder)
            entNeu.Property(feld).CurrentValue = entClient.Property(feld).CurrentValue;
          _DbServer.Set<T>().Add(entNeu.Entity);
        }
      }

      _DbClient.SaveChanges();
      _DbServer.SaveChanges();
    }

    public async void Start()
    {
      var dcStandorte = await _DbClient.tabStandortSet.ToDictionaryAsync(s => s.Id.ToString(), s => s);
      var dsStandorte = await _DbServer.tabStandortSet.ToDictionaryAsync(s => s.Id.ToString(), s => s);
      var felderAuschliessen = new string[] { "sMaschinen", "sArbeitszeiten", "sBediener" };

      DatenEintragen<tabStandort>(dcStandorte, dsStandorte, felderAuschliessen);
    }
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

