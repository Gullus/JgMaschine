using JgMaschineLib;

namespace JgMaschineDatafoxLib
{
  public static partial class ProgDatafox
  {
    /// <summary>
    /// Anzeige eines Textes auf dem Datafox Gerät
    /// </summary>
    /// <param name="Optionen">Übertragungsoptionen</param>
    /// <param name="MessageText">Text, welcher auf dem Display ausgeben werden soll</param>
    /// <param name="Tonfolge">Tonfolge: 0 - Kein Ton; 1-10 siehe Documentation</param>
    public static void Send(DatafoxOptionen Optionen, string MessageText, byte Tonfolge = 7)
    {
      byte idVerbindung = 3; // <= Verbindung über TcpIp

      if (DFComDLL.DFCComOpenIV(Optionen.ChannelId, 0, idVerbindung, Optionen.IpNummer, Optionen.Portnummer, Optionen.TimeOut) == 0)
      {
        var msg = string.Format("Schnittstelle oder Verbindung zum Gerät konnte nicht geöffnet werden.\n\nBitte überprüfen Sie die Einstellungen der Kommunikation und Erreichbarkeit des Terminals.");
        Helper.Protokoll(msg);
        return;
      }

      DFComDLL.DFCComSendMessage(Optionen.ChannelId, Optionen.DeviceId, idVerbindung, 0, Tonfolge, MessageText, MessageText.Length);

      DFComDLL.DFCComClose(Optionen.ChannelId);
    }
  }
}
