namespace JgMaschineServiceArbeitszeit
{
  public static partial class ProgDatafox
  {
    /// <summary>
    /// Oeffnen der Datafox Verbindung
    /// </summary>
    /// <param name="Optionen">Übertragungsoptionen</param>
    public static bool DatafoxOeffnen(OptionenTerminal Optionen)
    {
      var offen = DFComDLL.DFCComOpenIV(Optionen.ChannelId, 0, Optionen.IdVerbindung, Optionen.IpAdresse, Optionen.Portnummer, Optionen.TimeOut);
      return offen != 0; // offen == 0 ist Fehler;
    }
  }
}
