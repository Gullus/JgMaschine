using System;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineDatafoxLib
{
  public class DatafoxOptionen
  {
    public string IpNummer { get; set; } = "192.168.1.57";
    public int Portnummer { get; set; } = 8000;
    public int TimeOut { get; set; } = 3000;
    public int ChannelId { get; set; } = 1;   // Nur interessant bei mehreren Terminals
    public int DeviceId { get; set; } = 254;  // Bei Ip Verbindung immer 254
    public byte IdVerbindung { get; } = 3;    // <= Verbindung über TcpIp
  }

  public class DatafoxDsExport
  {
    public enum EnumVorgang
    {
      Fehler,
      Komme,
      Gehen,
      Pause
    }

    public string Version { get; set; }
    public string MatchCode { get; set; }
    public DateTime Datum { get; set; }
    public EnumVorgang Vorgang { get; set; }
    public int GehGrund { get; set; }
  }

  public class ZeitsteuerungDatafox
  {
    public string Standort = "";
    public JgModelContainer Db = null;
    public int TimerIntervall = 10000;
    public string VerbindungsString = "";
    public string PfadUpdateBediener = "";
    public int ZaehlerDatumAktualisieren = 50;

    public Proto Protokoll = null;
    public DatafoxOptionen Datafox = null;

    public ZeitsteuerungDatafox()
    { }
  }
}
