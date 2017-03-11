namespace JgMaschineDatafoxLib
{
  public static partial class ProgDatafox
  {
    /// <summary>
    /// Oeffnen der Datafox Verbindung
    /// </summary>
    /// <param name="Optionen">Übertragungsoptionen</param>
    public static bool DatafoxOeffnen(OptionenDatafox Optionen)
    {
      var offen = DFComDLL.DFCComOpenIV(Optionen.Terminal.ChannelId, 0, Optionen.Terminal.IdVerbindung, Optionen.Terminal.IpAdresse, Optionen.Terminal.Portnummer, Optionen.Terminal.TimeOut);
      return offen != 0; // offen == 0 ist Fehler;
    }
  }
}
