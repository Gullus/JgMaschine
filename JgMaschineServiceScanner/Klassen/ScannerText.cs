using System;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceScanner
{
  public class ScannerText
  {
    private string _CraddleEmpfangsText;
    private string _TextEmpfangen = "";
    private char _Esc = Convert.ToChar(27);

    public bool ScannerIstMitDisplay = false;

    public string ScannerKennung => (_TextEmpfangen.Length < 13) ? null : _TextEmpfangen.Substring(0, 13);
    public string ScannerVorgangScan => (_TextEmpfangen.Length < 17) ? null : _TextEmpfangen.Substring(13, 4);
    public string ScannerVorgangProgramm => (_TextEmpfangen.Length < 26) ? null : _TextEmpfangen.Substring(17, 9);

    public string ScannerKoerper
    {
      get
      {
        switch (_VorgangScan)
        {
          case VorgangScanner.PROG:
            return _TextEmpfangen.Substring(26);
          case VorgangScanner.SCHALTER:
            return _TextEmpfangen[14].ToString();
          case VorgangScanner.MITA:  // Zeichen 17 ist 0 - für Anmeldung, 1 - für Abmeldung
            return _TextEmpfangen.Substring(18);
          default:
            return _TextEmpfangen.Substring(17);       // bei allen anderen Vorgängen
        }
      }
    }

    private VorgangScanner _VorgangScan = VorgangScanner.FEHLER;
    public VorgangScanner VorgangScan => _VorgangScan;

    private VorgangProgram _VorgangProgramm = VorgangProgram.FEHLER;
    public VorgangProgram VorgangProgramm => _VorgangProgramm;

    private string _ZusatzText = "";
    private string[] _AusgabeZeilen = { " ", " ", " ", " ", " " };

    private string _AlarmMeldungsTon = null;

    public ScannerText(string CraddleEmpfangsText)
    {
      _CraddleEmpfangsText = CraddleEmpfangsText;

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
      _AlarmMeldungsTon = sb.ToString();
    }

    public void TextVonScanner(string EmpfangenerText)
    {
      _TextEmpfangen = EmpfangenerText;
      _VorgangScan = VorgangScanner.FEHLER;
      _VorgangProgramm = VorgangProgram.FEHLER;
      _ZusatzText = "";
      _AusgabeZeilen = new string[] { " ", " ", " ", " ", " " };

      Logger.Write("=> " + _TextEmpfangen, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);

      if (_TextEmpfangen.Contains(_CraddleEmpfangsText))
      {
        _VorgangScan = VorgangScanner.CRADDLEANMELDUNG;
        _VorgangProgramm = VorgangProgram.CRADDLEANMELDUNG;
      }
      else if ((this._TextEmpfangen.Length == 16) && (this._TextEmpfangen[13] == 'S'))
      {
        _VorgangScan = VorgangScanner.SCHALTER;
        Logger.Write($"Schalter getrückt: {ScannerKoerper}", "Service", 0, 0, System.Diagnostics.TraceEventType.Verbose);
      }
      else if (this._TextEmpfangen.Length < 17)
        FehlerAusgabe("Text zu kurz");
      else
      {
        if (!Enum.TryParse<VorgangScanner>(ScannerVorgangScan, true, out _VorgangScan))
          FehlerAusgabe("Scan Vorgang falsch.", "Wert: " + ScannerVorgangScan);
        else
        {
          switch (_VorgangScan)
          {
            case VorgangScanner.BF2D: _VorgangProgramm = VorgangProgram.BAUTEIL; break;
            case VorgangScanner.FEHLER: _VorgangProgramm = VorgangProgram.FEHLER; break;
            case VorgangScanner.SCHALTER: _VorgangProgramm = VorgangProgram.SCHALTER; break;
            case VorgangScanner.TEST: _VorgangProgramm = VorgangProgram.TEST; break;
            case VorgangScanner.MITA:
              if (_TextEmpfangen[17] == '0')
                _VorgangProgramm = VorgangProgram.ANMELDUNG;
              else
                _VorgangProgramm = VorgangProgram.ABMELDUNG;
              break;
            case VorgangScanner.PROG:
              if (!Enum.TryParse<VorgangProgram>(ScannerVorgangProgramm, true, out _VorgangProgramm))
                FehlerAusgabe("Prog. Vorgang falsch.", "Wert: " + ScannerVorgangProgramm);
              break;
          }
        }
      }
    }

    public byte[] PufferSendeText()
    {
      var sb = new StringBuilder(ScannerKennung);

      if (ScannerIstMitDisplay)
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

    public string TextCenter(string Text)
    {
      if (Text.Length > 24)
        Text = Text.Substring(0, 24);

      if (Text.Length < 23)
      {
        var anz = Convert.ToInt32((22 - Text.Length) / 2);
        return "".PadLeft(anz) + Text;
      }
      return Text;
    }

    public void SendeText(params string[] Zeilen)
    {
      for (int i = 0; i < Zeilen.Length; i++)
        _AusgabeZeilen[i] = TextCenter(Zeilen[i]);
    }

    public void FehlerAusgabe(params string[] FehlerText)
    {
      _AusgabeZeilen[0] = TextCenter("- F E H L E R -");

      for (int i = 0; i < FehlerText.Length; i++)
        _AusgabeZeilen[i + 1] = TextCenter(FehlerText[i]);

      _ZusatzText = _AlarmMeldungsTon;
    }
  }
}
