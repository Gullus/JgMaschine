using System;
using System.IO;
using System.Text;
using JgMaschineLib;

namespace JgMaschineDatafoxLib
{
  public partial class ProgDatafox
  {
    public static void ListenInTerminalSchreiben(ZeitsteuerungDatafox Optionen)
    {
      string msg;
      var errorString = new StringBuilder(255);
      int errorID = 0;
      int idx, res = 0;
      int importCount = 0;
      var import = new DFComDLL.ListImport();

      var lists = new DFComDLL.TableDeclarations(DFComDLL.TableDeclarations.TableType.List, "Lists.xml");
      if (lists.LoadFromDevice(Optionen.Datafox.ChannelId, Optionen.Datafox.DeviceId, "") == false)
      {
        // Fehlertext ermitteln
        DFComDLL.DFCGetErrorText(Optionen.Datafox.ChannelId, errorID, 0, errorString, errorString.Capacity);
        msg = string.Format("Lesen der Listenbeschreibung ist fehlgeschlagen.\n\nZurückgelieferte Fehlerbeschreibung:\n{0}", errorString);
        throw new Exception(msg);
      }

      if (lists.Tables == null)
      {
        msg = $"Es liegen keine Listendefinitionen im Verzeichnis {Optionen.PfadUpdateBediener} vor.";
        Optionen.Protokoll.Set(msg, Proto.ProtoArt.Kommentar);
        return;
      }

      DFComDLL.DFCClrListenBuffer(Optionen.Datafox.ChannelId);

      // Vorliegende Listendaten importieren und übertragen.
      for (idx = 0; idx < lists.Tables.Length; idx++)
      {
        string fileName = string.Format("{0}*.txt", lists.Tables[idx].Name);
        var files = Directory.GetFiles(Optionen.PfadUpdateBediener, fileName);

        if (files.Length == 0 || files[0].EndsWith("txt") == false)
          break;

        if (files.Length > 1)
        {
          msg = string.Format("Für Liste [{0}] liegen mehrere Listendateien vor.", lists.Tables[idx].Name);
          Optionen.Protokoll.Set(msg, Proto.ProtoArt.Warnung);
        }

        try
        {
          // Importieren der Listendaten
          import.Import(lists.Tables[idx], files[0]);
          res = DFComDLL.DFCMakeListe(Optionen.Datafox.ChannelId, idx, import.RecordCount, import.Size, import.Mem, 0);

          if (res == 0)
          {
            msg = string.Format("Übergabe der Listendaten aus der Datei [{0}] ist fehlgeschlagen.", files[0]);
            Optionen.Protokoll.Set(msg, Proto.ProtoArt.Warnung);
            continue;
          }

          msg = string.Format("Liste [{0} (Datensätze: {1})] wurde importiert. Datei: {2}", lists.Tables[idx].Name, import.RecordCount, files[0]);
          Optionen.Protokoll.Set(msg, Proto.ProtoArt.Kommentar);
        }
        catch (DFComDLL.ListImportException ex)
        {
          msg = string.Format("Import von Liste [{0}] schlug fehl. Datei: {1}\nFehler: {2}", lists.Tables[idx].Name, files[0], ex.Message);
          Optionen.Protokoll.Set(msg, Proto.ProtoArt.Warnung);
          continue;
        }

        importCount++;
      }

      if (importCount == 0)
      {
        msg = "Es liegen keine Listendaten vor.";
        Optionen.Protokoll.Set(msg, Proto.ProtoArt.Kommentar);
        return;
      }

      res = DFComDLL.DFCLoadListen(Optionen.Datafox.ChannelId, Optionen.Datafox.DeviceId, out errorID);
      if (res == 0)
      {
        // Fehlertext ermitteln
        DFComDLL.DFCGetErrorText(Optionen.Datafox.ChannelId, errorID, 0, errorString, errorString.Capacity);
        msg = string.Format("Übertragung der Listendaten ist fehlgeschlagen.\n\nZurückgelieferte Fehlerbeschreibung:\n{0}", errorString);
        throw new Exception(msg);
      }
      msg = string.Format("Es wurde{0} {1} von {2} Listen übertragen.", (importCount == 1) ? "" : "n", importCount, lists.Tables.Length);
      Optionen.Protokoll.Set(msg, Proto.ProtoArt.Kommentar);
    }
  }
}

