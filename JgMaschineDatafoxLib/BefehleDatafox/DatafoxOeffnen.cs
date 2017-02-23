using System;
using JgMaschineLib;

namespace JgMaschineDatafoxLib
{
  public static partial class ProgDatafox
  {
    /// <summary>
    /// Oeffnen der Datafoc Verbindung
    /// </summary>
    /// <param name="Optionen">Übertragungsoptionen</param>
    public static void DatafoxOeffnen(OptionenDatafox Optionen)
    {
      byte idVerbindung = 3; // <= Verbindung über TcpIp

      if (DFComDLL.DFCComOpenIV(Optionen.Datafox.ChannelId, 0, idVerbindung, Optionen.Datafox.IpNummer, Optionen.Datafox.Portnummer, Optionen.Datafox.TimeOut) == 0)
      {
        var msg = string.Format("Schnittstelle oder Verbindung zum Gerät konnte nicht geöffnet werden. Überprüfen Sie die Einstellungen der Kommunikation und Erreichbarkeit des Terminals.");
        throw new MyException(msg);
      }
    }
  }
}
