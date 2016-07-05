using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace JgMaschineTestTcpFirmaSchnell
{
  class Program
  {
    static void Main(string[] args)
    {
      var ipLocalhost = JgMaschineLib.TcpIp.Helper.GetIpAdressV4(Dns.GetHostName());
      var listener = new TcpListener(ipLocalhost, Properties.Settings.Default.Portnummer);

      Anzeige($"Adresse: {ipLocalhost} Port: {Properties.Settings.Default.Portnummer}");
      Anzeige("Server gestartet...");
      listener.Start(100);

      while (true)
      {
        var tcpClient = listener.AcceptTcpClient();
        Anzeige("Client verbunden ...");

        try
        {
          using (var networkStream = tcpClient.GetStream())
          {
            var buffer = new byte[4096];
            var anzahlEmpfangen = networkStream.Read(buffer, 0, buffer.Length);
            var empfangen = JgMaschineLib.TcpIp.Helper.BufferInString(buffer, anzahlEmpfangen);
            Anzeige("{0} Zeichen vom Client Empfangener,  Text: {1}", anzahlEmpfangen, empfangen);

            //var senden = Helper.StringInBuffer("Zurück: " + DatenZumServerZurueck);
            //networkStream.Write(senden, 0, senden.Length);
            //Anzeige("Daten vom Server zurück gesendet: {0}", senden.Length);

            tcpClient.Close();
            Anzeige("Verbindung geschlossen ...");
          }
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

    private static void Anzeige(string AnzeigeText, params object[] AnzeigeWerte)
    {
      Console.WriteLine("[Server] " + string.Format(AnzeigeText, AnzeigeWerte));
    }
  }
}
