using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JgMaschineLib.Email;

namespace JgMaschineLib
{
  public class Proto
  {
    public enum KategorieArt
    {
      Arbeitszeit,
      Setup,
      Verwaltung,
      Auswertung,
      ServiceArbeitszeit,
      ServiceScanner,
      ServiceDatenabfrage,
      ServiceDataMerge
    }

    public KategorieArt _Kategorie;

    public enum ProtoArt
    {
      Kommentar,
      Info,
      Warnung,
      Fehler
    }

    public enum AnzeigeArt
    {
      Console,
      Fenster,
      Email,
      WinProtokoll,
      DebugFenster
    }

    public SendEmailOptionen EmailOptionen = null;
    private SendEmail _Email;

    public List<ProtoAuswahl> _ListAnzeige = new List<ProtoAuswahl>();

    public Proto(KategorieArt Kategorie, SendEmailOptionen EmailOptionen = null)
    {
      _Kategorie = Kategorie;

      if (EmailOptionen != null)
      {
        this.EmailOptionen = EmailOptionen;
        _Email = new SendEmail()
        {
          Optionen = EmailOptionen,
          SendErgebniss = (erg, fehler) =>
          {
            if (fehler)
            {
              AnzeigeWinProtokoll(erg, Proto.ProtoArt.Warnung);
              AnzeigeConsole(erg, Proto.ProtoArt.Warnung);
            }
          }
        };
      }
    }

    public void AddAuswahl(ProtoArt Art, params AnzeigeArt[] Anzeige)
    {
      foreach (var anz in Anzeige)
        _ListAnzeige.Add(new ProtoAuswahl(Art, anz));
    }

    public static void PfadeInWindowsEreignissAnzeigeSetzten()
    {
      var prot = new StringBuilder();

      foreach (var name in Enum.GetNames(typeof(KategorieArt)))
      {
        var logName = "JgMaschine";
        try
        {
          var kategorie = name.ToString();
          if (EventLog.SourceExists(kategorie))
          {
            var logKontrolle = EventLog.LogNameFromSourceName(kategorie, ".");
            if (logKontrolle != logName)
            {
              EventLog.DeleteEventSource(kategorie);
              EventLog.CreateEventSource(kategorie, logName);
              prot.AppendLine($"{kategorie} von {logKontrolle} nach {logName} geändert");
            }
            else
              prot.AppendLine($"{kategorie} - vorhanden");
          }
          else
          { 
            EventLog.CreateEventSource(kategorie, logName);
            prot.AppendLine($"{kategorie} - eingetragen"); 
          }
        }
        catch (Exception f)
        {
          prot.AppendLine($"Fehler: {name} : {f.Message}");
        }
        prot.AppendLine();
      }

      Helper.Protokoll(prot.ToString(), Helper.ProtokollArt.Info);
    }

    public void Set(string ProtokollText, Exception Fehler, string Caption = "")
    {
      var msg = $"{ProtokollText}\nGrund: {Fehler.Message}";
      var inExcep = Fehler.InnerException;
      int zaehler = 0;
      while (inExcep != null)
      {
        zaehler++;
        msg += $"\nInExcep {zaehler}: {inExcep.Message}";
        inExcep = inExcep.InnerException;
      }
      Set(msg, ProtoArt.Fehler, Caption);
    }

    public void Set(string ProtokollText, ProtoArt ProtokollArt, string CaptionText = "")
    {
      var anzeigen = _ListAnzeige.Where(w => w.Art == ProtokollArt).ToList();
      if (anzeigen.Count > 0)
      {
        if (CaptionText == "")
        {
          switch (ProtokollArt)
          {
            case ProtoArt.Kommentar: CaptionText = "JgMaschine - Kommentar"; break;
            case ProtoArt.Info: CaptionText = "JgMaschine - Information"; break;
            case ProtoArt.Warnung: CaptionText = "JgMaschine - Warnung!"; break;
            case ProtoArt.Fehler: CaptionText = "JgMaschine - Fehler!"; break;
          }
        }
        foreach (var anz in anzeigen)
        {
          switch (anz.Anzeige)
          {
            case AnzeigeArt.Console:
              AnzeigeConsole(ProtokollText, anz.Art); break;
            case AnzeigeArt.Fenster:
              AnzeigeFenster(CaptionText, ProtokollText, anz.Art); break;
            case AnzeigeArt.Email:
              AnzeigeEmail(CaptionText, ProtokollText, anz.Art); break;
            case AnzeigeArt.WinProtokoll:
              AnzeigeWinProtokoll(ProtokollText, anz.Art); break;
            case AnzeigeArt.DebugFenster:
              AnzeigeDebugger(ProtokollText, anz.Art); break;
            default:
              break;
          }
        }
      }
    }

    public void AnzeigeDebugger(string ProtokollText, ProtoArt AnzeigeArt = ProtoArt.Fehler)
    {
      Debugger.Log(Convert.ToByte(AnzeigeArt), _Kategorie.ToString(), $"{AnzeigeArt}: {ProtokollText}");
    }

    public void AnzeigeWinProtokoll(string ProtokollText, ProtoArt AnzeigeArt = ProtoArt.Fehler)
    {
      var kategorie = _Kategorie.ToString();

      if (EventLog.SourceExists(kategorie))
      {
        var logType = EventLogEntryType.Information;

        switch (AnzeigeArt)
        {
          case ProtoArt.Info:
            logType = EventLogEntryType.SuccessAudit; break;
          case ProtoArt.Fehler:
            logType = EventLogEntryType.Error; break;
          case ProtoArt.Warnung:
            logType = EventLogEntryType.Warning; break;
        }

        EventLog.WriteEntry(kategorie, ProtokollText, logType, 1);
      }
    }

    public void AnzeigeEmail(string CaptionText, string ProtokollText, ProtoArt AnzeigeArt = ProtoArt.Fehler)
    {
      if (_Email != null)
      {
        _Email.Optionen.Koerper = @"<html lang = \""de\"" xmlns = \""http://www.w3.org/1999/xhtml\"">"
          + @"<body><h3>" + CaptionText + @"</h3><p>" + ProtokollText + @"</p></body></html>";
        _Email.Send();
      }
    }

    public void AnzeigeConsole(string ProtokollText, ProtoArt AnzeigeArt = ProtoArt.Fehler)
    {
      Console.WriteLine($"{DateTime.Now.ToString("dd.MM HH:mm:ss")}  {_Kategorie} {AnzeigeArt}: {ProtokollText}");
    }

    public void AnzeigeFenster(string CaptionText, string ProtokollText, ProtoArt AnzeigeArt = ProtoArt.Fehler)
    {
      MessageBoxIcon icon = MessageBoxIcon.Information;
      switch (AnzeigeArt)
      {
        case ProtoArt.Fehler:
          icon = MessageBoxIcon.Error; break;
        case ProtoArt.Warnung:
          icon = MessageBoxIcon.Warning; break;
      }

      MessageBox.Show(ProtokollText, CaptionText, MessageBoxButtons.OK, icon);
    }
  }

  public class ProtoAuswahl
  {
    public Proto.ProtoArt Art { get; set; }
    public Proto.AnzeigeArt Anzeige { get; set; }
    public ProtoAuswahl(Proto.ProtoArt Art, Proto.AnzeigeArt Anzeige)
    {
      this.Art = Art;
      this.Anzeige = Anzeige;
    }
  }
}
