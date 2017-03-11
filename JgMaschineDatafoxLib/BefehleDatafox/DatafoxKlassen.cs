using System;
using System.Collections.Generic;
using JgMaschineData;
using System.Linq;

namespace JgMaschineDatafoxLib
{

  public class OptionenDatafox
  {
    public class ClassTerminal
    {
      public string IpAdresse = "192.168.1.57";
      public int  Portnummer = 8000;
      public int  TimeOut = 3000;
      public int  ChannelId = 1;       // Nur interessant bei mehreren Terminals
      public int  DeviceId = 254;      // Bei Ip Verbindung immer 254
      public byte IdVerbindung = 3;    // <= Verbindung über TcpIp
    }

    internal JgModelContainer Db = null;
    internal List<tabBediener> ListeBediener;
    internal List<tabStandort> ListeStandorte;
    internal List<tabArbeitszeitTerminal> ListeTerminals;
    internal List<DatafoxDsImport> ListeAnmeldungen = new List<DatafoxDsImport>(); 

    public string VerbindungsString = "";
    public string PfadUpdateBediener = "";
    public int TimerIntervall = 10000;

    internal bool UpdateBenutzerOk = false;
    internal bool DatumAktualisieren = false;
    internal int ZaehlerDatumAktualisieren = 100; // Damit wird zeit beim Start aktualisiert

    public ClassTerminal Terminal = new ClassTerminal();

    public OptionenDatafox()
    { }

    public string GetStandort(Guid IdStandort)
    {
      return ListeStandorte.First(f => f.Id == IdStandort).Bezeichnung;
    }
  }

  public class DatafoxDsImport
  {
    public enum EnumVorgang
    {
      Fehler,
      Komme,
      Gehen,
      Pause
    }

    public Guid IdStandort;
    public string Version;
    public string MatchCode;
    public DateTime Datum;
    public EnumVorgang Vorgang;
    public int GehGrund;
  }
}
