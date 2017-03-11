using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using JgMaschineServiceDatenabfrage.Maschinen;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceDatenabfrage
{
  class Programm
  {

    static void Main(string[] args)
    {
      Logger.SetLogWriter(new LogWriterFactory().Create());
      ExceptionPolicy.SetExceptionManager(new ExceptionPolicyFactory().CreateManager(), false);

      var prop = Properties.Settings.Default;
      var opt = new OptioneAbfrage()
      {
        PfadEvg = prop.PfadDatenEvG,
        PfadProgress = prop.PfadDatenProgress,
        PfadSchnell = prop.PfadDatenSchnell,
      };

#if DEBUG

      var abfrage = new MaschineDatenabfrage(opt);
      abfrage.Start();
      Console.ReadKey();

#else

      var ServiceToRun = new ServiceBase[] { new JgMaschineServiceDatenabfrage(opt) };
      ServiceBase.Run(ServiceToRun);

#endif

    }
  }

  public class JgMaschineServiceDatenabfrage : ServiceBase
  {
    private MaschineDatenabfrage _DatenAbfrage;

    public JgMaschineServiceDatenabfrage(OptioneAbfrage Optionen)
    {
      _DatenAbfrage = new MaschineDatenabfrage(Optionen);
    }

    protected override void OnStart(string[] args)
    {
      base.OnStart(args);
      _DatenAbfrage.Start();
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
        ServiceName = "JgMaschine - Datenabfrage",
        DisplayName = "JgMaschine - Datenabfrage",
        Description = "Abfrage der Produktionsdaten aus Baustahl-Maschinen.",
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