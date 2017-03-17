namespace JgMaschineServiceArbeitszeit
{
  public class OptionenTerminal
  {
    public string IpAdresse = "192.168.1.57";
    public int Portnummer = 8000;
    public int TimeOut = 3000;
    public int ChannelId = 1;       // Nur interessant bei mehreren Terminals
    public int DeviceId = 254;      // Bei Ip Verbindung immer 254
    public byte IdVerbindung = 3;   // <= Verbindung über TcpIp

    public OptionenTerminal(string NeuIpAdresse, int NeuPortnummer, int NeuTimeOut)
    {
      IpAdresse = NeuIpAdresse;
      Portnummer = NeuPortnummer;
      TimeOut = NeuTimeOut;
    }
  }
}
