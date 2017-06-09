using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceScanner
{
  public delegate void ScannerAusgeloestDelegate(ScannerText ScText);

  public class DataLogicScanner
  {
    private const string _FehlerDatenEmpfang = "#[SCANNERFEHLER]#";

    private ScannerOptionen _Optionen;

    private TcpClient _Client;
    private NetworkStream _NetStream;

    private ScannerText _ScannerText;
    public ScannerText ScText { get => _ScannerText; }

    private ScannerAusgeloestDelegate _ScannerAusgeloest = null;

    public DataLogicScanner(ScannerOptionen Optionen, ScannerAusgeloestDelegate ScannerAusgeloest)
    {
      _Optionen = Optionen;
      _ScannerAusgeloest = ScannerAusgeloest;

      _ScannerText = new ScannerText(_Optionen.CradleTextAnmeldung);
    }

    public void Close()
    {
      try
      {
        _NetStream.Close();
        _NetStream.Dispose();
        _Client.Close();
      }
      catch { };
    }

    public void Start()
    {
      var msg = $"Start Scanner\nScanneradresse: {_Optionen.CradleIpAdresse} - Port: {_Optionen.CradlePortNummer}";
      Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Information);

      while (true)
      {
        if (!JgMaschineLib.Helper.IstPingOk(_Optionen.CradleIpAdresse, out msg))
          Thread.Sleep(30000);
        else
        {
          try
          {
            _Client = new TcpClient(_Optionen.CradleIpAdresse, _Optionen.CradlePortNummer);
          }
          catch (Exception f)
          {
            msg = $"Fehler beim Verbinungsaufbau zum Cradle.\nGrund: {f.Message}";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Error);
            Thread.Sleep(30000);
            continue;
          }

          msg = "Verbindung mit Cradle hergestellt.";
          Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);

          _NetStream = _Client.GetStream();

          while (true)
          {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            // Wenn dieser Task eher als Scanner beendet wird, liegt ein Verbindungsfehler vor;

            var taskKontrolle = Task.Factory.StartNew((Adresse) =>
            {
              var adresse = (string)Adresse;

              while (true)
              {
                Thread.Sleep(10000);
                if (ct.IsCancellationRequested)
                  break;

                if (!JgMaschineLib.Helper.IstPingOk(_Optionen.CradleIpAdresse, out msg))
                  break;

                if (ct.IsCancellationRequested)
                  break;
              }
            }, _Optionen.CradleIpAdresse, ct);

            var taskScannen = Task.Factory.StartNew<string>((nStream) =>
            {
              byte[] bufferEmpfang = new byte[4096];
              int anzZeichen = 0;

              try
              {
                anzZeichen = _NetStream.Read(bufferEmpfang, 0, bufferEmpfang.Length);
              }
              catch (Exception f)
              {
                _NetStream.Close();
                _NetStream.Dispose();
                if (!ct.IsCancellationRequested)
                {
                  msg = $"Fehler beim Empfang der Daten von Sanner!\nGrund: {f.Message}";
                  Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
                  return _FehlerDatenEmpfang;
                }
              }

              return Encoding.ASCII.GetString(bufferEmpfang, 0, anzZeichen);

            }, _NetStream, ct);

            Task.WaitAny(new Task[] { taskKontrolle, taskScannen });

            cts.Cancel();
            if (taskScannen.IsCompleted)
            {
              var textEmpfangen = taskScannen.Result;
              if ((textEmpfangen == _FehlerDatenEmpfang) || (textEmpfangen.Length <= 1))
              {
                if (textEmpfangen == _FehlerDatenEmpfang)
                {
                  msg = "Fehlertext wurde angesprochen.";
                  Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
                }
                else
                {
                  var zeichen = "";
                  if (textEmpfangen.Length == 1)
                    zeichen = " Text: " + textEmpfangen + " Byte: " + Convert.ToByte(textEmpfangen[0]);

                  msg = $"Unklarer Text empfangen. Anzahl der Zeichen: {textEmpfangen.Length}{zeichen}";
                  Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
                }

                _Client.Close();
                break;
              }

              // Letztes Zeichen, Seuerzeichen entfernen

              _ScannerText.TextVonScanner(textEmpfangen.Substring(0, textEmpfangen.Length - 1));
              _ScannerAusgeloest(_ScannerText);

              byte[] senden = _ScannerText.PufferSendeText();
              _NetStream.Write(senden, 0, senden.Length);
            }
            else
            {
              _Client.Close();
              break;
            }
          }
        }
      }
    }
  }
}