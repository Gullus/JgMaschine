using System;
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
    public static void ZeitEinstellen(DatafoxOptionen Optionen, DateTime DatumZeit)
    {
      byte idVerbindung = 3; // <= Verbindung über TcpIp

      if (DFComDLL.DFCComOpenIV(Optionen.ChannelId, 0, idVerbindung, Optionen.IpNummer, Optionen.Portnummer, Optionen.TimeOut) == 0)
      {
        var msg = string.Format("Schnittstelle oder Verbindung zum Gerät konnte nicht geöffnet werden.\n\nBitte überprüfen Sie die Einstellungen der Kommunikation und Erreichbarkeit des Terminals.");
        Helper.Protokoll(msg);
        return;
      }

      //Buffer[0] = 20;
      //Buffer[1] = 12;
      //Buffer[2] = 4;
      //Buffer[3] = 6; // 06.04.2012 Datum
      //Buffer[4] = 4;
      //Buffer[5] = 12;
      //Buffer[6] = 0; // 04:12:00 Uhrzeit

      var jahr = DatumZeit.Year.ToString();

      var buffer = new byte[7];
      buffer[0] = Convert.ToByte(jahr.Substring(0, 2));
      buffer[1] = Convert.ToByte(jahr.Substring(2, 2)); ;
      buffer[2] = Convert.ToByte(DatumZeit.Month);
      buffer[3] = Convert.ToByte(DatumZeit.Day); 
      buffer[4] = Convert.ToByte(DatumZeit.Hour);
      buffer[5] = Convert.ToByte(DatumZeit.Minute);
      buffer[6] = Convert.ToByte(DatumZeit.Second); // 04:12:00 Uhrzeit

      DFComDLL.DFCComSetTime(Optionen.ChannelId, Optionen.DeviceId, buffer);

      DFComDLL.DFCComClose(Optionen.ChannelId);
    }
  }
}
