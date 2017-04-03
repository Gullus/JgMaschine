using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JgMaschineLib;
using JgMaschineLib.Arbeitszeit;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceArbeitszeit
{
  public static partial class ProgDatafox
  {
    public static List<ArbeitszeitImportDaten> KonvertDatafoxImport(List<string> DatafoxStringListe, Guid IdAktuellerStandort, string SteuerString = "")
    {
      var liste = new List<ArbeitszeitImportDaten>(DatafoxStringListe.Count);
      var leangeSteuerstring = SteuerString.Length;

      foreach (var dsString in DatafoxStringListe)
      {
        var erg = dsString.Split(new char[] { '\t' }, StringSplitOptions.None);

        var ds = new ArbeitszeitImportDaten()
        {
          Version = erg[1],
          IdStandort = IdAktuellerStandort          
        };

        try
        {
          var kennung = erg[2].ToUpper()[0];
          switch (kennung)
          {
            case 'K': ds.Vorgang = ArbeitszeitImportDaten.EnumVorgang.Komme; break;
            case 'G': ds.Vorgang = ArbeitszeitImportDaten.EnumVorgang.Gehen; break;
            case 'P': ds.Vorgang = ArbeitszeitImportDaten.EnumVorgang.Pause; break;
            default: ds.Vorgang = ArbeitszeitImportDaten.EnumVorgang.Fehler; break;
          }
        }
        catch (Exception f)
        {
          var msg = $"Fehler Konvertierung Vorgang! ({dsString})";
          throw new MyException(msg, f);
        }

        try
        {
          ds.MatchCode = erg[3].Substring(leangeSteuerstring, erg[3].Length - leangeSteuerstring);
        }
        catch (Exception f)
        {
          var msg = $"Fehler Konvertierung Matchcode! ({dsString})";
          throw new MyException(msg, f);
        }

        try
        {
          ds.Datum = Convert.ToDateTime(erg[4]);
        }
        catch (Exception f)
        {
          var msg = $"Fehler Konvertierung Datum! ({dsString})";
          throw new MyException(msg, f);
        }

        try
        {
          ds.GehGrund = Convert.ToInt32(erg[5]);
        }
        catch (Exception f)
        {
          var msg = $"Fehler Konvertierung Gehgrund! ({dsString})";
          throw new MyException(msg, f);
        }

        liste.Add(ds);
      }

      return liste;
    }

    /// <summary>
    /// Programm zu auslesen er Datensätze aus dem Datsfox Terminal
    /// </summary>
    /// <param name="Optionen">Übertragungsoptionen zum Datafox Terminal</param>
    /// <returns>Ausgelesene Datensätze aus dem Terminal</returns>
    public static List<string> ListeAusTerminalAuslesen(OptionenTerminal Optionen)
    {
      string msg;
      byte[] buf = new byte[256];
      int length, errorID = 0, result;
      var listeAntwort = new List<string>();
      var errorString = new StringBuilder(255);

      try
      {
        // Schleife nur um mit break abzubrechen, kein goto verwenden.
        do
        {
          // Lesen der Datensatzbeschreibungen, diese stellen die Tabellendefinitionen dar.
          DFComDLL.TableDeclarations records = new DFComDLL.TableDeclarations(DFComDLL.TableDeclarations.TableType.Record, "Records.xml");
          if (records.LoadFromDevice(Optionen.ChannelId, Optionen.DeviceId, "") == false)
          {
            // Fehlertext ermitteln
            DFComDLL.DFCGetErrorText(Optionen.ChannelId, errorID, 0, errorString, errorString.Capacity);
            msg = string.Format("Lesen der Datensatzbeschreibung ist fehlgeschlagen.\nFehlerbeschreibung: {0}", errorString);
            throw new MyException(msg);
          }

          if (records.Tables == null)
          {
            // Es liegen keine Datensatzbeschreibungen vor.
            msg = string.Format("Es liegen keine Datensatzbeschreibungen vor.\nBitte prüfen Sie das eingespielte Setup.}");
            throw new MyException(msg);
          }

          do
          {
            length = buf.Length;
            // Datensatz lesen
            if ((result = DFComDLL.DFCReadRecord(Optionen.ChannelId, Optionen.DeviceId, buf, out length, out errorID)) < 0)
            {
              msg = string.Format("Datensatz konnte nicht aus Terminal gelesen werden. Fehlercode: {0}", errorID);
              throw new MyException(msg);
            }

            if (result == 0)
            {
              msg = "Es sind keine Anmeldungen im Terminal regiestriert.";
              Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);
              break;
            }

            DFComDLL.DFRecord rs = new DFComDLL.DFRecord(buf, records);
            listeAntwort.Add(rs.TabbedString());

            // Datensatz quittieren
            if (DFComDLL.DFCQuitRecord(Optionen.ChannelId, Optionen.DeviceId, out errorID) < 0)
            {
              msg = string.Format("Datensatz konnte nicht im Terminal quittiert werden. Fehlercode: {0}", errorID);
              throw new MyException(msg);
            }
          } while (true);
        } while (false);
      }
      catch (Exception f)
      { 
        msg = "Fehler beim einlesen der Anmeldedaten aus dem Terminal";
        throw new Exception(msg, f);
      }

      return listeAntwort;
    }
  }
}
