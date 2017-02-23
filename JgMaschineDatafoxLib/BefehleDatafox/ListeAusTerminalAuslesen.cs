using System;
using System.Collections.Generic;
using System.Text;
using JgMaschineLib;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineDatafoxLib
{
  public static partial class ProgDatafox
  {
    public static List<DatafoxDsExport> KonvertDatafoxExport(List<string> DatafoxStringListe, string SteuerString = "")
    {
      var liste = new List<DatafoxDsExport>(DatafoxStringListe.Count);
      var leangeSteuerstring = SteuerString.Length;

      foreach (var dsString in DatafoxStringListe)
      {
        var erg = dsString.Split(new char[] { '\t' }, StringSplitOptions.None);

        var ds = new DatafoxDsExport()
        {
          Version = erg[1]
        };

        try
        {
          var kennung = erg[2].ToUpper()[0];
          switch (kennung)
          {
            case 'K': ds.Vorgang = DatafoxDsExport.EnumVorgang.Komme; break;
            case 'G': ds.Vorgang = DatafoxDsExport.EnumVorgang.Gehen; break;
            case 'P': ds.Vorgang = DatafoxDsExport.EnumVorgang.Pause; break;
            default: ds.Vorgang = DatafoxDsExport.EnumVorgang.Fehler; break;
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
    public static List<string> ListeAusTerminalAuslesen(OptionenDatafox Optionen)
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
          if (records.LoadFromDevice(Optionen.Datafox.ChannelId, Optionen.Datafox.DeviceId, "") == false)
          {
            // Fehlertext ermitteln
            DFComDLL.DFCGetErrorText(Optionen.Datafox.ChannelId, errorID, 0, errorString, errorString.Capacity);
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
            if ((result = DFComDLL.DFCReadRecord(Optionen.Datafox.ChannelId, Optionen.Datafox.DeviceId, buf, out length, out errorID)) < 0)
            {
              msg = string.Format("Datensatz konnte nicht aus Terminal gelesen werden. Fehlercode: {0}", errorID);
              throw new MyException(msg);
            }

            if (result == 0)
            {
              msg = "Es liegen keine Datensätze vor";
              Logger.Write(msg, "Service", 0, 0, System.Diagnostics.TraceEventType.Verbose);
              break;
            }

            DFComDLL.DFRecord rs = new DFComDLL.DFRecord(buf, records);
            listeAntwort.Add(rs.TabbedString());

            // Datensatz quittieren
            if (DFComDLL.DFCQuitRecord(Optionen.Datafox.ChannelId, Optionen.Datafox.DeviceId, out errorID) < 0)
            {
              msg = string.Format("Datensatz konnte nicht im Terminal quittiert werden. Fehlercode: {0}", errorID);
              throw new MyException(msg);
            }
          } while (true);
        } while (false);

      }
      catch (Exception f)
      { 
        msg = "Fehler beim einlesen der Zeiten aus dem Terminal";
        throw new Exception(msg, f);
      }

      return listeAntwort;
    }
  }
}
