using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JgMaschineDatafoxLib
{
  public class DatafoxOptionen
  {
    public string IpNummer { get; set; } = "192.168.1.57";
    public int Portnummer { get; set; } = 8000;
    public int TimeOut { get; set; } = 3000;
    public int ChannelId { get; set; } = 1;
    public int DeviceId { get; set; } = 254;
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
}
