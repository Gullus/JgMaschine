using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace JgMaschineDienst
{
  public class JgMaschieService : ServiceBase
  {
    static void Main()
    {
      EintragLog("Service gestartet", EventLogEntryType.Information);
      JgMaschieService.Run(new JgMaschieService());
      EintragLog("Sevice beendet", EventLogEntryType.Information);
    }

    private static void EintragLog(string Ereigniss, EventLogEntryType EreignissTyp)
    {
      string source = "JgMaschine Service";
      string log = "Application";

      if (!EventLog.SourceExists(source))
        EventLog.CreateEventSource(source, log);

      EventLog.WriteEntry(source, Ereigniss, EreignissTyp, 1);
    }

    protected override void OnStart(string[] args)
    {
      EintragLog("Start Dienst", EventLogEntryType.Warning);
      base.OnStart(args);

      var t = new Task(() =>
      {
        while (true)
        {
          EintragLog("Durchlauf " + JgMaschineService.Properties.Settings.Default.ScannerAdresse, EventLogEntryType.SuccessAudit);
          System.Media.SystemSounds.Beep.Play();
          Thread.Sleep(5000);
        }
      });
      t.Start();
    }

    protected override void OnStop()
    {
      EintragLog("Stop Dienst", EventLogEntryType.Warning);
      base.OnStop();
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
        ServiceName = "JgMaschine - Service",
        DisplayName = "JgMaschine - Service",
        Description = "Dienst zum erfassen von Daten und dem Steuern von Baustahl-Maschinen.",
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
