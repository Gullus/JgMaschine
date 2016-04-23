using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace JgMaschineLib.DataLogicScanner
{
  public delegate void ScannerDelegate(ScannerText e);

  public class Scanner
  {
    public enum VorgangScanner
    {
      FEHLER,
      PROG,
      MITA,
      BF2D,
      TEST
    }

    public enum VorgangProgram
    {
      FEHLER,
      BAUTEIL,
      BENUTZER,
      ABMELDUNG,
      COILSTART,
      COIL_ENDE,
      REPASTART,
      REPA_ENDE,
      WARTSTART,
      WART_ENDE
    }

    private Timer _Timer;
    private ScannerClient _ScannerClient;

    public Scanner(string Adresse, int Port, ScannerDelegate ScannerCom, bool AnzeigeProtokoll = false)
    {
      _ScannerClient = new ScannerClient(Adresse, Port, ScannerCom, AnzeigeProtokoll);
      TimerCallback tcb = _ScannerClient.ScannerManager;
      _Timer = new Timer(tcb, null, 100, 5000);
    }

    public void Close()
    {
      _ScannerClient.ScannerVerbindungClose();
      _Timer.Dispose();
    }
  }

  public class ScannerClient
  {
    private string _Adresse = "";
    private int _Port = 0;

    private bool _ClientLaeuft = false;
    private bool _PingAussetzen = false;
    private bool _AnzeigeProtokoll = false;

    private TcpClient _Client;
    private NetworkStream _NetStream;

    private ScannerDelegate _ScannerCom = null;

    public ScannerClient(string Adresse, int Port, ScannerDelegate ScannerCom, bool AnzeigeProtokoll)
    {
      _Adresse = Adresse;
      _Port = Port;
      _AnzeigeProtokoll = AnzeigeProtokoll;
      _ScannerCom = ScannerCom;
    }

    public void ScannerVerbindungClose()
    {
      _NetStream.Close();
      _Client.Close();
    }

    public void ScannerManager(Object stateInfo)
    {
      if (_PingAussetzen)
        _PingAussetzen = false;
      else
      {
        Ping Sender = new Ping();
        PingReply Result = Sender.Send(_Adresse);
        if (Result.Status == IPStatus.Success)
        {
          if (_AnzeigeProtokoll)
            Console.WriteLine("Ping Erfolgreich");

          if (!_ClientLaeuft)
          {
            _ClientLaeuft = true;
            ScannerStart();
          }
        }
        else
        {
          if (_AnzeigeProtokoll)
            Console.WriteLine("Ping keine Antwort");

          if (_ClientLaeuft)
          {
            _ClientLaeuft = false;
            ScannerVerbindungClose();
          }
        }
      }
    }

    public async void ScannerStart()
    {
      try
      {
        _Client = new TcpClient(_Adresse, _Port);
        _NetStream = _Client.GetStream();

        if (_AnzeigeProtokoll)
          Console.WriteLine("Client gestartet");

        while (true)
        {
          if (_NetStream.CanRead)
          {
            byte[] bufferEmpfang = new byte[4096];
            int anzZeichen = 0;

            try
            {
              anzZeichen = await _NetStream.ReadAsync(bufferEmpfang, 0, bufferEmpfang.Length);
              _PingAussetzen = true;
            }
            catch (ObjectDisposedException)         // beim schließen des Streams tritt ein Fehler auf. Kein Lösung dafür gefunden.
            { }

            if (anzZeichen > 0)
            {

              var scText = new ScannerText(bufferEmpfang, anzZeichen);

              if (_AnzeigeProtokoll)
                Console.WriteLine(scText.TextEmpfangen);

              if (_ScannerCom != null)
                _ScannerCom.Invoke(scText);

              byte[] senden = scText.PufferSendeText();
              await _NetStream.WriteAsync(senden, 0, senden.Length);
            }
          }
        }
      }
      catch (Exception f)
      {
        Console.WriteLine("Sendefehler: " + f.Message);
      }
    }
  }

  public class ScannerText
  {
    private char _Esc = Convert.ToChar(27);

    public string TextEmpfangen { get; set; }
    public bool MitDisplay { get; set; }

    public string ScannerKennung { get { return TextEmpfangen.Substring(0, 13); } }
    public string ScannerVorgangScan { get { return TextEmpfangen.Substring(13, 4); } }
    public string ScannerVorgangProgramm { get { return TextEmpfangen.Substring(17, 9); } }

    public string ScannerKoerper 
    {
      get
      {
        if (_VorgangScan == Scanner.VorgangScanner.PROG)
          return TextEmpfangen.Substring(26);
        else
          return TextEmpfangen.Substring(17);
      }
    }

    private string _ZusatzText = "";

    private Scanner.VorgangScanner _VorgangScan = Scanner.VorgangScanner.FEHLER;
    public Scanner.VorgangScanner VorgangScan
    {
      get { return _VorgangScan; }
      set { _VorgangScan = value; }
    }

    private Scanner.VorgangProgram _VorgangProgramm = Scanner.VorgangProgram.FEHLER;
    public Scanner.VorgangProgram VorgangProgramm
    {
      get { return _VorgangProgramm; }
      set { _VorgangProgramm = value; }
    }

    private string[] _AusgabeZeilen = { " ", " ", " ", " ", " " };

    public ScannerText(byte[] EmpfangsPuffer, int AnzahlZeichen)
    {
      this.TextEmpfangen = Encoding.ASCII.GetString(EmpfangsPuffer, 0, AnzahlZeichen);

      Console.WriteLine(this.TextEmpfangen);

      if (this.TextEmpfangen.Length < 16)
        FehlerAusgabe("Text zu kurz");
      else
      {
        if (!Enum.TryParse<Scanner.VorgangScanner>(ScannerVorgangScan, true, out _VorgangScan))
          FehlerAusgabe("Scan Vorgang falsch.", "Wert: " + ScannerVorgangScan);
        else
        {
          switch (_VorgangScan)
          {
            case Scanner.VorgangScanner.BF2D: _VorgangProgramm = Scanner.VorgangProgram.BAUTEIL; break;
            case Scanner.VorgangScanner.MITA: _VorgangProgramm = Scanner.VorgangProgram.BENUTZER; break;
            default:
              if (!Enum.TryParse<Scanner.VorgangProgram>(ScannerVorgangProgramm, true, out _VorgangProgramm))
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
      if (_VorgangScan != Scanner.VorgangScanner.FEHLER)
      {
        for (int i = 0; i < Zeilen.Length; i++)
          _AusgabeZeilen[i] = Zeilen[i];
      }
    }

    public void FehlerAusgabe(params string[] FehlerText)
    {
      _VorgangScan = Scanner.VorgangScanner.FEHLER;

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
