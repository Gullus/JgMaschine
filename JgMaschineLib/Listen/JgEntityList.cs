using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using JgMaschineData;

namespace JgMaschineLib
{
  public abstract class JgEntityList
  {
    protected JgModelContainer _Db;
    public JgModelContainer Db { get { return _Db; } }

    protected int _ZeilenNummer = 0;
    protected CollectionViewSource _ViewSource = null;

    public bool RefreshAusloesen = false;
    public bool DatenAnViewSource = true;

    public DataGrid[] Tabellen { get; set; }
    public abstract CollectionViewSource ViewSource { get; set; }

    public abstract void Refresh();

    public static string IdisInString(Guid[] Idis)
    {
      return "'" + string.Join("','", Idis) + "'";
    }
  }

  public class JgEntityList<K> : JgEntityList
    where K : class
  {
    public Guid[] Idis { get; set; }
    private ObservableCollection<K> _Daten = new ObservableCollection<K>();

    #region Laden von Daten über ein Delegaten

    public Dictionary<string, object> Parameter = null;

    public delegate IEnumerable<K> DatenLadenDelegate(JgModelContainer Db, Dictionary<string, object> Parameter);
    public DatenLadenDelegate OnDatenLaden = null;

    public void DatenLaden()
    {
      if (OnDatenLaden != null)
        Daten = OnDatenLaden(_Db, Parameter);
    }

    public void DatenAktualisieren()
    {
      if (OnDatenLaden != null)
      {
        if (_Daten.Count == 0)
        {
          Daten = OnDatenLaden(_Db, Parameter);
        }
        else
        {
          var propId = typeof(K).GetProperty("Id");
          var propDateAbleich = typeof(K).GetProperty("DatenAbgleich");

          var idisVorhanden = new SortedSet<Guid>();
          foreach (var ds in _Daten)
            idisVorhanden.Add((Guid)propId.GetValue(ds));

          Daten = OnDatenLaden(_Db, Parameter);

          var sbAbfrageText = new StringBuilder();
          foreach (var ds in Daten)
          {
            var id = (Guid)propId.GetValue(ds);
            if (idisVorhanden.Contains(id))
              sbAbfrageText.AppendLine($"  ('{id.ToString()}','{(propDateAbleich.GetValue(ds) as DatenAbgleich).Datum.ToString("dd.MM.yyyy HH:mm:ss")}'),");
          }

          var sqlText = "IF OBJECT_ID(N'tempdb..#TempDs', N'U') IS NOT NULL DROP TABLE #TempDs \n"
                      + "CREATE TABLE #TempDs (Id uniqueidentifier NOT NULL, Datum char(19) NOT NULL) \n"
                      + "INSERT INTO #TempDs VALUES \n"
                      + sbAbfrageText.ToString().Substring(0, sbAbfrageText.ToString().Length - 3) + "\n"
                      + "SELECT Id FROM #TempDs as t \n"
                      + "  WHERE EXISTS(SELECT * FROM " + typeof(K).Name +  "Set WHERE (Id = t.Id) AND (FORMAT(DatenAbgleich_Datum , 'dd/MM/yyyy HH:mm:ss') <> t.Datum))";

          var idisAendern = new SortedSet<Guid>();
          var sc = _Db.Database.Connection.ConnectionString;
          using (var con = new SqlConnection(sc))
          {
            con.Open();
            var cl = new SqlCommand(sqlText, con);
            using (var reader = cl.ExecuteReader())
            {
              while (reader.Read())
                idisAendern.Add((Guid)reader[0]);
            }
          }

          if (idisAendern.Count > 0)
          {
            foreach (var ds in Daten)
            {
              var id = (Guid)propId.GetValue(ds);
              if (idisAendern.Contains(id))
                _Db.Entry<K>(ds).Reload();
            }
          }
        }
      }
    }
    #endregion

    public IEnumerable<K> Daten
    {
      get { return _Daten; }
      set
      {
        _Daten = new ObservableCollection<K>(value);
        if (DatenAnViewSource && (_ViewSource != null))
          _ViewSource.Source = _Daten;
      }
    }

    private Dictionary<Guid, DateTime> ListeAusDatenbankLaden(SqlConnection SqlVerbindung, string QueryString)
    {
      var dbDaten = new Dictionary<Guid, DateTime>();
      var com = new SqlCommand(QueryString, SqlVerbindung);
      using (
      var reader = com.ExecuteReader())
      {
        while (reader.Read())
          dbDaten.Add((Guid)reader[0], (DateTime)reader[1]);
      }

      if (dbDaten.Count > 0)
        Idis = dbDaten.Keys.ToArray();
      else
        Idis = null;

      return dbDaten;
    }

    public void ErgebnissFormular(bool? ErgebnissShowDialog, bool istNeu, K Datensatz)
    {
      if (ErgebnissShowDialog ?? false)
      {
        if (istNeu)
          Add(Datensatz);
      }
      else if (!istNeu)
      {
        MerkeZeile();
        Reload(Datensatz);
        Refresh();
        GeheZuZeile();
      }
    }

    public K Current
    {
      get { return (K)_ViewSource?.View?.CurrentItem; }
    }

    public override CollectionViewSource ViewSource
    {
      get { return _ViewSource; }
      set
      {
        _ViewSource = value;
        if (DatenAnViewSource && (_ViewSource != null))
          _ViewSource.Source = _Daten;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Db"></param>
    /// <param name="DatenAnViewSourceBinden">Bei Detailtabellen dürfen die Daten nicht an Viesource gebunden Werden</param>
    public JgEntityList(JgModelContainer NeuDb, bool DatenAnViewSourceBinden = true)
    {
      _Db = NeuDb;
      DatenAnViewSource = DatenAnViewSourceBinden;
    }

    public void MerkeZeile()
    {
      _ZeilenNummer = 0;
      if (ViewSource?.View != null)
        _ZeilenNummer = _ViewSource.View.CurrentPosition;
    }

    public void GeheZuZeile()
    {
      if ((ViewSource?.View != null) && (_Daten.Count > 0) && (_ZeilenNummer > 0))
      {
        if (_ZeilenNummer >= _Daten.Count)
          _ZeilenNummer = _Daten.Count - 1;
        _ViewSource.View.MoveCurrentToPosition(_ZeilenNummer);
      }
    }

    public void Add(K DatenSatz, bool Sichern = true)
    {
      var entr = _Db.Entry(DatenSatz);
      entr.Property("Id").CurrentValue = Guid.NewGuid();
      _Db.Set<K>().Add(DatenSatz);
      _Daten.Add(DatenSatz);

      GeheZuDatensatz(DatenSatz);

      if (Sichern)
        DsSave(DatenSatz);
    }

    public void GeheZuDatensatz(K DatenSatz)
    {
      if (_ViewSource?.View != null)
      {
        _ViewSource.View.MoveCurrentTo(DatenSatz);

        if (Tabellen != null)
        {
          foreach (var mGrid in Tabellen)
          {
            if (mGrid.SelectedItem != null)
              mGrid.ScrollIntoView(mGrid.SelectedItem);
          }
        }
      }
    }

    public void Delete(K DatenSatz = null)
    {
      var entr = _Db.Entry<K>(DatenSatz ?? Current);
      var dabgl = (DatenAbgleich)entr.ComplexProperty("DatenAbgleich").CurrentValue;
      dabgl.Geloescht = true;
      DsSave(DatenSatz);
      _Db.Set<K>().Remove(DatenSatz ?? Current);
    }

    public void Remove(K DatenSatz = null)
    {
      _Daten.Remove(DatenSatz ?? Current);
    }

    public void Reload(K DatenSatz = null)
    {
      _Db.Entry(DatenSatz ?? Current).Reload();
    }

    public void DsSave(K DatenSatz = null)
    {
      var entr = _Db.Entry(DatenSatz ?? Current);
      _Db.SaveChanges();
    }

    public override void Refresh()
    {
      _ViewSource?.View?.Refresh();
    }
  }
}
