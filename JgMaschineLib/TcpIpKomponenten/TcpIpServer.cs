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
    private bool CloseNachEmpfang { get; set; } = true;
    private string DatenZumServerZurueck { get; set; } = "";

    public TcpIpServer(int PortNummer)
    {
      this.Port = PortNummer;
    }

    private void Anzeige(string AnzeigeText, params object[] AnzeigeWerte)
    {
      Console.WriteLine("[Server] " + string.Format(AnzeigeText, AnzeigeWerte));
    }

    public async Task<int> Start()
    {
      var ipLocalhost = Helper.GetIpAdressV4(Dns.GetHostName());
      _Listener = new TcpListener(ipLocalhost, this.Port);

      Anzeige($"Adresse: {ipLocalhost} Port: {this.Port}");
      Anzeige("Server gestartet...");
      _Listener.Start(500);

      while (true)
      {
        var tcpClient = await _Listener.AcceptTcpClientAsync();
        Anzeige("Client verbunden ...");

        try
        {
          Task task = Task.Factory.StartNew(client =>
            {
              using (var networkStream = tcpClient.GetStream())
              {
                var buffer = new byte[4096];
                var anzahlEmpfangen = networkStream.Read(buffer, 0, buffer.Length);
                var empfangen = Helper.BufferInString(buffer, anzahlEmpfangen);
                Anzeige("{0} Zeichen vom Client Empfangener,  Text: {1}", anzahlEmpfangen, empfangen);

                if (DatenZumServerZurueck != "")
                {
                  var senden = Helper.StringInBuffer("Zurück: " + DatenZumServerZurueck);
                  networkStream.Write(senden, 0, senden.Length);
                  Anzeige("Daten vom Server zurück gesendet: {0}", senden.Length);
                }

                if (CloseNachEmpfang)
                {
                  tcpClient.Close();
                  Anzeige("Verbindung geschlossen ...");
                }
              }
            }, tcpClient);
        }
        catch (Exception f)
        {
          Anzeige("Serverfehler " + f.Message);
        }
        finally
        {
          tcpClient.Close();
        }
      }
    }
  }
}
