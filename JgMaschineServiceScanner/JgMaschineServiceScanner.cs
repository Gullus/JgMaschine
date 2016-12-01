using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Threading.Tasks;
using JgMaschineLib;
using JgMaschineLib.Scanner;

namespace JgMaschineServiceScanner
{
  class Programm
  {
    static void Main()
    {
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

        Protokoll = new Proto(Proto.KategorieArt.ServiceScanner, new JgMaschineLib.Email.SendEmailOptionen()
        {
          AdresseAbsender = pr.EmailAbsender,
          AdressenEmpfaenger = pr.EmailListeEmpfaenger,
          Betreff = pr.EmailBetreff,

          ServerAdresse = pr.EmailServerAdresse,
          ServerPort = pr.EmailServerPortNummer,
          ServerBenutzername = pr.EmailServerBenutzerName,
          ServerPasswort = pr.EmailServerBenutzerKennwort
        })
      };

#if DEBUG

      scOptionen.Protokoll.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.Console);
      scOptionen.Protokoll.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.Console);
      scOptionen.Protokoll.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.Console);
      scOptionen.Protokoll.AddAuswahl(Proto.ProtoArt.Kommentar, Proto.AnzeigeArt.Console);

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

      ScannOptionen.Protokoll.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.WinProtokoll, Proto.AnzeigeArt.Email);
      ScannOptionen.Protokoll.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.WinProtokoll);
      ScannOptionen.Protokoll.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.WinProtokoll);
      //ScannOptionen.Protokoll.AddAuswahl(Proto.ProtoArt.Kommentar, Proto.AnzeigeArt.WinProtokoll);
    }

    protected override void OnStart(string[] args)
    {
      base.OnStart(args);

      _ScannProgramm.Optionen.Protokoll.Set("Scannerservice starten!", Proto.ProtoArt.Info);

      var task = new Task(() =>
      {
        _ScannProgramm.Start(); ;
      });
      task.Start();
    }

    protected override void OnShutdown()
    {
      base.OnShutdown();
      _ScannProgramm.Optionen.Protokoll.Set("Scannerservice heruntergefahren!", Proto.ProtoArt.Info);
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
