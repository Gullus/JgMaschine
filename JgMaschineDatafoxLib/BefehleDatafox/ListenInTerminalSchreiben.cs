using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineDatafoxLib
{
  public partial class ProgDatafox
  {
    public static void ListenInTerminalSchreiben(OptionenDatafox Optionen)
    {
      string msg;
      var errorString = new StringBuilder(255);
      int errorID = 0;
      int idx, res = 0;
      int importCount = 0;
      var import = new DFComDLL.ListImport();

      try
      {
        // Listenformate aus Terminal auslesen
        var lists = new DFComDLL.TableDeclarations(DFComDLL.TableDeclarations.TableType.List, "Lists.xml");
        if (lists.LoadFromDevice(Optionen.Terminal.ChannelId, Optionen.Terminal.DeviceId, "") == false)
        {
          // Fehlertext ermitteln
          DFComDLL.DFCGetErrorText(Optionen.Terminal.ChannelId, errorID, 0, errorString, errorString.Capacity);
          msg = $"Lesen der Listenbeschreibung ist fehlgeschlagen.\nFehlerbeschreibung: {errorString}";
          Logger.Write(msg, "Service", 1, 0, TraceEventType.Warning);
        }

        if (lists.Tables == null)
        {
          msg = $"Es liegen keine Listendefinitionen im Verzeichnis {Optionen.PfadUpdateBediener} vor.";
          Logger.Write(msg, "Service", 1, 0, TraceEventType.Warning);
          return;
        }

        DFComDLL.DFCClrListenBuffer(Optionen.Terminal.ChannelId);

        // Vorliegende Listendaten importieren und übertragen.
        for (idx = 0; idx < lists.Tables.Length; idx++)
        {
          string fileName = string.Format("{0}*.txt", lists.Tables[idx].Name);
          var files = Directory.GetFiles(Optionen.PfadUpdateBediener, fileName);

          if (files.Length == 0 || files[0].EndsWith("txt") == false)
            break;

          if (files.Length > 1)
          {
            msg = $"Für Liste [{lists.Tables[idx].Name}] liegen mehrere Listendateien vor.";
            Logger.Write(msg, "Service", 1, 0, TraceEventType.Warning);
          }

          try
          {
            // Importieren der Listendaten
            import.Import(lists.Tables[idx], files[0]);
            res = DFComDLL.DFCMakeListe(Optionen.Terminal.ChannelId, idx, import.RecordCount, import.Size, import.Mem, 0);

            if (res == 0)
            {
              msg = $"Übergabe der Listendaten aus der Datei [{files[0]}] ist fehlgeschlagen.";
              Logger.Write(msg, "Service", 1, 0, TraceEventType.Warning);
              continue;
            }

            msg = $"Liste [{lists.Tables[idx].Name} (Datensätze: {lists.Tables[idx].Name})] wurde importiert. Datei: {files[0]}";
            Logger.Write(msg, "Service", 1, 0, TraceEventType.Verbose);
          }
          catch (DFComDLL.ListImportException ex)
          {
            msg = $"Import von Liste [{lists.Tables[idx].Name}] schlug fehl. Datei: {files[0]}\nGrund: {ex.Message}";
            Logger.Write(msg, "Service", 1, 0, TraceEventType.Warning);
            continue;
          }

          importCount++;
        }

        if (importCount == 0)
        {
          msg = "Es liegen keine Listendaten vor.";
          Logger.Write(msg, "Service", 0, 0, TraceEventType.Verbose);
          return;
        }

        res = DFComDLL.DFCLoadListen(Optionen.Terminal.ChannelId, Optionen.Terminal.DeviceId, out errorID);
        if (res == 0)
        {
          // Fehlertext ermitteln
          DFComDLL.DFCGetErrorText(Optionen.Terminal.ChannelId, errorID, 0, errorString, errorString.Capacity);
          msg = $"Übertragung der Listendaten ist fehlgeschlagen.\nGrund: {errorString}";
          Logger.Write(msg, "Service", 1, 0, TraceEventType.Warning);
        }

        msg = string.Format("Es wurde{0} {1} von {2} Listen übertragen.", (importCount == 1) ? "" : "n", importCount, lists.Tables.Length);
        Logger.Write(msg, "Service", 1, 0, TraceEventType.Information);

      }
      catch (Exception f)
      {
        msg = $"Fehler beim eintragen von Daten in das Terminal !\nGrund: {f.Message}";
        Logger.Write(msg, "Service", 1, 0, TraceEventType.Warning);
      }
    }
  }
}

