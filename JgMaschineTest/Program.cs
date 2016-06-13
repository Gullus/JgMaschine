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
      double d = 763552.965;
      Console.WriteLine(d.ToString(System.Globalization.CultureInfo.InvariantCulture));

      // Culture.InvariantCulture

      Console.ReadKey();
      return;

      var pr = Properties.Settings.Default;
      var st = new ScannerProgramm(pr.ScannerAdresse, pr.ScannerPort, pr.Verbindungszeichen, true);
      Console.ReadKey();
      st.Close();
    }
  }
}

