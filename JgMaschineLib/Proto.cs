using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
      ServiceScanner
    }

    public KategorieArt _Kategorie;

    public enum ProtoArt
    {
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
              AnzeigeWinProtokoll(erg, Proto.ProtoArt.Warnung);
          }
        };
      }
    }

    public void AddAuswahl(ProtoArt Art, AnzeigeArt Anzeige)
    {
      _ListAnzeige.Add(new ProtoAuswahl(Art, Anzeige));
    }

    public static void PfadeInWindowsEreignissAnzeigeSetzten()
    {
      foreach (var name in Enum.GetNames(typeof(KategorieArt)))
      {
        try
        {
          if (! EventLog.SourceExists(name))
            EventLog.CreateEventSource(name, "JgMaschine");
        }
        catch (Exception f)
        {
          var msg = $"Fehler beim Einrichten des Pfades !\nGrund: {f.Message}";
          Helper.Protokoll(msg, Helper.ProtokollArt.Warnung);
        }
      }
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
            case ProtoArt.Info: CaptionText = "Information"; break;
            case ProtoArt.Warnung: CaptionText = "Warnung"; break;
            case ProtoArt.Fehler: CaptionText = "Fehler"; break;
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
      if (EventLog.SourceExists(_Kategorie.ToString()))
      {
      var logType = EventLogEntryType.Error;

        switch (AnzeigeArt)
        {
          case ProtoArt.Info:
            logType = EventLogEntryType.Information; break;
          case ProtoArt.Warnung:
            logType = EventLogEntryType.Warning; break;
        }

        EventLog.WriteEntry(_Kategorie.ToString() , ProtokollText, logType, 1);
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
      Console.WriteLine($"{_Kategorie} {AnzeigeArt}: {ProtokollText}");
    }

    public void AnzeigeFenster(string CaptionText, string ProtokollText, ProtoArt AnzeigeArt = ProtoArt.Fehler)
    {
      MessageBoxIcon icon = MessageBoxIcon.Error;
      switch (AnzeigeArt)
      {
        case ProtoArt.Info:
          icon = MessageBoxIcon.Information; break;
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
