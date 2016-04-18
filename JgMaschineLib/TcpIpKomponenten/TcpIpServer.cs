using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JgMaschineLib.TcpIp
{
  public class TcpIpServer : TcpIpStamm
  {
    private TcpListener _Listener;

    public TcpIpServer(int PortNummer)
    {
      this.Port = PortNummer;
    }

    private void Anzeige(string AnzeigeText, params object[] AnzeigeWerte)
    {
      Console.WriteLine("[Server] " + string.Format(AnzeigeText, AnzeigeWerte));
    }

    public async void Start()
    {
      var ipLocalhost = Helper.GetIpAdressV4(Dns.GetHostName());
      _Listener = new TcpListener(ipLocalhost, this.Port);

      Anzeige("Server gestartet...");
      _Listener.Start(500);

      while (true)
      {
        try
        {
          var tcpClient = await _Listener.AcceptTcpClientAsync();
          Anzeige("Client verbunden ...");

          Task task = Task.Factory.StartNew(client =>
            {
              using (var networkStream = tcpClient.GetStream())
              {
                var buffer = new byte[4096];
                var anzahlEmpfangen = networkStream.Read(buffer, 0, buffer.Length);
                var empfangen = Helper.BufferInString(buffer, anzahlEmpfangen);
                Anzeige("{0} Zeichen vom Client Empfangener,  Text: {1}", anzahlEmpfangen, empfangen);

                double b = 0.5;
                for (int i = 0; i < 10000000; i++)
                {
                  b = (b + 299.938) / (i + 0.763537);
                }

                var senden = Helper.StringInBuffer("Zurück: " + empfangen + "  " + b.ToString());
                networkStream.Write(senden, 0, senden.Length);
                Anzeige("Daten vom Server zurück gesendet: {0}", senden.Length);
              }
            }, tcpClient);
        }
        catch (Exception f)
        {
          Anzeige("Serverfehler " + f.Message);
        }
      }
    }
  }
}
