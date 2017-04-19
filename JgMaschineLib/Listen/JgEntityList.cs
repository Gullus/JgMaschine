using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using JgMaschineData;

namespace JgMaschineLib
{
  public class JgEntityListEventArgs : EventArgs
  {
    public Dictionary<string, object> Params;
    public bool ErsterDurchlauf;
    public bool IstSortiert;

    public JgEntityListEventArgs(Dictionary<string, object> NeuParams, bool NeuErsterDurchlauf, bool NeuIstSortierung)
    {
      Params = NeuParams;
      ErsterDurchlauf = NeuErsterDurchlauf;
      IstSortiert = NeuIstSortierung;
    }
  }

  public class JgEntityList<K>
    where K : class
  {
    protected JgModelContainer _Db;
    public JgModelContainer Db { get { return _Db; } }

    public delegate IEnumerable<K> DatenLadenDelegate(JgModelContainer Db, JgEntityListEventArgs EventArg);
    public DatenLadenDelegate OnDatenLaden = null;

    protected CollectionViewSource _ViewSource = null;
    public CollectionViewSource ViewSource
    {
      get { return _ViewSource; }
      set
      {
        _ViewSource = value;
        _ViewSource.Source = _Daten;
      }
    }

    public DataGrid[] Tabellen { get; set; }

    public Dictionary<string, object> Parameter = new Dictionary<string, object>();

    private int _ZeilenNummer = 0;
    public bool ErsterDurchlauf = true;
    public bool IstSortierung
    {
      get
      {
        if (Tabellen?.Length > 0)
          return Tabellen[0].Columns.Any(a => a.SortDirection != null);

        return false;
      }
    }

    private ObservableCollection<K> _Daten = new ObservableCollection<K>();

    public void DatenLaden()
    {
      Mouse.OverrideCursor = Cursors.Wait;

      if (OnDatenLaden != null)
        Daten = OnDatenLaden(_Db, new JgEntityListEventArgs(Parameter, ErsterDurchlauf, IstSortierung));

      ErsterDurchlauf = false;

      Mouse.OverrideCursor = null;
    }

    public void DatenAktualisieren()
    {
      if (OnDatenLaden != null)
      {
        Mouse.OverrideCursor = Cursors.Wait;

        MerkeZeile();

        var propId = typeof(K).GetProperty("Id");
        var propDateAbleich = typeof(K).GetProperty("DatenAbgleich");

        var lNeu = OnDatenLaden(_Db, new JgEntityListEventArgs(Parameter, ErsterDurchlauf, IstSortierung));

        var dicAlt = new SortedDictionary<Guid, K>();
        var dicNeu = new SortedDictionary<Guid, K>();

        foreach (var ds in _Daten)
          dicAlt.Add((Guid)propId.GetValue(ds), ds);

        foreach (var ds in lNeu)
          dicNeu.Add((Guid)propId.GetValue(ds), ds);

        // Nicht mehr relevante Datensätze entfernen
        foreach (var dsAlt in dicAlt)
        {
          if (!dicNeu.ContainsKey(dsAlt.Key))
            _Daten.Remove(dsAlt.Value);
        }

        // Neue Datensätze hinzufügen
        foreach (var dsNeu in dicNeu)
        {
          if (!dicAlt.ContainsKey(dsNeu.Key))
            _Daten.Add(dsNeu.Value);
        }

        // geänderte Datensätze aktualisieren, Wenn mehr als 500 Datensätze dann keine Aktualisierung
        if ((dicNeu.Count != 0) && (dicNeu.Count < 500))
        {
          var sbAbfrageText = new StringBuilder();
          foreach (var ds in dicNeu)
            sbAbfrageText.AppendLine($"  ('{ds.Key.ToString()}','{(propDateAbleich.GetValue(ds.Value) as DatenAbgleich).Datum.ToString("dd.MM.yyyy HH:mm:ss")}'),");

          var sqlText = "IF OBJECT_ID(N'tempdb..#TempDs', N'U') IS NOT NULL DROP TABLE #TempDs \n"
                      + "CREATE TABLE #TempDs (Id uniqueidentifier NOT NULL, Datum char(19) NOT NULL) \n"
                      + "INSERT INTO #TempDs VALUES \n"
                      + sbAbfrageText.ToString().Substring(0, sbAbfrageText.ToString().Length - 3) + "\n"
                      + "SELECT Id FROM #TempDs as t \n"
                      + "  WHERE EXISTS(SELECT * FROM " + typeof(K).Name + "Set WHERE (Id = t.Id) AND (FORMAT(DatenAbgleich_Datum , 'dd/MM/yyyy HH:mm:ss') <> t.Datum))";

          var idisAendern = new List<Guid>();
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

          foreach (var ds in idisAendern)
            _Db.Entry<K>(dicNeu[ds]).Reload();

          Refresh();
          GeheZuZeile();

        }

        Mouse.OverrideCursor = null;
      }
    }

    public IEnumerable<K> Daten
    {
      get { return _Daten; }
      set
      {
        _Daten = new ObservableCollection<K>(value);
        if (_ViewSource != null)
          _ViewSource.Source = _Daten;
      }
    }

    public bool ErgebnissFormular(bool? ErgebnissShowDialog, bool istNeu, K Datensatz)
    {
      if (ErgebnissShowDialog ?? false)
      {
        if (istNeu)
          Add(Datensatz);
        else
          _Db.SaveChanges();

        return true;
      }
      else if (!istNeu)
      {
        MerkeZeile();
        Reload(Datensatz);
        Refresh();
        GeheZuZeile();
      }

      return false;
    }

    public K Current
    {
      get { return (K)_ViewSource?.View?.CurrentItem; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Db"></param>
    /// <param name="DatenAnViewSourceBinden">Bei Detailtabellen dürfen die Daten nicht an Viesource gebunden Werden</param>
    public JgEntityList(JgModelContainer NeuDb)
    {
      _Db = NeuDb;
    }

    public void MerkeZeile()
    {
      _ZeilenNummer = _ViewSource?.View?.CurrentPosition ?? 0;
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
      var ds = _Db.Entry(DatenSatz ?? Current);
      
      if (ds.State == System.Data.Entity.EntityState.Modified)
        ds.Reload();
    }

    public void DsSave(K DatenSatz = null)
    {
      var entr = _Db.Entry(DatenSatz ?? Current);
      _Db.SaveChanges();
    }

    public void Refresh()
    {
      _ViewSource?.View?.Refresh();
    }
  }
}
