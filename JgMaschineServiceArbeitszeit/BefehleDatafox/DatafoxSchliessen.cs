namespace JgMaschineServiceArbeitszeit
{
  public static partial class ProgDatafox
  {
    /// <summary>
    /// Schliessen der Verbindung
    /// </summary>
    /// <param name="Optionen">Übertragungsoptionen</param>
    public static void DatafoxSchliessen(OptionenTerminal Optionen)
    {
      DFComDLL.DFCComClose(Optionen.ChannelId);
    }
  }
}
