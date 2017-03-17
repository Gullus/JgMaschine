using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Newtonsoft.Json;

namespace JgMaschineServiceHandyArbeitszeit
{
  public class ArbeitszeitVonHandy
  {
    //"Manage NuGet packages" -> Search for "newtonsoft json".

    private const string _Lc = "JgDatenHandy";
    private string _ConnectionString;
    private int _PortNummer;

    private TcpListener _Listener;

    public class HandyOptionen
    {
      public const string Lc = "JgDatenHandy";
      public string ConnectionString;

      public TcpClient Client = null;

      public HandyOptionen(string ConnectionStringDb, TcpClient ClientHandy)
      {
        Client = ClientHandy;
        ConnectionString = ConnectionStringDb;
      }
    }

    public class HandyDaten
    {
      public string IdMitarbeiter { get; set; }
      public string Zeitpunkt { get; set; }
      public string IstAnmeldung { get; set; }
    }

    public ArbeitszeitVonHandy(string ConnectionStringDb, int PortNummerServer)
    {
      _PortNummer = PortNummerServer;
      _ConnectionString = ConnectionStringDb;
    }

    public void Start()
    {
      string msg = $"Starte Server !";
      Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);

      string hostName = Dns.GetHostName();
      var hostIp = JgMaschineLib.TcpIp.Helper.GetIpAdressV4(Dns.GetHostName());

      try
      {
        _Listener = new TcpListener(hostIp, _PortNummer);
        _Listener.Start(200);

        msg = $"Listener gestartet:\n  Server: {hostIp} Port: {_PortNummer}";
        Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);

        while (true)
        {
          var client = _Listener.AcceptTcpClient();

          var hOpt = new HandyOptionen(_ConnectionString, client);

          Task.Factory.StartNew(handyOpt =>
          {
            var ho = (HandyOptionen)handyOpt;

            NetworkStream nwStream = null;

            try
            {
              var clientIp = ((IPEndPoint)ho.Client.Client.RemoteEndPoint).Address.ToString();
              var clientPort = ((IPEndPoint)ho.Client.Client.RemoteEndPoint).Port;

              msg = $"Client verbunden:\nPort: {clientIp} Port: {clientPort}";
              Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);

              while (true)
              {
                var buffer = new byte[4096];
                nwStream = client.GetStream();
                var anzahlZeichen = nwStream.Read(buffer, 0, buffer.Length);

                var empf = JgMaschineLib.TcpIp.Helper.BufferInString(buffer, anzahlZeichen);
                msg = $"{anzahlZeichen} Zeichnen vom Server Empfangen.\nText: {empf}";
                Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);

                try
                {
                  var listeHandyDaten = JsonConvert.DeserializeObject<List<HandyDaten>>(empf);
                  DatenInDatenbankEintragen(listeHandyDaten);
                }
                catch (Exception ex)
                {
                  msg = $"Fehler beim konvertieren der JSon Werte.\nGrund: {ex.Message}\nEmpfangen: {empf}";
                  Logger.Write(msg, _Lc, 0, 0, TraceEventType.Error);
                }

                var senden = JgMaschineLib.TcpIp.Helper.StringInBuffer($"{anzahlZeichen} Zeichen empfangen!");
                nwStream.WriteAsync(senden, 0, senden.Length);
              }
            }
            catch (ObjectDisposedException ex)
            {
              msg = $"DisposeException.\nGrund: {ex.Message}";
              Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);
              return;
            }
            catch (SocketException ex)
            {
              msg = $"Client Socket Abbruch.\nGrund: {ex.Message}";
              Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);
            }
            catch (IOException ex)
            {
              msg = $"Client IO Abbruch.\nGrund: {ex.Message}";
              Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);
            }
            catch (Exception ex)
            {
              throw new Exception("Fehler bei Clientverarbeitung !", ex);
            }
            finally
            {
              ho.Client.Close();
              nwStream?.Close();
            }
          }, hOpt, TaskCreationOptions.LongRunning);
        }
      }
      catch (Exception ex)
      {
        ExceptionPolicy.HandleException(ex, "Policy");
      }
      finally
      {
        _Listener.Stop();
        Thread.Sleep(1000);
      }
    }

    public void DatenInDatenbankEintragen(List<HandyDaten> ListeDaten)
    {


    } 
  }
}

