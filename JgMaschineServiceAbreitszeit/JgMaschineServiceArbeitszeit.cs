using System;
using System.ComponentModel;
using System.ServiceProcess;
using JgMaschineDatafoxLib;
using JgMaschineLib;

namespace JgMaschineServiceAbreitszeit
{
  class Programm
  {
    static void Main(string[] args)
    {
      var prop = Properties.Settings.Default;
      var _ArbeitszeitErfassung = new ArbeitszeitErfassen(new ZeitsteuerungDatafox()
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
      });

      var prot = _ArbeitszeitErfassung.ZeitsteuerungOptionen.Protokoll;
      prot.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.WinProtokoll);
      prot.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.WinProtokoll);
      prot.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.WinProtokoll, Proto.AnzeigeArt.Email);

      var msg = $"Arbeitszeit startet!\nAdresse: {_ArbeitszeitErfassung.ZeitsteuerungOptionen.Datafox.IpNummer}  Port: {_ArbeitszeitErfassung.ZeitsteuerungOptionen.Datafox.Portnummer}.";
      prot.Set(msg, Proto.ProtoArt.Info);

      Console.ReadKey();
    }
  }

  //public class JgMaschineServiceArbeitszeit : ServiceBase
  //{
  //  private ArbeitszeitErfassen _ArbeitszeitErfassung;

  //  private static void Main()
  //  {
  //    JgMaschineServiceArbeitszeit.Run(new JgMaschineServiceArbeitszeit());
  //  }

  //  protected override void OnStart(string[] args)
  //  {
  //    base.OnStart(args);

  //    var prop = Properties.Settings.Default;
  //    _ArbeitszeitErfassung = new ArbeitszeitErfassen(new ZeitsteuerungDatafox()
  //    {
  //      Standort = prop.Standort,
  //      PfadUpdateBediener = prop.PfadUpdateBediener,
  //      TimerIntervall = prop.AusleseIntervallInSekunden,
  //      VerbindungsString = prop.DatenbankVerbindungsString,

  //      Datafox = new DatafoxOptionen()
  //      {
  //        IpNummer = prop.Terminal_IpNummer,
  //        Portnummer = prop.Terminal_PortNummer,
  //        TimeOut = prop.Terminal_TimeOut
  //      },

  //      Protokoll = new Proto(Proto.KategorieArt.ServiceArbeitszeit)
  //      {
  //        EmailOptionen = new JgMaschineLib.Email.SendEmailOptionen()
  //        {
  //          AdresseAbsender = prop.EmailAbsender,
  //          AdressenEmpfaenger = prop.EmailListeEmpfaenger,
  //          Betreff = prop.EmailBetreff,
  //          ServerAdresse = prop.EmailServerAdresse,
  //          ServerPort = prop.EmailServerPortNummer,
  //          ServerBenutzername = prop.EmailServerBenutzerName,
  //          ServerPasswort = prop.EmailServerBenutzerKennwort
  //        }
  //      }
  //    });

  //    var prot = _ArbeitszeitErfassung.ZeitsteuerungOptionen.Protokoll;
  //    prot.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.WinProtokoll);
  //    prot.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.WinProtokoll);
  //    prot.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.WinProtokoll);
  //    prot.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.Email);
  //    prot.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.Email);
  //    prot.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.Email);

  //    prot.AnzeigeWinProtokoll("Arbeitszeit startet!", Proto.ProtoArt.Info);
  //  }

  //  protected override void OnShutdown()
  //  {
  //    base.OnShutdown();
  //    _ArbeitszeitErfassung.ZeitsteuerungOptionen.Protokoll.AnzeigeWinProtokoll("Arbeitszeit herunter gefahren!", Proto.ProtoArt.Info);
  //  }

  //  protected override void OnStop()
  //  {
  //    base.OnStop();
  //    _ArbeitszeitErfassung.TimerStop();
  //    _ArbeitszeitErfassung.ZeitsteuerungOptionen.Protokoll.AnzeigeWinProtokoll("Arbeitszeit angehalten!", Proto.ProtoArt.Info);
  //  }

  //  protected override void OnContinue()
  //  {
  //    base.OnContinue();
  //    _ArbeitszeitErfassung.TimerContinue();
  //    _ArbeitszeitErfassung.ZeitsteuerungOptionen.Protokoll.AnzeigeWinProtokoll("Arbeitszeit wieder gestartet!", Proto.ProtoArt.Info);
  //  }
  //}

  //[RunInstaller(true)]
  //public class Installation : Installer
  //{
  //  private ServiceInstaller _serviceInstall;
  //  private ServiceProcessInstaller _prozessInstaller;

  //  public Installation()
  //  {
  //    _serviceInstall = new ServiceInstaller()
  //    {
  //      ServiceName = "JgMaschineArbeitszeit - Service",
  //      DisplayName = "JgMaschineArbeitszeit - Service",
  //      Description = "Dienst zum erfassen von Arbeiteitszeit Daten.",
  //      StartType = ServiceStartMode.Automatic,
  //      DelayedAutoStart = true,
  //    };

  //    _prozessInstaller = new ServiceProcessInstaller()
  //    {
  //      Account = ServiceAccount.LocalSystem,
  //    };

  //    Installers.Add(_serviceInstall);
  //    Installers.Add(_prozessInstaller);
  //  }
  //}
}
