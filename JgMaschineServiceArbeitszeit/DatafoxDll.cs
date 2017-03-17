using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace JgMaschineServiceArbeitszeit
{
  public delegate int CBFunction();
  public static class DFComDLL
    {
    [DllImport("DFComDLL.dll")]
    public static extern int DFCComOpenIV(int channelID, int deviceID, int commType, string commString, int commValue, int commTimeout);

    [DllImport("DFComDLL.dll")]
    public static extern void DFCComClose(int channelID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCCheckAE(int channelID, int deviceID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCCheckDevice(int channelID, int deviceID, out int errorID, int iDevicePollRetry);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCComSetTime(int channelID, int deviceID, byte[] pucDateTime);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCComGetTime(int channelID, int deviceID, [MarshalAs(UnmanagedType.LPArray)] byte[] pucDateTime);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCComSendMessage(int channelID, int deviceID, byte ucDelayTime, byte ucAction, byte ucAudio, string szMessage, int iLength);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCComSendInfotext(int channelID, int deviceID, string szMessage, int iLength);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetSeriennummer(int channelID, int deviceID, out int errorID, out int serial);

    [DllImport("DFComDLL.dll")]
    public static extern void DFCSetLogOn(int channelID);

    [DllImport("DFComDLL.dll")]
    public static extern void DFCSetLogOff(int channelID);

    [DllImport("DFComDLL.dll")]
    public static extern void DFCSetCallBack(int channelID, CBFunction func);

    [DllImport("DFComDLL.dll")]
    public static extern void DFCSetLogFileName(int channelID, string szLogFileName);

    [DllImport("DFComDLL.dll")]
    public static extern void DFCGetErrorText(int channelID, int errorID, int iLanguage, StringBuilder szText, int iLength);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCSetGlobVar(int channelID, int deviceID, string gvName, int typeGV, string valueGV, out int errorID);
    [DllImport("DFComDLL.dll")]
    public static extern int DFCSetGlobVar(int channelID, int deviceID, [MarshalAs(UnmanagedType.LPArray)] byte[] gvIndex, int typeGV, string valueGV, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetGlobVar(int channelID, int deviceID, string gvName, int typeGV, StringBuilder valueGV, int valueLength, out int errorID);
    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetGlobVar(int channelID, int deviceID, [MarshalAs(UnmanagedType.LPArray)] byte[] gvIndex, int typeGV, StringBuilder valueGV, int valueLength, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCCloseRelay(int channelID, int deviceID, int iNum, int iTime, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetRelayState(int channelID, int deviceID, int iNum, out int piState, out int piTime, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCOpenRelay(int channelID, int deviceID, int iNum, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetDevicePollRetry(int channelID);

    [DllImport("DFComDLL.dll")]
    public static extern IntPtr DFCGetComPort(int channelID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCSetComPort(int channelID, IntPtr hComPort, int bPortIsSocket, int iBaudRate, int iTimeOut);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCWrite(int channelID, string Buf, int lengthBuf, out int lengthWrite, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCRead(int channelID, StringBuilder Buf, int lengthBuf, out int lengthRead, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCUpload(int channelID, int deviceID, string szFileName, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetVersionFirmware(int channelID, int deviceID, StringBuilder szVersion, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetVersionFirmwareFromFile(int channelID, string fileName, StringBuilder szVersion, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetInfo(int channelID, int deviceID, string name, string parameters, StringBuilder info, out int len, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCOpenComServerMode(int channelID, int deviceID, string comm, int baudRate, int byteSize, int parity, int stopBits, int flags, int timeoutToClose, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCCloseComServerMode(int channelID, int deviceID, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCIsChannelOpen(int channelID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCSetOptionFirmware(int channelID, int deviceID, int mask, int options, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetOptionFirmware(int channelID, int deviceID, out int mask, out int options, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCReset(int channelID, int deviceID, int mode, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCSetFontType(int channelID, int deviceID, int iType, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCSetPassword(int channelID, string password, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetPasswordKey(int channelID, int deviceID, StringBuilder key, out int len, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCPressVirtualKey(int channelID, int deviceID, int virtualKey, int flags, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetFlashStatus(int channelID, int deviceID, out int status, out int errorID);

    // ########################################################
    // ############## FUNKTIONEN FUER SETUP ###################
    // ########################################################

    [DllImport("DFComDLL.dll")]
    public static extern int DFCSetupLaden(int channelID, int deviceID, string fileName, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCDownload(int channelID, int deviceID, string fileName, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCModifyStudioFile(string sourceFile, string destFile, string changeData, int flags, out int errorID);


    // ###############################################################
    // ######### FUNKTIONEN FÜR LISTEN, SOWIE ZUTRITTSLISTEN #########
    // ###############################################################

    [DllImport("DFComDLL.dll")]
    public static extern int DFCOpenTable(int channelID, int deviceID, string name, int options, out int handle, string reserved, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCCloseTable(int channelID, int deviceID, int handle, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCSetFilter(int channelID, int deviceID, int handle, string filter, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetFilter(int channelID, int deviceID, int handle, StringBuilder filter, out int len, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCClearFilter(int channelID, int deviceID, int handle, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCSkip(int channelID, int deviceID, int handle, int offset, int origin, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCSetField(int channelID, int deviceID, int handle, string name, string value, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetField(int channelID, int deviceID, int handle, string name, StringBuilder value, out int len, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCComClearData(int channelID, int deviceID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCLoadDatensatzbeschreibung(int channelID, int deviceID, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCDatBCnt(int channelID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCDatBDatensatz(int channelID, int iDataNum, StringBuilder szName, out int piFieldCount);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCDatBFeld(int channelID, int iDataNum, int iField, StringBuilder szName, out int piType, out int piLength);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCReadRecord(int channelID, int deviceID, [MarshalAs(UnmanagedType.LPArray)] byte[] buf, out int length, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCQuitRecord(int channelID, int deviceID, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCRestoreRecords(int channelID, int deviceID, out int errorID);


    // ########################################################
    // ############## FUNKTIONEN FUER LISTEN ##################
    // ########################################################

    [DllImport("DFComDLL.dll")]
    public static extern int DFCMakeListe(int channelID, int iListNum, int iLineCount, int iListSize, byte[] pucBuf, int iListMemSizeNum);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCLoadListen(int channelID, int deviceID, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern void DFCClrListenBuffer(int channelID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCLoadListenbeschreibung(int channelID, int deviceID, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCListBCnt(int channelID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCListBDatensatz(int channelID, int iDataNum, StringBuilder szName, out int piFieldCount, out int piVar);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCListBFeld(int channelID, int iDataNum, int iField, StringBuilder szName, out int piType, out int piLength);


    // ########################################################
    // ####### FUNKTIONEN FUER ZUTRITTSKONTROLLLISTEN 2 #######
    // ########################################################

    [DllImport("DFComDLL.dll")]
    public static extern int DFCMakeEntrance2List(int channelID, int iListNum, int iLineCount, int iListSize, byte[] pucBuf, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCLoadEntrance2List(int channelID, int deviceID, int iListNum, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern void DFCClearEntrance2ListBuffer(int channelID, int iListNum);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCEntrance2Identification(int channelID, int deviceID, string TM, string Id, string Pin, StringBuilder status, out int len, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCEntrance2OnlineAction(int channelID, int deviceID, string TM, int mask, int type, int duration, out int errorID);


    // ########################################################
    // ########### FUNKTIONEN FUER FINGERPRINT ################
    // ########################################################


    [DllImport("DFComDLL.dll")]
    public static extern int DFCFingerprintAppendRecord(int channelID, int deviceID, int type, byte[] fingertemplate, int size, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCFingerprintGetRecord(int channelID, int deviceID, int type, int pid, int fid, [MarshalAs(UnmanagedType.LPArray)] byte[] fingertemplate, out int size, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCFingerprintDeleteRecord(int channelID, int deviceID, int pid, int fid, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCFingerprintList(int channelID, int deviceID, int flags, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCFingerprintBackup(int channelID, int deviceID, string backupFile, int flags, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCFingerprintRestore(int channelID, int deviceID, string backupFile, int flags, out int errorID);


    // ########################################################
    // ########### FUNKTIONEN FUER TIMEBOYLISTEN  #############
    // ########################################################


    [DllImport("DFComDLL.dll")]
    public static extern int DFCMakeTimeboyList(int channelID, int iGroupID, int iListNum, int iLineCount, int iListSize, byte[] pucBuf, int flags, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCLoadTimeboyList(int channelID, int deviceID, int iGroupID, int iListNum, int reserve, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern void DFCClearTimeboyListBuffer(int channelID, int iGroupID, int iListNum);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCStartActiveConnection(string ip, int port, int deviceID, int timeout, int aliveTO, int maxConnections, int infoFlags, int reserved, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCStopActiveConnection(out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetFirstActiveChannelID();

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetNextActiveChannelID(int prev);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCGetInfoActiveChannel(int channelID, StringBuilder infoString, out int infoStringLength);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCSetRecordAvailable(int enable);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCRecordAvailable(out int channelID, out int deviceID, StringBuilder infoString, out int infoStringLength, out int errorID);

    [DllImport("DFComDLL.dll")]
    public static extern int DFCBindDeviceToChannel(int channelID, int deviceType, int deviceSerial, string deviceIp, out int errorID);

    /// <summary>
    /// Definition einer Tabelle (Datensatz- oder Listenbeschreibung)
    /// </summary>
    public class DFTable
    {
      public DFTable()
      {
        // Gesamtgröße des Datensatzes
        _length = -1;
      }
      string _name;
      public string Name
      {
        get { return _name; }
        set { _name = value; }
      }
      DFTableField[] _fields;
      public DFTableField[] Fields
      {
        get { return _fields; }
        set { _fields = value; }
      }
      int _length;
      public int Length
      {
        get
        {
          // Gesamtgroesse bereits ermittelt?
          if (_length == -1 && Fields != null)
          {
            _length = 0;
            int num;
            for (num = 0; num < Fields.Length; num++)
            {
              _length += Fields[num].Length;
            }
          }

          return _length;
        }
      }
    }
    /// <summary>
    /// Definition eines Tabellenfeldes (Datensatz- oder Listenbeschreibung)
    /// </summary>
    public class DFTableField
    {
      public DFTableField()
      {

      }
      string _name;
      public string Name
      {
        get { return _name; }
        set { _name = value; }
      }
      int _type;
      public int Type
      {
        get { return _type; }
        set { _type = value; }
      }
      int _length;
      public int Length
      {
        get { return _length; }
        set { _length = value; }
      }
    }
    /// <summary>
    /// Laden aller Tabellenbeschreibungen. Stellt diese über die Tables Auflistung zur Verfügung.
    /// </summary>
    public class TableDeclarations
    {
      // Handelt es sich um Tabellendefinitionen für Datensätze, Listen der Bedienung oder Zutrittskontrolle
      public enum TableType { Record, List, ListZK2 };
      TableType _type;
      string _fileNameSerialize;
      public TableDeclarations(TableType type, string fileNameSerialize)
      {
        _type = type;
        _fileNameSerialize = fileNameSerialize;
      }
      int _channelID;
      int _deviceID;
      int _errorID;
      public int ErrorID
      {
        get { return _errorID; }
        set { _errorID = value; }
      }
      string _filePath;
      int _baseIndex;
      DFTable[] _tables;
      //TableDeclarationsSerializer _serializer;
      public DFTable[] Tables
      {
        get { return _tables; }
      }

      public bool LoadFromDevice(int channelID, int deviceID, string filePath)
      {
        int idx, count;
        _channelID = channelID;
        _deviceID = deviceID;
        _filePath = filePath;
        _baseIndex = 0;
        _tables = null;
        ErrorID = 0;

        if (_type == TableType.Record)
        {
          // Laden der Listenbeschreibungen
          if (DFComDLL.DFCLoadDatensatzbeschreibung(_channelID, _deviceID, out _errorID) == 0)
          {
            return false;
          }

          count = DFComDLL.DFCDatBCnt(_channelID);
          if (count == 0)
          {
            return true;
          }

        }
        else if (_type == TableType.List)
        {
          // Listenbeschreibungen des Setups müssen geladen werden, sind nicht ZK2-Listen

          // Laden der Listenbeschreibungen
          if (DFComDLL.DFCLoadListenbeschreibung(_channelID, _deviceID, out _errorID) == 0)
          {
            return false;
          }

          count = DFComDLL.DFCListBCnt(_channelID);
          if (count == 0)
          {
            return true;
          }

        }
        else if (_type == TableType.ListZK2)
        {
          count = 7;

          // Basisindex setzen
          _baseIndex = 200;
        }
        else
        {
          // Listentype liegt außerhalb des unterstützten Bereichs.
          throw new Exception("Tabellentyp liegt außerhalb des unterstützten Bereiches.");
        }

        // Listenbeschreibungen aus DLL importieren
        _tables = new DFTable[count];
        for (idx = 0; idx < Tables.Length; idx++)
        {
          Tables[idx] = new DFTable();
        }
        int idxTable, idxField, fieldCount = 0, type = 0, var;
        StringBuilder name = new StringBuilder(20);
        int length = 0;
        for (idxTable = 0; idxTable < count; idxTable++)
        {
          // Listendefinition lesen
          if (((_type == TableType.List || _type == TableType.ListZK2) && DFComDLL.DFCListBDatensatz(_channelID, _baseIndex + idxTable, name, out fieldCount, out var) == 0) ||
              (_type == TableType.Record && DFComDLL.DFCDatBDatensatz(_channelID, _baseIndex + idxTable, name, out fieldCount) == 0))
          {
            throw new Exception("Funktion zur Verarbeitung der Tabellenbeschreibungen (DFCListBDatensatz, DFCDatBDatensatz) schlug fehl.");
          }

          Tables[idxTable].Name = name.ToString();
          Tables[idxTable].Fields = new DFTableField[fieldCount];
          for (idx = 0; idx < Tables[idxTable].Fields.Length; idx++)
          {
            Tables[idxTable].Fields[idx] = new DFTableField();
          }
          for (idxField = 0; idxField < fieldCount; idxField++)
          {
            if (((_type == TableType.List || _type == TableType.ListZK2) && DFComDLL.DFCListBFeld(_channelID, _baseIndex + idxTable, idxField, name, out type, out length) == 0) ||
                (_type == TableType.Record && DFComDLL.DFCDatBFeld(_channelID, _baseIndex + idxTable, idxField, name, out type, out length) == 0))
            {
              throw new Exception("Funktion zur Verarbeitung der Tabellenbeschreibungen (DFCListBFeld, DFCDatBFeld) schlug fehl.");
            }
            Tables[idxTable].Fields[idxField].Name = name.ToString();
            Tables[idxTable].Fields[idxField].Type = type;
            Tables[idxTable].Fields[idxField].Length = length;
          }
        }

        return true;
      }
    }
    /// <summary>
    /// Aktuelle Tabellenbeschreibungen in Datei Serialisieren.
    /// </summary>
    public class TableDeclarationsSerializer : XmlSerializer
    {
      public TableDeclarationsSerializer(string fileName)
          : base(typeof(DFTable[]))
      {
        _fileName = fileName;
      }
      string _fileName;
      public void Serialize(DFTable[] data)
      {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        XmlWriter writer = XmlWriter.Create(_fileName, settings);
        base.Serialize(writer, data);
        writer.Close();
      }

      public DFTable[] Deserialize()
      {
        DFTable[] _data;
        try
        {
          XmlReader reader = XmlReader.Create(_fileName);
          _data = (DFTable[])base.Deserialize(reader);
          reader.Close();
        }
        //catch (System.Exception ex)
        catch
        {
          _data = new DFTable[0];
        }

        return _data;
      }
    }
    /// <summary>
    /// Ausnahme beim Import von Listendaten.
    /// </summary>
    public class ListImportException : Exception
    {
      public ListImportException(string fileName, int line, int column, string msg)
          : base(string.Format("Fehler bei Import der Daten aus Datei [{0}]\r\nIn Zeile {1}{2}: {3}", fileName, line, (column != 0) ? string.Format(", Spalte {0}", column) : "", msg))
      {
        this.Line = line;
        this.Column = column;
      }
      string _fileName;
      public string FileName
      {
        get { return _fileName; }
        set { _fileName = value; }
      }
      int _line;
      public int Line
      {
        get { return _line; }
        set { _line = value; }
      }
      int _column;
      public int Column
      {
        get { return _column; }
        set { _column = value; }
      }
    }
    /// <summary>
    /// Import einer Textdatei als Listendaten.
    /// </summary>
    public class ListImport
    {
      public ListImport()
      {
        // Member initialisieren
        _recordCount = 0;
        _recordSize = 0;
      }

      /// <summary>
      /// Speicherarray der importierten Listendaten.
      /// </summary>
      byte[] _mem;
      public byte[] Mem
      {
        get { return _mem; }
      }
      /// <summary>
      /// Anzahl der importierten Datensätze.
      /// </summary>
      int _recordCount;
      public int RecordCount
      {
        get { return _recordCount; }
      }
      /// <summary>
      /// Länge eines importierten Datensatzes.
      /// </summary>
      int _recordSize;
      public int RecordSize
      {
        get { return _recordSize; }
      }
      /// <summary>
      /// Gesamtlänge der aktuell importierten Listendaten.
      /// </summary>
      public int Size
      {
        get { return RecordCount * RecordSize; }
      }

      public bool Import(DFTable table, string fileName)
      {
        int lineNum = 0, columnNum, idx;
        string textLine;
        string[] tokens;
        Encoding ascii = Encoding.ASCII;
        StreamReader reader = new StreamReader(fileName);
        MemoryStream list = new MemoryStream();

        _recordCount = 0;
        _recordSize = 0;

        // Datensatzgröße berechnen
        for (idx = 0; idx < table.Fields.Length; idx++)
        {
          _recordSize += table.Fields[idx].Length;
        }
        // Zeilenweise die Textdatei abarbeiten
        while ((textLine = reader.ReadLine()) != null)
        {
          // Zähler der Gesamtzeilen erhöhen.
          lineNum++;

          // Kommentar- und Leerzeilen übergehen
          if (textLine.Length == 0 || textLine[0] == ';')
          {
            continue;
          }
          // Zeilendaten zerlegen
          tokens = textLine.Split('\t');
          // Anzahl Felder muss mit Definition übereinstimmen
          if (tokens.Length != table.Fields.Length)
          {
            reader.Close();
            throw new ListImportException(fileName, lineNum, 0, "Falsche Feldanzahl."); ;
          }
          byte[] buf;
          for (columnNum = 0; columnNum < tokens.Length; columnNum++)
          {
            // Spaltenbreite muss mit Definition übereinstimmen
            if (tokens[columnNum].Length >= table.Fields[columnNum].Length)
            {
              reader.Close();
              throw new ListImportException(fileName, lineNum, columnNum + 1, "Feldbreite überschritten."); ;
            }
            buf = ascii.GetBytes(tokens[columnNum]);
            for (idx = 0; idx < buf.Length; idx++)
            {
              // Feldinhalt prüfen
              if (table.Fields[columnNum].Type == 3 && char.IsDigit(Convert.ToChar(buf[idx])) == false)
              {
                reader.Close();
                throw new ListImportException(fileName, lineNum, columnNum + 1, "Feldwert fehlerhaft (Formatfehler)."); ;
              }
              list.WriteByte(buf[idx]);
            }
            for (; idx < table.Fields[columnNum].Length; idx++)
            {
              list.WriteByte(0);
            }
          }
          _recordCount++;
        }

        if (list.Length > 0)
        {
          _mem = list.GetBuffer();
        }
        else
        {
          _recordCount = 0;
          _recordSize = 0;
          _mem = null;
        }

        reader.Close();
        return true;
      }
    }
    public class DFRecord
    {
      string[] _values;
      byte[] _val = new byte[40];

      public DFRecord(byte[] data, TableDeclarations decl)
      {
        // Der Datensatz gehört zur Datensatzbeschreibung 0 < 19 und ist 1 bis 242 Byte lang?
        if (data[0] < 20 || data.Length < 243)
        {
        }

        int idx, pos = 1;
        string val = "";
        DateTime dt;
        _values = new string[decl.Tables[data[0]].Fields.Length];

        // Für jedes Feld den Wert aus dem Bytearray extrahieren
        for (idx = 0; idx < decl.Tables[data[0]].Fields.Length; idx++)
        {
          switch (decl.Tables[data[0]].Fields[idx].Type)
          {
            case 2: // DatumUhrzeit
              dt = new DateTime(data[pos] * 100 + data[pos + 1], data[pos + 2], data[pos + 3], data[pos + 4], data[pos + 5], data[pos + 6]);
              val = dt.ToString();
              break;
            case 3: // ASCII nur Ziffern
              val = ExtractASCII(data, pos, decl.Tables[data[0]].Fields[idx].Length);
              break;
            case 4: // ASCII
              val = ExtractASCII(data, pos, decl.Tables[data[0]].Fields[idx].Length);
              break;
              //default:
              //  MessageBox.Show("Unbekanntes Feldformat");
          }

          _values[idx] = val;
          pos += decl.Tables[data[0]].Fields[idx].Length;
        }
      }

      private string ExtractASCII(byte[] data, int offs, int length)
      {
        StringBuilder sb = new StringBuilder(length);
        int idx;
        for (idx = 0; idx < length; idx++)
        {
          if (data[offs + idx] != 0x09 && data[offs + idx] != 0x0A && data[offs + idx] != 0x0D &&
              !(data[offs + idx] >= 0x20 && data[offs + idx] <= 0x7F))
          {
            break;
          }

          sb.Append(ASCIIEncoding.ASCII.GetChars(data, offs + idx, 1));
        }

        return sb.ToString();
      }

      public string TabbedString()
      {
        StringBuilder sb = new StringBuilder();
        int idx;
        for (idx = 0; idx < _values.Length; idx++)
        {
          sb.Append(_values[idx]);
          sb.Append('\t');
        }

        return sb.ToString();
      }
    }
  }
}
