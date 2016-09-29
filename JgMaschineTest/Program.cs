using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using JgMaschineData;
using JgMaschineDatafoxLib;
using JgMaschineLib;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main(string[] args)
    {
      var s = "12345";
      Console.WriteLine(s.Substring(0, s.Length - 1));

      Console.ReadKey();


      var t = JgMaschineLib.Stahl.StahlGewichte.GetGewichtKgAsString(12, 1224);
      Console.WriteLine(t);

      Console.ReadKey();

    }
  }
}


