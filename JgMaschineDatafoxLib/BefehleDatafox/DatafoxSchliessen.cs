namespace JgMaschineDatafoxLib
{
  public static partial class ProgDatafox
  {
    /// <summary>
    /// Schliessen der Verbindung
    /// </summary>
    /// <param name="Optionen">Übertragungsoptionen</param>
    public static void DatafoxSchliessen(OptionenDatafox Optionen)
    {
      DFComDLL.DFCComClose(Optionen.Terminal.ChannelId);
    }
  }
}
