using System.IO;
using System.Text;
using JgMaschineLib;

namespace JgMaschineDatafoxLib
{
  public partial class ProgDatafox
  {
    public static void ListenInTerminalSchreiben(DatafoxOptionen Optionen, string DateiPfad)
    {
      string msg;
      var errorString = new StringBuilder(255);
      int errorID = 0;
      int idx, res = 0;
      int importCount = 0;
      var import = new DFComDLL.ListImport();

      byte idVerbindung = 3; // <= Verbindung über TcpIp

      if (DFComDLL.DFCComOpenIV(Optionen.ChannelId, 0, idVerbindung, Optionen.IpNummer, Optionen.Portnummer, Optionen.TimeOut) == 0)
      {
        msg = string.Format("Schnittstelle oder Verbindung zum Gerät konnte nicht geöffnet werden.\n\nBitte überprüfen Sie die Einstellungen der Kommunikation und Erreichbarkeit des Terminals.");
        Helper.Protokoll(msg);
        return;
      }

      var lists = new DFComDLL.TableDeclarations(DFComDLL.TableDeclarations.TableType.List, "Lists.xml");
      if (lists.LoadFromDevice(Optionen.ChannelId, Optionen.DeviceId, "") == false)
      {
        // Fehlertext ermitteln
        DFComDLL.DFCGetErrorText(Optionen.ChannelId, errorID, 0, errorString, errorString.Capacity);
        // Nachricht anzeigen
        msg = string.Format("Lesen der Listenbeschreibung ist fehlgeschlagen.\n\nZurückgelieferte Fehlerbeschreibung:\n{0}", errorString);
        Helper.Protokoll(msg);
        return;
      }

      if (lists.Tables == null)
      {
        msg = $"Es liegen keine Listendefinitionen im Verzeichnis {DateiPfad} vor.";
        Helper.Protokoll(msg, Helper.ProtokollArt.Info);
        return;
      }

      DFComDLL.DFCClrListenBuffer(Optionen.ChannelId);

      // Vorliegende Listendaten importieren und übertragen.
      for (idx = 0; idx < lists.Tables.Length; idx++)
      {
        string fileName = string.Format("{0}*.txt", lists.Tables[idx].Name);
        var files = Directory.GetFiles(DateiPfad, fileName);

        if (files.Length == 0 || files[0].EndsWith("txt") == false)
          break;

        if (files.Length > 1)
        {
          msg = string.Format("Für Liste [{0}] liegen mehrere Listendateien vor.", lists.Tables[idx].Name);
          Helper.Protokoll(msg);
          break;
        }

        try
        {
          // Importieren der Listendaten
          import.Import(lists.Tables[idx], files[0]);
          res = DFComDLL.DFCMakeListe(Optionen.ChannelId, idx, import.RecordCount, import.Size, import.Mem, 0);

          if (res == 0)
          {
            msg = string.Format("Übergabe der Listendaten aus der Datei [{0}] ist fehlgeschlagen.", files[0]);
            Helper.Protokoll(msg);
            continue;
          }

          msg = string.Format("Liste [{0} (Datensätze: {1})] wurde importiert. Datei: {2}", lists.Tables[idx].Name, import.RecordCount, files[0]);
          Helper.Protokoll(msg, Helper.ProtokollArt.Info);
        }
        catch (DFComDLL.ListImportException ex)
        {
          msg = string.Format("Import von Liste [{0}] schlug fehl. Datei: {1}", lists.Tables[idx].Name, files[0]);
          Helper.Protokoll(msg);
          continue;
        }

        importCount++;
      }

      if (importCount == 0)
      {
        msg = "Es liegen keine Listendaten vor.";
        Helper.Protokoll(msg, Helper.ProtokollArt.Info);
        return;
      }

      res = DFComDLL.DFCLoadListen(Optionen.ChannelId, Optionen.DeviceId, out errorID);
      if (res == 0)
      {
        // Fehlertext ermitteln
        DFComDLL.DFCGetErrorText(Optionen.ChannelId, errorID, 0, errorString, errorString.Capacity);
        msg = string.Format("Übertragung der Listendaten ist fehlgeschlagen.\n\nZurückgelieferte Fehlerbeschreibung:\n{0}", errorString);
        Helper.Protokoll(msg);
      }
      msg = string.Format("Es wurde{0} {1} von {2} Listen übertragen.", (importCount == 1) ? "" : "n", importCount, lists.Tables.Length);
      Helper.Protokoll(msg, Helper.ProtokollArt.Info);

      // Verbindung schliessen
      DFComDLL.DFCComClose(Optionen.ChannelId);
    }
  }
}

