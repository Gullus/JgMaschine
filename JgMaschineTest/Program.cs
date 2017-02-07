using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main(string[] args)
    {
      Logger.SetLogWriter(new LogWriterFactory().Create());

      var log = new LogEntry()
      {
        Categories = new string[] { "Cat1", "Car2" },
        Message = "My message body",
        Severity = System.Diagnostics.TraceEventType.Critical,
        Priority = 100,
        Title = "Der Titel",
      };
      Logger.Write(log);


      Console.ReadKey();
    }
  }

}

