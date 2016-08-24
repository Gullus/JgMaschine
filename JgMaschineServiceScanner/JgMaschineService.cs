using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Threading.Tasks;
using JgMaschineLib;
using JgMaschineLib.Scanner;

namespace JgMaschineServiceScanner
{
  //class Programm
  //{
  //  static void Main()
  //  {
  //    var pr = Properties.Settings.Default;
  //    var st = new ScannerProgramm(new ScannerOptionen()
  //    {
  //      DbVerbindungsString = pr.DatenbankVerbindungsString,

  //      CradleIpAdresse = pr.CradleIpAdresse,
  //      CradlePortNummer = pr.CradlePortNummer,
  //      CradleTextAnmeldung = pr.CradleTextAnmeldung,

  //      EvgPfadProduktionsListe = pr.EvgPfadProduktionsListe,
  //      EvgDateiProduktionsAuftrag = pr.EvgDateiProduktionsAuftrag,
  //      ProgressPfadProduktionsListe = pr.ProgressPfadProduktionsListe,

  //      Protokoll = new JgMaschineLib.Proto(JgMaschineLib.Proto.KategorieArt.ServiceScanner, new JgMaschineLib.Email.SendEmailOptionen()
  //      {
  //        AdresseAbsender = pr.EmailAbsender,
  //        AdressenEmpfaenger = pr.EmailListeEmpfaenger,
  //        Betreff = pr.EmailBetreff,

  //        ServerAdresse = pr.EmailServerAdresse,
  //        ServerPort = pr.EmailServerPortNummer,
  //        ServerBenutzername = pr.EmailServerBenutzerName,
  //        ServerPasswort = pr.EmailServerBenutzerKennwort
  //      })
  //    });

  //    var prot = st.Optionen.Protokoll;
  //    prot.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.Console);
  //    prot.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.Console);
  //    prot.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.Console);
  //    prot.AddAuswahl(Proto.ProtoArt.Kommentar, Proto.AnzeigeArt.Console);

  //    st.Start();

  //    Console.WriteLine("Gestartet");

  //    Console.ReadKey();
  //    st.Close();
  //  }
  //}

  public class JgMaschineServiceScanner : ServiceBase
  {
    private ScannerProgramm _ScannProgramm;

    private static void Main()
    {
      JgMaschineServiceScanner.Run(new JgMaschineServiceScanner());
    }

    protected override void OnStart(string[] args)
    {
      base.OnStart(args);

      var pr = Properties.Settings.Default;
      _ScannProgramm = new ScannerProgramm(new ScannerOptionen()
      {
        DbVerbindungsString = pr.DatenbankVerbindungsString,

        CradleIpAdresse = pr.CradleIpAdresse,
        CradlePortNummer = pr.CradlePortNummer,
        CradleTextAnmeldung = pr.CradleTextAnmeldung,

        EvgPfadProduktionsListe = pr.EvgPfadProduktionsListe,
        EvgDateiProduktionsAuftrag = pr.EvgDateiProduktionsAuftrag,
        ProgressPfadProduktionsListe = pr.ProgressPfadProduktionsListe,

        Protokoll = new JgMaschineLib.Proto(JgMaschineLib.Proto.KategorieArt.ServiceScanner, new JgMaschineLib.Email.SendEmailOptionen()
        {
          AdresseAbsender = pr.EmailAbsender,
          AdressenEmpfaenger = pr.EmailListeEmpfaenger,
          Betreff = pr.EmailBetreff,

          ServerAdresse = pr.EmailServerAdresse,
          ServerPort = pr.EmailServerPortNummer,
          ServerBenutzername = pr.EmailServerBenutzerName,
          ServerPasswort = pr.EmailServerBenutzerKennwort
        })
      });

      var prot = _ScannProgramm.Optionen.Protokoll;
      prot.AddAuswahl(Proto.ProtoArt.Fehler, Proto.AnzeigeArt.WinProtokoll, Proto.AnzeigeArt.Email);
      prot.AddAuswahl(Proto.ProtoArt.Warnung, Proto.AnzeigeArt.WinProtokoll);
      prot.AddAuswahl(Proto.ProtoArt.Info, Proto.AnzeigeArt.WinProtokoll);
      prot.AddAuswahl(Proto.ProtoArt.Kommentar, Proto.AnzeigeArt.WinProtokoll);

      prot.Set("Programm Start !", Proto.ProtoArt.Info);

      var task = new Task(() =>
      {
        _ScannProgramm.Start(); ;
      });
      task.Start();
    }

    protected override void OnShutdown()
    {
      base.OnShutdown();
      _ScannProgramm.Optionen.Protokoll.Set("Serve Scanner heruntergefahren!", Proto.ProtoArt.Info);
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
