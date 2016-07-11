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
using System.Management;
using System.Net;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main(string[] args)
    {
      var networkPath = @"\\gullus-server\Sicherungen";

      var fileList = Directory.GetFiles(networkPath);

      foreach (var file in fileList)
      {
        Console.WriteLine("{0}", Path.GetFileName(file));
      }

      var credentials = new NetworkCredential("jg", "pezdx65");
      try
      {
        using (new JgMaschineLib.NetzwerkVerbindung(networkPath, credentials))
        {
          fileList = Directory.GetFiles(networkPath);

          foreach (var file in fileList)
          {
            Console.WriteLine("{0}", Path.GetFileName(file));
          }
        }
      }
      catch (Exception f)
      {
        Console.WriteLine(f.Message);
      }

      string s = "Hallo\nBallo";
      Console.WriteLine(s);

      //Console.ReadKey();


      //var p = Properties.Settings.Default;
      //string bvbs = "BF2D@HjTest14@r417@ia@p1@l1000@n10@e0.888@d12@g500S@s48@v@Gl400@w90@l600@w0@C88@";

      //try
      //{
      //  BenutzerAnmeldung.Anmeldung(p.BenutzerName, p.BenutzerKennwort, p.MaschinenAdresse, p.MaschinenPfad, bvbs);
      //}
      //catch (Exception f)
      //{
      //  Console.WriteLine(f.Message);
      //}

      //var dat = @"c\Progress\Pro2\impdata\Auftrag.txt";      //impdata
      //var domaene = p.MaschinenAdresse;

      //Console.WriteLine($"Maschinenadesse: {domaene}");

      //dat = string.Format(@"\\{0}\{1}", domaene, dat);

      //Console.WriteLine($"Programmpfad: {dat}");

      //try
      //{
      //  File.WriteAllText(dat, bvbs, Encoding.UTF8);
      //}
      //catch (Exception f)
      //{
      //  Console.WriteLine(f.Message);
      //}
      //Console.ReadKey();
    }
  }
}

