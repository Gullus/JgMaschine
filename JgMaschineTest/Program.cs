using System;
using System.Diagnostics;
using System.Linq;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main(string[] args)
    {
      var s = Enum.GetNames(typeof(JgMaschineData.ZeitHelper.Monate));

      foreach (var ff in s)
        Console.WriteLine(ff);


      string gg = Enum.GetName(typeof(JgMaschineData.ZeitHelper.Monate), 0);

      Console.WriteLine(gg);

      Console.ReadKey();
    }
  }
  
}


