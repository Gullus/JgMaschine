using System;
using System.Collections.Generic;
using System.Text;
using JgMaschineLib;

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
          var msg = $"Fehler Konvertierung Vorgang!\nGrund: {f.Message}\nDs: {dsString}";
          throw new Exception(msg);
        }

        try
        {
          ds.MatchCode = erg[3].Substring(leangeSteuerstring, erg[3].Length - leangeSteuerstring);
        }
        catch (Exception f)
        {
          var msg = $"Fehler Konvertierung Matchcode!\nGrund: {f.Message}\nDs: {dsString}";
          throw new Exception(msg);
        }

        try
        {
          ds.Datum = Convert.ToDateTime(erg[4]);
        }
        catch (Exception f)
        {
          var msg = $"Fehler Konvertierung Datum!\nGrund: {f.Message}\nDs: {dsString}";
          throw new Exception(msg);
        }

        try
        {
          ds.GehGrund = Convert.ToInt32(erg[5]);
        }
        catch (Exception f)
        {
          var msg = $"Fehler Konvertierung Gehgrund!\nGrund: {f.Message}\nDs: {dsString}";
          throw new Exception(msg);
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
    public static List<string> ListeAusTerminalAuslesen(ZeitsteuerungDatafox Optionen)
    {
      string msg;
      byte[] buf = new byte[256];
      int length, errorID = 0, result;
      var listeAntwort = new List<string>();
      var errorString = new StringBuilder(255);

      // Schleife nur um mit break abzubrechen, kein goto verwenden.
      do
      {
        // Lesen der Datensatzbeschreibungen, diese stellen die Tabellendefinitionen dar.
        DFComDLL.TableDeclarations records = new DFComDLL.TableDeclarations(DFComDLL.TableDeclarations.TableType.Record, "Records.xml");
        if (records.LoadFromDevice(Optionen.Datafox.ChannelId, Optionen.Datafox.DeviceId, "") == false)
        {
          // Fehlertext ermitteln
          DFComDLL.DFCGetErrorText(Optionen.Datafox.ChannelId, errorID, 0, errorString, errorString.Capacity);
          msg = string.Format("Lesen der Datensatzbeschreibung ist fehlgeschlagen.\n\nZurückgelieferte Fehlerbeschreibung:\n{0}", errorString);
          throw new Exception(msg);
        }
        if (records.Tables == null)
        {
          // Es liegen keine Datensatzbeschreibungen vor.
          msg = string.Format("Es liegen keine Datensatzbeschreibungen vor.\n\nBitte prüfen Sie das eingespielte Setup.}");
          throw new Exception(msg);
        }

        do
        {
          length = buf.Length;
          // Datensatz lesen
          if ((result = DFComDLL.DFCReadRecord(Optionen.Datafox.ChannelId, Optionen.Datafox.DeviceId, buf, out length, out errorID)) < 0)
          {
            msg = string.Format("Datensatz konnte nicht gelesen werden. Fehlercode: {0}", errorID);
            throw new Exception(msg);
          }

          if (result == 0)
          {
            //msg = string.Format("Es liegt kein Datensatz vor");
            //Helper.Protokoll(msg);
            break;
          }

          DFComDLL.DFRecord rs = new DFComDLL.DFRecord(buf, records);
          listeAntwort.Add(rs.TabbedString());

          // Datensatz quittieren
          if (DFComDLL.DFCQuitRecord(Optionen.Datafox.ChannelId, Optionen.Datafox.DeviceId, out errorID) < 0)
          {
            msg = string.Format("Datensatz konnte nicht quittiert werden. Fehlercode: {0}", errorID);
            throw new Exception(msg);
          }
        } while (true);
      } while (false);

      return listeAntwort;
    }
  }
}
