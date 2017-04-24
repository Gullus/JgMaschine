using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;


namespace JgMaschineServiceHandyArbeitszeit
{
  class Programm
  {
    static void Main(string[] args)
    {
      Logger.SetLogWriter(new LogWriterFactory().Create());
      ExceptionPolicy.SetExceptionManager(new ExceptionPolicyFactory().CreateManager(), false);

      var msg = "Service Arbeitszeit von Handy startet.";
      Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Start);

#if DEBUG

      var prop = Properties.Settings.Default;
      var arbeitszeitHandy = new ArbeitszeitVonHandy(prop.ConnectionString, prop.PortNummerServer);

      var t = new Task((azHandy) =>
      {
        (azHandy as ArbeitszeitVonHandy).Start();
      }, arbeitszeitHandy, TaskCreationOptions.LongRunning);
      t.Start();

      Console.WriteLine("Dienst Arbeitszeit Handy läuft....");
      Console.ReadKey();

#else

      var ServiceToRun = new ServiceBase[] { new JgMaschineServiceHandyArbeitszeit() };
      ServiceBase.Run(ServiceToRun);

#endif

    }
  }

  public class JgMaschineServiceHandyArbeitszeit : ServiceBase
  {
    private ArbeitszeitVonHandy _ArbeitszeitHandy;

    public JgMaschineServiceHandyArbeitszeit()
    {
      var prop = Properties.Settings.Default;
      _ArbeitszeitHandy = new ArbeitszeitVonHandy(prop.ConnectionString, prop.PortNummerServer);
    }

    protected override void OnStart(string[] args)
    {
      base.OnStart(args);

      var msg = "ServiceTask startet!";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Start);

      var t = new Task((azHandy) =>
      {
        (azHandy as ArbeitszeitVonHandy).Start();
      }, _ArbeitszeitHandy, TaskCreationOptions.LongRunning);
      t.Start();
    }

    protected override void OnShutdown()
    {
      _ArbeitszeitHandy.Listener.Stop();
      base.OnShutdown();

      var msg = "Service wurde heruntergefahren!";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Stop);
    }
  }

  [RunInstaller(true)]
  public class Installation : Installer
  {
    private ServiceInstaller _serviceInstall;
    private ServiceProcessInstaller _prozessInstaller;

    public Installation()
    {
      _serviceInstall = new ServiceInstaller()
      {
        ServiceName = "JgMaschine - ArbeitszeitVonHandy",
        DisplayName = "JgMaschine - ArbeitszeitVonHandy",
        Description = "Dienst zum erfassen von Arbeiteitszeit aus einem Handy.",
        StartType = ServiceStartMode.Automatic,
        DelayedAutoStart = true,
      };

      _prozessInstaller = new ServiceProcessInstaller()
      {
        Account = ServiceAccount.LocalSystem,
      };

      Installers.Add(_serviceInstall);
      Installers.Add(_prozessInstaller);
    }
  }
}

