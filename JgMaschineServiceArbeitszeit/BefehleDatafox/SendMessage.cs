namespace JgMaschineServiceArbeitszeit
{
  public static partial class ProgDatafox
  {
    /// <summary>
    /// Anzeige eines Textes auf dem Datafox Gerät
    /// </summary>
    /// <param name="Optionen">Übertragungsoptionen</param>
    /// <param name="MessageText">Text, welcher auf dem Display ausgeben werden soll</param>
    /// <param name="Tonfolge">Tonfolge: 0 - Kein Ton; 1-10 siehe Documentation</param>
    public static void Send(OptionenTerminal Optionen, string MessageText, byte Tonfolge = 7)
    {
      DatafoxOeffnen(Optionen);
      DFComDLL.DFCComSendMessage(Optionen.ChannelId, Optionen.DeviceId, Optionen.IdVerbindung, 0, Tonfolge, MessageText, MessageText.Length);
      DatafoxSchliessen(Optionen);
    }
  }
}
