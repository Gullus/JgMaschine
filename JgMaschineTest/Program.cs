using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using System.Configuration;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main(string[] args)
    {
      //var property = new System.Configuration.SettingsProperty("TestSetting");

      //Properties.Settings.Default["TestSetting"] = "TestFeld";
      //Properties.Settings.Default.Save();



      //System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

      //config.AppSettings.Settings.Add("Test", "MeinTest");
      //config.Save();

      //Console.WriteLine(Properties.Settings.Default.CurrentConfiguration .FilePath);

      //Console.WriteLine(Properties.Settings.Default["Test"].ToString());


      //Logger.SetLogWriter(new LogWriterFactory().Create());

      //var log = new LogEntry()
      //{
      //  Categories = new string[] { "Cat1", "Car2" },
      //  Message = "My message body",
      //  Severity = System.Diagnostics.TraceEventType.Critical,
      //  Priority = 100,
      //  Title = "Der Titel"
      //};

      ////Logger.Write("HalloBallo");

      //ExceptionPolicy.SetExceptionManager(new ExceptionPolicyFactory().CreateManager(), false);

      //try
      //{
      //  var i = 0;
      //  var z = 50 / i;
      //}
      //catch (Exception f)
      //{
      //  bool th = ExceptionPolicy.HandleException(f, "PolicyName");

      //}

      Console.ReadKey();
    }
  }



}

