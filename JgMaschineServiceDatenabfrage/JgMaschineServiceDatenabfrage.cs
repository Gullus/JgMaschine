using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineServiceDatenabfrage.Maschinen;

namespace JgMaschineServiceDatenabfrage
{
  class Programm
  {
    static void Main(string[] args)
    {
      var prop = Properties.Settings.Default;
      var opt = new OptioneAbfrage()
      {
        PfadEvg = prop.PfadDatenEvG,
        PfadProgress = prop.PfadDatenProgress,
        PfadSchnell = prop.PfadDatenSchnell,
        Protokoll = new Proto(Proto.KategorieArt.ServiceDatenabfrage, new JgMaschineLib.Email.SendEmailOptionen()
        {
          AdresseAbsender = prop.EmailAbsender,
          AdressenEmpfaenger = prop.EmailListeEmpfaenger,
          Betreff = prop.EmailBetreff,
          ServerAdresse = prop.EmailServerAdresse,
          ServerPort = prop.EmailServerPortNummer,
          ServerBenutzername = prop.EmailServerBenutzerName,
          ServerPasswort = prop.EmailServerBenutzerKennwort
        }),
      };

#if DEBUG

      opt.Protokoll.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.Console);
      opt.Protokoll.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.Console);
      opt.Protokoll.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.Console);
      opt.Protokoll.AddAuswahl(Proto.ProtoArt.Kommentar, Proto.AnzeigeArt.Console);

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
      Optionen.Protokoll.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.WinProtokoll, Proto.AnzeigeArt.Email);
      Optionen.Protokoll.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.WinProtokoll);
      Optionen.Protokoll.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.WinProtokoll);
      Optionen.Protokoll.AddAuswahl(Proto.ProtoArt.Kommentar, Proto.AnzeigeArt.WinProtokoll);

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