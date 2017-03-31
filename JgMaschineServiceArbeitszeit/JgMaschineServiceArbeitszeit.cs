using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceArbeitszeit
{
  class Programm
  {
    static void Main(string[] args)
    {
      Logger.SetLogWriter(new LogWriterFactory().Create());
      ExceptionPolicy.SetExceptionManager(new ExceptionPolicyFactory().CreateManager(), false);

      var prop = JgMaschineServiceAbreitszeit.Properties.Settings.Default;

      var msg = "Programm startet. Initialisierung Datafox Optionen.";
      Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Start);

      var optArbeitszeit = new OptionenArbeitszeit()
      {
        PfadUpdateBediener = prop.PfadUpdateBediener,
        AuslesIntervallTerminal = prop.AusleseIntervallInSekunden,
        VerbindungsString = prop.DatenbankVerbindungsString,
        AnzahlBisFehlerAusloesen = prop.AnzahlBisFehlerAusloesen,
        Terminal_TimeOut = prop.Terminal_TimeOut
      };

#if DEBUG

      var _ArbeitszeitErfassung = new ArbeitszeitErfassen(optArbeitszeit);

      msg = $"Arbeitszeit startet!";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);

      var task = new Task((azerf) =>
      {
        var az = (ArbeitszeitErfassen)azerf;
        az.Start();
      }, _ArbeitszeitErfassung);
      task.Start();

      Console.WriteLine("Service läuft. Beendigung mit einer Taste...");
      Console.ReadKey();

#else

      var ServiceToRun = new ServiceBase[] { new JgMaschineServiceArbeitszeit(optArbeitszeit) };
      ServiceBase.Run(ServiceToRun);

#endif

    }
  }

  public class JgMaschineServiceArbeitszeit : ServiceBase
  {
    private ArbeitszeitErfassen _ArbErfassung;

    public JgMaschineServiceArbeitszeit(OptionenArbeitszeit OptDatafox)
    {
      _ArbErfassung = new ArbeitszeitErfassen(OptDatafox);
    }

    protected override void OnStart(string[] args)
    {
      base.OnStart(args);

      var msg = "ServiceTask startet!";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);

      var task = new Task(() =>
      {
        _ArbErfassung.Start();
      });
      task.Start();
    }

    protected override void OnShutdown()
    {
      base.OnShutdown();
      var msg = "Service wurde heruntergefahren!";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
    }

    protected override void OnStop()
    {
      base.OnStop();
      _ArbErfassung.TimerStop();
      var msg = "Service wurde gestoppt!";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
    }

    protected override void OnContinue()
    {
      base.OnContinue();
      _ArbErfassung.TimerContinue();
      var msg = "Service wurde wieder gestartet!";
      Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
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
        ServiceName = "JgMaschine - Arbeitszeit",
        DisplayName = "JgMaschine - Arbeitszeit",
        Description = "Dienst zum erfassen von Arbeiteitszeit Daten.",
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
