using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using JgMaschineDatafoxLib;
using JgMaschineLib;

namespace JgMaschineServiceAbreitszeit
{
  class Programm
  {
    static void Main(string[] args)
    {
      var prop = Properties.Settings.Default;
      var optDatafox = new OptionenDatafox()
      {
        Standort = prop.Standort,
        PfadUpdateBediener = prop.PfadUpdateBediener,
        TimerIntervall = prop.AusleseIntervallInSekunden,
        VerbindungsString = prop.DatenbankVerbindungsString,

        Datafox = new DatafoxOptionen()
        {
          IpNummer = prop.Terminal_IpNummer,
          Portnummer = prop.Terminal_PortNummer,
          TimeOut = prop.Terminal_TimeOut
        },

        Protokoll = new Proto(Proto.KategorieArt.ServiceArbeitszeit, new JgMaschineLib.Email.SendEmailOptionen()
        {
          AdresseAbsender = prop.EmailAbsender,
          AdressenEmpfaenger = prop.EmailListeEmpfaenger,
          Betreff = prop.EmailBetreff,
          ServerAdresse = prop.EmailServerAdresse,
          ServerPort = prop.EmailServerPortNummer,
          ServerBenutzername = prop.EmailServerBenutzerName,
          ServerPasswort = prop.EmailServerBenutzerKennwort
        })
      };

#if DEBUG

      optDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.WinProtokoll);
      optDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.WinProtokoll);
      optDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.WinProtokoll);
      optDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Kommentar, Proto.AnzeigeArt.WinProtokoll);

      //optDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.Console, Proto.AnzeigeArt.Email);
      //optDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.Console);
      //optDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.Console);
      //optDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Kommentar, Proto.AnzeigeArt.Console);

      var msg = $"Arbeitszeit startet!\nAdresse: {optDatafox.Datafox.IpNummer}  Port: {optDatafox.Datafox.Portnummer}.";
      optDatafox.Protokoll.Set(msg, Proto.ProtoArt.Info);

      var _ArbeitszeitErfassung = new ArbeitszeitErfassen(optDatafox);
      // _ArbeitszeitErfassung.Start();



      using (var db = new JgMaschineData.JgModelContainer())
      {
        var standort = db.tabStandortSet.FirstOrDefault();


        ArbeitszeitErfassen.ArbeitszeitInDatenbank(db, null, standort.Id, new Proto(Proto.KategorieArt.Arbeitszeit));
      }

      Console.ReadKey();
#else

      var ServiceToRun = new ServiceBase[] { new JgMaschineServiceArbeitszeit(optDatafox) };
      ServiceBase.Run(ServiceToRun);

#endif

    }
  }

  public class JgMaschineServiceArbeitszeit : ServiceBase
  {
    private ArbeitszeitErfassen _ArbErfassung;

    public JgMaschineServiceArbeitszeit(OptionenDatafox OptDatafox)
    {
      OptDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.WinProtokoll, Proto.AnzeigeArt.Email);
      OptDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.WinProtokoll);
      OptDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.WinProtokoll);
      OptDatafox.Protokoll.AddAuswahl(Proto.ProtoArt.Kommentar, Proto.AnzeigeArt.WinProtokoll);

      _ArbErfassung = new ArbeitszeitErfassen(OptDatafox);
    }

    protected override void OnStart(string[] args)
    {
      base.OnStart(args);
      _ArbErfassung.OptDatafox.Protokoll.Set("Arbeitszeitservice startet!", Proto.ProtoArt.Info);

      var task = new Task(() =>
      {
        _ArbErfassung.Start();
      });
      task.Start();
    }

    protected override void OnShutdown()
    {
      base.OnShutdown();
      _ArbErfassung.OptDatafox.Protokoll.AnzeigeWinProtokoll("Arbeitszeitservice heruntergefahren!", Proto.ProtoArt.Info);
    }

    protected override void OnStop()
    {
      base.OnStop();
      _ArbErfassung.TimerStop();
      _ArbErfassung.OptDatafox.Protokoll.AnzeigeWinProtokoll("Arbeitszeit angehalten!", Proto.ProtoArt.Info);
    }

    protected override void OnContinue()
    {
      base.OnContinue();
      _ArbErfassung.TimerContinue();
      _ArbErfassung.OptDatafox.Protokoll.AnzeigeWinProtokoll("Arbeitszeit wieder gestartet!", Proto.ProtoArt.Info);
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
