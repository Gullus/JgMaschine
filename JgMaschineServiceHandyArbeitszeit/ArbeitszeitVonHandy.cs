using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineLib.Arbeitszeit;
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
    public TcpListener Listener { get { return _Listener; } }

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
      public string userId { get; set; }
      public DateTime timestamp { get; set; }
      public bool isCheckIn { get; set; }
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
            var fehler = true;
            var ho = (HandyOptionen)handyOpt;
            NetworkStream nwStream = null;

            try
            {
              var clientIp = ((IPEndPoint)ho.Client.Client.RemoteEndPoint).Address.ToString();
              var clientPort = ((IPEndPoint)ho.Client.Client.RemoteEndPoint).Port;

              msg = $"Client verbunden:\nPort: {clientIp} Port: {clientPort}";
              Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);

              var buffer = new byte[8192];
              nwStream = client.GetStream();
              var anzahlZeichen = nwStream.Read(buffer, 0, buffer.Length);

              var empf = JgMaschineLib.TcpIp.Helper.BufferInString(buffer, anzahlZeichen);
              List<HandyDaten> listeHandyDaten = null;

              try
              {
                listeHandyDaten = JsonConvert.DeserializeObject<List<HandyDaten>>(empf);

                var werte = listeHandyDaten.Select(s => $"  {s.timestamp} - {s.userId} / {s.isCheckIn}").ToList();
                msg = $"{anzahlZeichen} Zeichnen vom Server Empfangen.\n {Helper.ListeInString(werte)}";
                Console.WriteLine(msg);

                Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);
              }
              catch (Exception ex)
              {
                msg = $"Fehler beim konvertieren der JSon Werte.\nGrund: {ex.Message}\nEmpfangen: {empf}";
                Logger.Write(msg, _Lc, 0, 0, TraceEventType.Error);
                throw;
              }

              DatenInDatenbankEintragen(listeHandyDaten);

              fehler = false;
            }
            catch (ObjectDisposedException ex)
            {
              msg = $"DisposeException.\nGrund: {ex.Message}";
              Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);
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
              try
              {
                if (ho.Client.Connected)
                {
                  var senden = JgMaschineLib.TcpIp.Helper.StringInBuffer(fehler ? "201" : "200"); // 200 - Daten erfolgreich eingetragen
                  nwStream.Write(senden, 0, senden.Length);
                }

                ho.Client.Close();
                nwStream?.Close();
              }
              catch { }
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
        Thread.Sleep(10000);
      }

      Environment.Exit(10);
    }

    public void DatenInDatenbankEintragen(List<HandyDaten> ListeDaten)
    {
      var listeAuswertung = ListeDaten.Select(s => new ArbeitszeitImportDaten()
      {
        Datum = s.timestamp,
        MatchCode = s.userId,
        Vorgang = s.isCheckIn ? ArbeitszeitImportDaten.EnumVorgang.Komme : ArbeitszeitImportDaten.EnumVorgang.Gehen
      }).ToList();

      var msg = "";
      try
      {
        using (var Db = new JgModelContainer())
        {
          if (!string.IsNullOrWhiteSpace(_ConnectionString))
            Db.Database.Connection.ConnectionString = _ConnectionString;

          var azImport = new ArbeitszeitImport();
          azImport.ImportStarten(Db, listeAuswertung);

          Db.SaveChanges();
          msg = $"{azImport.AnzahlAnmeldungen} Handy Anmeldungen in Datenbank gespeichert.\n{azImport.ProtokollOk}";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);

          if (azImport.ProtokollFehler != null)
          {
            msg = $"Anmeldungen ohne Benutzerzuordnung.\n{azImport.ProtokollFehler}";
            Logger.Write(msg, "Service", 0, 0, TraceEventType.Warning);
          }
        }
      }
      catch (Exception f)
      {
        msg = "Fehler beim eintragen der Anmeldedaten in die Datenbank.";
        throw new MyException(msg, f);
      }
    }
  }
}

