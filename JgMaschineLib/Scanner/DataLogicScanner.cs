using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JgMaschineLib.Scanner
{
  public delegate void ScannerTextDelegate(DataLogicScannerText e);

  public class DataLogicScanner
  {
    public enum VorgangScanner
    {
      FEHLER,
      PROG,
      MITA,  // Mitarbeiter Anmeldung an Maschine  
      BF2D,
      BF3D,
      TEST,
      SCHALTER
    }

    public enum VorgangProgram
    {
      FEHLER,
      BAUTEIL,
      ANMELDUNG,
      ABMELDUNG,
      COILSTART,
      COIL_ENDE,
      REPASTART,
      REPA_ENDE,
      WARTSTART,
      WART_ENDE,
      SCHALTER,
      TEST
    }

    private string _Adresse = "";
    private int _Port = 0;

    private const string _FehlerDatenEmpfang = "#[SCANNERFEHLER]#";

    private bool _AnzeigeProtokoll = false;

    private TcpClient _Client;
    private NetworkStream _NetStream;

    private ScannerTextDelegate _ScannerCom = null;

    public DataLogicScanner(string Adresse, int Port, ScannerTextDelegate ScannerText, bool AnzeigeProtokoll = false)
    {
      _Adresse = Adresse;
      _Port = Port;
      _AnzeigeProtokoll = AnzeigeProtokoll;
      _ScannerCom = ScannerText;
    }

    private void Protokoll(string FehlerText, params object[] Felder)
    {
      if (_AnzeigeProtokoll)
        Console.WriteLine(FehlerText, Felder);
    }

    private bool istPingOk()
    {
      Ping sender = new Ping();
      PingReply result = null;
      try
      {
        result = sender.Send(_Adresse);
      }
      catch (Exception f)
      {
        Protokoll("Fehler bei Pingabfrage.\nGrund: {0}", f.Message);
        return false;
      }

      if (result.Status == IPStatus.Success)
        Protokoll("Ping erfolgreich.");
      else
        Protokoll("Ping fehlgeschlagen mit IPStatus: {0}", result.Status);

      return result.Status == IPStatus.Success;
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
      while (true)
      {
        if (!istPingOk())
          Thread.Sleep(10000);
        else
        {
          try
          {
            _Client = new TcpClient(_Adresse, _Port);
          }
          catch (Exception f)
          {
            Protokoll("Fehler beim Client Verbinungsaufbau.\nGrund: {0}", f.Message);
            Thread.Sleep(10000);
            continue;
          }

          Protokoll("Verbindung mit Server hergestellt.");

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

                if (!istPingOk())
                  break;

                if (ct.IsCancellationRequested)
                  break;
              }
            }, _Adresse, ct);

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
                  Protokoll("Fehler beim Empfang der Daten.\r\nGrund: " + f.Message);
                  return _FehlerDatenEmpfang;
                }
              }

              // -1 ist für das herausfiltern des Steuerezichens vom Scanner

              return Encoding.ASCII.GetString(bufferEmpfang, 0, anzZeichen - 1);

            }, _NetStream, ct);

            Task.WaitAny(new Task[] { taskKontrolle, taskScannen });

            cts.Cancel();
            if (taskScannen.IsCompleted)
            {
              var textEmpfangen = taskScannen.Result;
              if (textEmpfangen == _FehlerDatenEmpfang)
              {
                _Client.Close();
                break;
              }
              else if (textEmpfangen.Length > 0)
              {
                var scText = new DataLogicScannerText(textEmpfangen);

                Protokoll(scText.TextEmpfangen);

                _ScannerCom.Invoke(scText);

                byte[] senden = scText.PufferSendeText();
                _NetStream.Write(senden, 0, senden.Length);
              }
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

  public class DataLogicScannerText
  {
    private char _Esc = Convert.ToChar(27);

    public string TextEmpfangen { get; set; }
    public bool MitDisplay { get; set; }

    public string ScannerKennung { get { return (TextEmpfangen.Length < 13) ? "" : TextEmpfangen.Substring(0, 13); } }
    public string ScannerVorgangScan { get { return (TextEmpfangen.Length < 17) ? "" : TextEmpfangen.Substring(13, 4); } }
    public string ScannerVorgangProgramm { get { return (TextEmpfangen.Length < 26) ? "" : TextEmpfangen.Substring(17, 9); } }

    public string ScannerKoerper
    {
      get
      {
        switch (_VorgangScan)
        {
          case DataLogicScanner.VorgangScanner.PROG:
            return TextEmpfangen.Substring(26);
          case DataLogicScanner.VorgangScanner.SCHALTER:
            return TextEmpfangen[14].ToString();
          case DataLogicScanner.VorgangScanner.MITA:  // Zeichen 17 ist 0 - für Anmeldung, 1 - für Abmeldung
            return TextEmpfangen.Substring(18);
          default:
            return TextEmpfangen.Substring(17);
        }
      }
    }

    private string _ZusatzText = "";

    private DataLogicScanner.VorgangScanner _VorgangScan = DataLogicScanner.VorgangScanner.FEHLER;
    public DataLogicScanner.VorgangScanner VorgangScan
    {
      get { return _VorgangScan; }
      set { _VorgangScan = value; }
    }

    private DataLogicScanner.VorgangProgram _VorgangProgramm = DataLogicScanner.VorgangProgram.FEHLER;
    public DataLogicScanner.VorgangProgram VorgangProgramm
    {
      get { return _VorgangProgramm; }
      set { _VorgangProgramm = value; }
    }

    private string[] _AusgabeZeilen = { " ", " ", " ", " ", " " };

    public DataLogicScannerText(string EmpfangenerText)
    {
      MitDisplay = true;
      this.TextEmpfangen = EmpfangenerText;

      if ((this.TextEmpfangen.Length == 16) && (this.TextEmpfangen[13] == 'S'))
      {
        VorgangScan = DataLogicScanner.VorgangScanner.SCHALTER;
        Console.WriteLine("Schalter getrückt: {0}", ScannerKoerper);
      }
      else if (this.TextEmpfangen.Length < 17)
        FehlerAusgabe("Text zu kurz");
      else
      {
        if (!Enum.TryParse<DataLogicScanner.VorgangScanner>(ScannerVorgangScan, true, out _VorgangScan))
          FehlerAusgabe("Scan Vorgang falsch.", "Wert: " + ScannerVorgangScan);
        else
        {
          switch (_VorgangScan)
          {
            case DataLogicScanner.VorgangScanner.BF2D: _VorgangProgramm = DataLogicScanner.VorgangProgram.BAUTEIL; break;
            case DataLogicScanner.VorgangScanner.FEHLER: _VorgangProgramm = DataLogicScanner.VorgangProgram.FEHLER; break;
            case DataLogicScanner.VorgangScanner.SCHALTER: _VorgangProgramm = DataLogicScanner.VorgangProgram.SCHALTER; break;
            case DataLogicScanner.VorgangScanner.TEST: _VorgangProgramm = DataLogicScanner.VorgangProgram.TEST; break;
            case DataLogicScanner.VorgangScanner.MITA:
              if (TextEmpfangen[17] == '0')
                _VorgangProgramm = DataLogicScanner.VorgangProgram.ANMELDUNG;
              else
                _VorgangProgramm = DataLogicScanner.VorgangProgram.ABMELDUNG;
              break;
            case DataLogicScanner.VorgangScanner.PROG:
              if (!Enum.TryParse<DataLogicScanner.VorgangProgram>(ScannerVorgangProgramm, true, out _VorgangProgramm))
                FehlerAusgabe("Prog. Vorgang falsch.", "Wert: " + ScannerVorgangProgramm);
              break;
          }
        }
      }
    }

    public byte[] PufferSendeText()
    {
      var sb = new StringBuilder(ScannerKennung);

      if (MitDisplay)
      {
        sb.Append(_Esc + "[2J");
        for (int i = 0; i < _AusgabeZeilen.Length; i++)
        {
          sb.Append(_Esc + "[0K" + (_AusgabeZeilen[i].Length > 22 ? _AusgabeZeilen[i].Substring(0, 22) : _AusgabeZeilen[i]) + _Esc + "[G");
          if (i < _AusgabeZeilen.Length - 1)
            sb.Append(_Esc + "[G");
        }
      }

      sb.Append(_ZusatzText);
      sb.Append(Convert.ToChar(13));

      return Encoding.ASCII.GetBytes(sb.ToString());
    }

    public void SendeText(params string[] Zeilen)
    {
      if (_VorgangScan != DataLogicScanner.VorgangScanner.FEHLER)
      {
        for (int i = 0; i < Zeilen.Length; i++)
          _AusgabeZeilen[i] = Zeilen[i];
      }
    }

    public void FehlerAusgabe(params string[] FehlerText)
    {
      _VorgangScan = DataLogicScanner.VorgangScanner.FEHLER;
      _AusgabeZeilen[0] = "    - F E H L E R -";

      for (int i = 0; i < FehlerText.Length; i++)
        _AusgabeZeilen[i + 1] = FehlerText[i];

      var sb = new StringBuilder();
      for (int i = 0; i < 4; i++)
      {
        sb.Append(_Esc + "[4q");                       // Klingel
        sb.Append(_Esc + "[8q");                       // Rote LED an
        sb.Append(_Esc + "[5q" + _Esc + "[5q");        // 2 x 100 ms warten
        sb.Append(_Esc + "[9q");                       // Rote LED aus
        if (i < 3)
          sb.Append(_Esc + "[5q" + _Esc + "[5q");      // 2 x 100 ms warten
      }
      _ZusatzText = sb.ToString();
    }

  }
}