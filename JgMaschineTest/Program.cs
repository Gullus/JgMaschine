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
      Logger.SetLogWriter(new LogWriterFactory().Create());

      Logger.Write("HalloBallo Verbose", "Kategorie", 1, 0, TraceEventType.Verbose);
      Logger.Write("HalloBallo Information", "Kategorie", 1, 0, TraceEventType.Information);


      //foreach(var t in Enum.GetValues(typeof(TraceEventType)))
      //{
      //  Logger.Write(t.ToString(), "Kategorie", 100, 10, (TraceEventType)t);
      //}

      ////foreach (var t in Enum.GetValues(typeof(TraceEventType)))
      ////{
      ////  Logger.Write(t.ToString(), "", 100, 10, (TraceEventType)t);
      ////}


      ExceptionPolicy.SetExceptionManager(new ExceptionPolicyFactory().CreateManager(), false);

      try
      {
        try
        {
          var i = 0;
          var z = 50 / i;
        }
        catch (Exception f)
        {
          bool th = ExceptionPolicy.HandleException(f, "Policy");
          throw new Exception("Meine neuer Fehler", f);
        }
      }
      catch (Exception h)
      {
        bool th = ExceptionPolicy.HandleException(h, "Policy");
      }

      //AddOrUpdateAppSettings("T1", "T2");

      //var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      //var asr = new AppSettingsReader();
      //var s = asr.GetValue("Juhu", typeof(string));

      //Console.WriteLine(s);
      //ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);

      //ConfigurationManager.AppSettings.Add("Mist", "Arsch");

      //Console.WriteLine(ConfigurationManager.AppSettings["T1"]);

      //Console.ReadKey();

    }

    public static void AddOrUpdateAppSettings(string key, string value)
    {
      try
      {
        var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        var settings = configFile.AppSettings.Settings;
        if (settings[key] == null)
        {
          settings.Add(key, value);
        }
        else
        {
          settings[key].Value = value;
        }
        configFile.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
      }
      catch (ConfigurationErrorsException)
      {
        Console.WriteLine("Error writing app settings");
      }
    }
  }



}

