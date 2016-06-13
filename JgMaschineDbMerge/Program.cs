using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace JgMaschineDbMerge
{
  class Program
  {
    static void Main(string[] args)
    {
      var dba = new DatenBankAbgleich(Properties.Settings.Default.SqlConnectionServer, Properties.Settings.Default.SqlConnectionClient, true);
      Console.WriteLine("Fertsch...");
      Console.ReadKey();
    }
  }

  public class DatenBankAbgleich
  {
    private string[] _ListeTabellen = new string[] { "tabAuswertungSet", "tabStandortSet", "tabMaschineSet", "tabBedienerSet", "tabProtokollSet", "tabArbeitszeitSet", "tabAnmeldungMaschineSet", "tabBauteilSet" };
    private SqlConnection _ConServer;
    private SqlConnection _ConNiederlassung;
    private bool _GesamteDatenbank;

    public DatenBankAbgleich(string VerbindungsStringServer, string VerbindungsStringNiederlassung, bool GesamteDatenbank = false)
    {
      _GesamteDatenbank = GesamteDatenbank;
      _ConServer = VerbindungOeffnen(VerbindungsStringServer, "Server");
      if (_ConServer == null)
        return;

      _ConNiederlassung = VerbindungOeffnen(VerbindungsStringNiederlassung, "Niederlassung");
      if (_ConNiederlassung == null)
      {
        _ConServer.Close();
        return;
      }

      var tabNameFehler = "";
      try
      {
        foreach (var tabelle in _ListeTabellen)
        {
          tabNameFehler = tabelle;
          var felder = FelderAuslesen(_ConServer, tabelle);
          TabelleAbgleichen(_ConServer, _ConNiederlassung, tabelle, felder);
          TabelleAbgleichen(_ConNiederlassung, _ConServer, tabelle, felder);
        }
      }
      catch (Exception f)
      {
        Protokoll($"Fehler beim DbMerge in Tabelle '{tabNameFehler}'.\n\rGrund: {f.Message}");
      }
      finally
      {
        _ConServer.Close();
        _ConNiederlassung.Close();
      }
    }

    public void TabelleAbgleichen(SqlConnection ConVon, SqlConnection ConNach, string Tabelle, string[] Felder)
    {
      var sendAnVon = new StringBuilder();
      var sendAnNach = new StringBuilder();

      var dmVon = new DatenMatrix(Felder);

      byte statusAbgleich = 2;
      if (_GesamteDatenbank)
        statusAbgleich = 100;

      var sq = $"SELECT * FROM {Tabelle} WHERE DatenAbgleich_Status < {statusAbgleich}";

      var com = new SqlCommand(sq, ConVon);
      var reader = com.ExecuteReader();

      while (reader.Read())
        dmVon.ReaderEintragen(reader);
      reader.Close();

      if (dmVon.ListeDaten.Count == 0)  // Wenn keine Daten vorhanden sind
        return;

      var dmNach = new DatenMatrix(Felder);
      var idis = JgMaschineLib.Helper.ListInString<string>(dmVon.ListeDaten.Keys.ToArray(), ",", "'");
      sq = $"SELECT * FROM {Tabelle} WHERE Id IN ({idis})";
      com = new SqlCommand(sq, ConNach);
      reader = com.ExecuteReader();

      while (reader.Read())
        dmNach.ReaderEintragen(reader);
      reader.Close();

      foreach (var dsVon in dmVon.ListeDaten)
      {
        var geaendertVon = Convert.ToDateTime(dmVon.GetWert(dsVon.Value, "DatenAbgleich_Datum"));

        if (dmNach.ListeDaten.ContainsKey(dsVon.Key))
        {
          var dsNach = dmNach.ListeDaten[dsVon.Key];
          var geaendertNach = Convert.ToDateTime(dmNach.GetWert(dsNach, "DatenAbgleich_Datum"));
          var istStatusNachOffen = Convert.ToInt16(dmNach.GetWert(dsNach, "DatenAbgleich_Status")) != 2;

          var matrixVon = dmVon;
          var satzVon = dsVon.Value;
          var matrixNach = dmNach;
          var satzNach = dsNach;
          var sbVon = sendAnVon;
          var sbNach = sendAnNach;

          if (istStatusNachOffen && (geaendertNach > geaendertVon))
          {
            matrixVon = dmNach;
            satzVon = dsNach;
            matrixNach = dmVon;
            satzNach = dsVon.Value;
            sbVon = sendAnNach;
            sbNach = sendAnVon;
          }

          var sb = new StringBuilder();
          foreach (var feld in Felder)
          {
            if ((feld != "Id") && (feld != "DatenAbgleich_Status"))
            {
              var wertVon = matrixVon.GetString(satzVon, feld);
              if (wertVon != matrixNach.GetString(satzNach, feld))
                sb.Append($"{feld} = {wertVon}, ");
            }
          }
          sb.Append($"Datenabgleich_Status = 2");
          sbNach.AppendLine($"UPDATE {Tabelle} SET {sb.ToString()} WHERE Id = {dmVon.GetString((object[])dsVon.Value, "Id")}");
          sbVon.AppendLine($"UPDATE {Tabelle} SET DatenAbgleich_Status = 2 WHERE Id = {dmVon.GetString((object[])dsVon.Value, "Id")}");
        }
        else // Insertanweisung für Tabelle Nach erstellen, wenn kein Datensatz vorhanden
        {
          var sb = new StringBuilder();
          foreach (var feld in Felder)
          {
            if (feld == "DatenAbgleich_Status")
              sb.Append("2,");
            else
              sb.Append(dmVon.GetString(dsVon.Value, feld) + ",");
          }
          string felder = string.Join(",", Felder);

          sendAnNach.AppendLine($"INSERT INTO {Tabelle} ({felder}) VALUES ({JgMaschineLib.Helper.EntferneLetztesZeichen(sb.ToString())})");
          sendAnVon.AppendLine($"UPDATE {Tabelle} SET DatenAbgleich_Status = 2 WHERE Id = {dmVon.GetString((object[])dsVon.Value, "Id")}");
        }
      }

      var transVon = ConVon.BeginTransaction();
      var transNach = ConNach.BeginTransaction();

      try
      {
        com = new SqlCommand(sendAnVon.ToString(), ConVon);
        com.Transaction = transVon;
        com.ExecuteNonQuery();

        com = new SqlCommand(sendAnNach.ToString(), ConNach);
        com.Transaction = transNach;
        com.ExecuteNonQuery();

        transVon.Commit();
        transNach.Commit();

        Console.WriteLine($"Tabelle {Tabelle} erfolgreich eingetragen.");
      }
      catch (Exception f)
      {
        transVon.Rollback();
        transNach.Rollback();

        throw new Exception($"Fehler beim eintragen der Daten in die Datenbank. \r\nGrund: {f.Message}");
      }

      if (Tabelle == "tabBauteilSet")
        N_N_Verbindung(ConVon, ConNach, "tabBauteiltabBediener", idis);
    }

    public string[] FelderAuslesen(SqlConnection Verbindung, string TabellenName)
    {
      var sq = $"SELECT column_name FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{TabellenName}'";
      var com = new SqlCommand(sq, Verbindung);

      List<string> felder = new List<string>();

      SqlDataReader reader = com.ExecuteReader();
      while (reader.Read())
        felder.Add(reader[0].ToString());
      reader.Close();

      return felder.ToArray();
    }

    public SqlConnection VerbindungOeffnen(string Verbindungsstring, string Vorgang)
    {
      var Verbindung = new SqlConnection(Verbindungsstring);
      try
      {
        Verbindung.Open();
      }
      catch (Exception f)
      {
        Protokoll($"Fehler beim öffnen der {Vorgang} Verbindung {Verbindungsstring}.\n\rGrund: {f.Message} ");
        return null;
      }
      return Verbindung;
    }

    public void Protokoll(string ProtokollText)
    {
      Console.WriteLine(ProtokollText);
    }

    public void N_N_Verbindung(SqlConnection ConVon, SqlConnection conNach, string TabName, string IdisListeIdVon)
    {
      var felder = FelderAuslesen(ConVon, TabName);
      var sq = $"SELECT * FROM {TabName} WHERE {felder[0]} IN ({IdisListeIdVon})";

      var lVon = new SortedSet<string>();
      var lNach = new SortedSet<string>();

      var com = new SqlCommand(sq, ConVon);
      var reader = com.ExecuteReader();
      while (reader.Read())
        lVon.Add(reader[0].ToString() + ";" + reader[1].ToString());
      reader.Close();

      com = new SqlCommand(sq, conNach);
      reader = com.ExecuteReader();
      while (reader.Read())
        lNach.Add(reader[0].ToString() + ";" + reader[1].ToString());
      reader.Close();

      string felderNamen = string.Join(",", felder);

      var sbNach = new StringBuilder();
      foreach (var ds in lVon)
      {
        if (!lNach.Contains(ds))
          sbNach.AppendLine($"INSERT INTO {TabName} ({felderNamen}) VALUES (N'{ds.Substring(0, 36)}', N'{ds.Substring(37)}')");
      }
      foreach (var ds in lNach)
      {
        if (!lVon.Contains(ds))
          sbNach.AppendLine($"DELETE FROM {TabName} WHERE ({felder[0]} = N'{ds.Substring(0, 36)}') AND ({felder[1]} = N'{ds.Substring(37)}')");
      }

      if (sbNach.ToString() != "")
      {
        com = new SqlCommand(sbNach.ToString(), conNach);
        com.ExecuteNonQuery();
      }
    }
  }

  public class DatenMatrix
  {
    private string[] _Felder;
    public string[] Felder { get { return _Felder; } }

    private SortedDictionary<string, object[]> _ListeDaten = new SortedDictionary<string, object[]>();
    public SortedDictionary<string, object[]> ListeDaten { get { return _ListeDaten; } }

    public DatenMatrix(string[] Felder)
    {
      _Felder = Felder;
    }

    public int NummerFeld(string FeldName)
    {
      for (int i = 0; i < _Felder.Length; i++)
      {
        if (FeldName == _Felder[i])
          return i;
      }
      return -1;
    }

    public void ReaderEintragen(SqlDataReader Reader)
    {
      var ds = new object[_Felder.Length];
      for (int i = 0; i < _Felder.Length; i++)
        ds[i] = Reader[_Felder[i]];

      _ListeDaten.Add(Reader["Id"].ToString(), ds);
    }

    public object GetWert(object[] Werte, string Feld)
    {
      return Werte[NummerFeld(Feld)];
    }

    public string GetString(object[] Werte, string Feld)
    {
      var wert = GetWert(Werte, Feld);
      if (wert is DBNull)
        return "NULL";

      if (wert is System.Boolean)
        return Convert.ToBoolean(wert) ? "1" : "0";

      if (wert is System.Decimal)
        return JgMaschineLib.Helper.DezimalInString((System.Decimal)wert);

      if ((wert is System.String) || (wert is System.Guid) || (wert is System.DateTime))
        return $"N'{wert}'";

      if (wert is byte[])
        return "0x" + BitConverter.ToString((byte[])wert).Replace("-", string.Empty);

      return wert.ToString();
    }
  }
}
