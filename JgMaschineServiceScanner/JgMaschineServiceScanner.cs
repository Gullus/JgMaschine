using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Threading.Tasks;
using JgMaschineLib;
using JgMaschineLib.Scanner;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceScanner
{
  class Programm
  {
    static void Main()
    {
      Logger.SetLogWriter(new LogWriterFactory().Create());
      ExceptionPolicy.SetExceptionManager(new ExceptionPolicyFactory().CreateManager(), false);

      var pr = Properties.Settings.Default;
      var scOptionen = new ScannerOptionen()
      {
        DbVerbindungsString = pr.DatenbankVerbindungsString,

        CradleIpAdresse = pr.CradleIpAdresse,
        CradlePortNummer = pr.CradlePortNummer,
        CradleTextAnmeldung = pr.CradleTextAnmeldung,

        EvgPfadProduktionsListe = pr.EvgPfadProduktionsListe,
        EvgDateiProduktionsAuftrag = pr.EvgDateiProduktionsAuftrag,
        ProgressPfadProduktionsListe = pr.ProgressPfadProduktionsListe,
      };

#if DEBUG

      var scanner = new ScannerProgramm(scOptionen);
      var task = new Task(() =>
      {
        scanner.Start(); ;
      });
      task.Start();

      Console.WriteLine("Scanner Gestartet");
      Console.ReadKey();
      scanner.Close();

#else

      var ServiceToRun = new ServiceBase[] { new JgMaschineServiceScanner(scOptionen) };
      ServiceBase.Run(ServiceToRun);

#endif

    }
  }

  public class JgMaschineServiceScanner : ServiceBase
  {
    private ScannerProgramm _ScannProgramm;

    public JgMaschineServiceScanner(ScannerOptionen ScannOptionen)
    {
      _ScannProgramm = new ScannerProgramm(ScannOptionen);
    }

    protected override void OnStart(string[] args)
    {
      base.OnStart(args);

      var msg = "Scannerservice starten!";
      Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Start);

      var task = new Task(() =>
      {
        _ScannProgramm.Start(); ;
      });
      task.Start();
    }

    protected override void OnShutdown()
    {
      base.OnShutdown();
      var msg = "Scannerservice herunterfahren!";
      Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Information);
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
        ServiceName = "JgMaschine - Scanner",
        DisplayName = "JgMaschine - Scanner",
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
