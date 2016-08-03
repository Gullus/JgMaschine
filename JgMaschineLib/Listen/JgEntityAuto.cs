using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using JgMaschineData;

namespace JgMaschineLib
{
  //private ObjectContext _Context { get { return (_Db as IObjectContextAdapter).ObjectContext; } }

  public delegate void JgEntityAutoTimerAusgeloestDelegate();

  public class JgEntityAuto
  {
    public enum TabArt
    {
      Standort,
      Bediener,
      Maschine,
      Arbeitszeit,
      Anmeldung,
      Bauteil,
      Reparatur,
      RepAnmeldung
    }

    private JgModelContainer _Db;
    private DispatcherTimer _Timer;
    private Dictionary<TabArt, JgEntityTab> _Tabs = new Dictionary<TabArt, JgEntityTab>();

    public JgEntityAutoTimerAusgeloestDelegate TimerAusgeloest { get; set; }

    public JgModelContainer Db { get { return _Db; } }
    public DispatcherTimer Timer { get { return _Timer; } }
    public Dictionary<TabArt, JgEntityTab> Tabs { get { return _Tabs; } }

    public JgEntityAuto(String ConnectionString, int AktualisierungsIntervall = 30)
    {
      _Db = new JgModelContainer();
      _Db.Database.Connection.ConnectionString = ConnectionString;

      _Timer = new DispatcherTimer(new TimeSpan(0, 0, AktualisierungsIntervall), DispatcherPriority.SystemIdle, (sen, erg) =>
      {
        (sen as DispatcherTimer).Stop();

        DatenAktualisieren();
        TimerAusgeloest?.Invoke();

        (sen as DispatcherTimer).Start();
      }, Dispatcher.CurrentDispatcher);
    }

    public SqlConnection DbVerbindung()
    {
      var verbindung = new SqlConnection(_Db.Database.Connection.ConnectionString);
      try
      {
        verbindung.Open();
      }
      catch (Exception f)
      {
        Helper.Protokoll($"Fehler beim öffnen der Datenbank !\nGrund: {f.Message}\nZeichenfolge: {verbindung}", Helper.ProtokollArt.Fehler);
        return null;
      }

      return verbindung;
    }

    public void DatenAktualisieren()
    {
      var verbindung = DbVerbindung();
      try
      {
        foreach (var tab in _Tabs.Values)
          tab.DatenAktualisieren(verbindung);
      }
      catch (Exception f)
      {
        Helper.Protokoll($"Fehler bei der Datenaktualisierung!\nGrund: {f.Message}");
      }
      finally
      {
        verbindung.Close();
      }

      foreach (var tab in _Tabs.Values)
      {
        if (!tab.DatenAnViewSource) 
          tab.Refresh();
        else if (tab.RefreshAusloesen)
          tab.ViewSource?.View?.Refresh();
      }
    }
  }

  public delegate string AbfrageSqlStringDelegate();

  public abstract class JgEntityTab
  {
    protected JgModelContainer _Db;
    protected int _ZeilenNummer = 0;
    protected CollectionViewSource _ViewSource = null;

    public  bool RefreshAusloesen = false;
    public bool DatenAnViewSource = true;
    public static readonly string IstNull = "#IstNull#";

    public DataGrid[] Tabellen { get; set; }
    public abstract CollectionViewSource ViewSource { get; set; }
    public abstract void DatenAktualisieren(SqlConnection SqlVerbindung);
    public abstract void Refresh();

    public static string IdisInString(Guid[] Idis)
    {
      return "'" + string.Join("','", Idis) + "'";
    }
  }

  public class JgEntityTab<K> : JgEntityTab
    where K : class
  {
    public Guid[] Idis { get; set; }
    private ObservableCollection<K> _Daten = new ObservableCollection<K>();

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

    public AbfrageSqlStringDelegate AbfrageSqlString { get; set; }

    public void SetIdis(SqlConnection SqlVerbindung)
    {
      string queryString = AbfrageSqlString();

      if (queryString != IstNull)
      {
        var guids = new List<Guid>();
        var com = new SqlCommand(queryString, SqlVerbindung);
        using (var reader = com.ExecuteReader())
        {
          while (reader.Read())
            guids.Add((Guid)reader[0]);
        };

        if (guids.Count > 0)
        {
          Idis = guids.ToArray();
          return;
        }
      }

      Idis = null;
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

    public override void DatenAktualisieren(SqlConnection SqlVerbindung)
    {
      RefreshAusloesen = false;
      var queryString = AbfrageSqlString();

      if (queryString == IstNull)
        _Daten.Clear();
      else
      {
        var listeDb = ListeAusDatenbankLaden(SqlVerbindung, queryString);

        var listeLoeschen = new List<K>();
        var idisVorhanden = new SortedSet<Guid>();
        foreach (var ds in _Daten)
        {
          var dsEntity = _Db.Entry<K>(ds);
          var id = (Guid)dsEntity.Property("Id").CurrentValue;

          if (listeDb.ContainsKey(id))
          {
            idisVorhanden.Add(id);
            if (dsEntity.Property<DatenAbgleich>("DatenAbgleich").CurrentValue.Datum != listeDb[id])
            {
              _Db.Entry(ds).Reload();
              RefreshAusloesen = true;
            }
          }
          else
            listeLoeschen.Add(ds);
        }

        foreach (var ds in listeLoeschen)
        {
          _Daten.Remove(ds);
          _Db.Entry(ds).Reload();
        }

        if (Idis != null)
        {
          var idisNeu = Idis.Where(w => !idisVorhanden.Contains(w)).ToArray();
          foreach (var id in idisNeu)
            _Daten.Add(_Db.Set<K>().Find(id));
        }
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
    public JgEntityTab(JgModelContainer Db, bool DatenAnViewSourceBinden = true)
    {
      _Db = Db;
      DatenAnViewSource = DatenAnViewSourceBinden;
    }

    public void MerkeZeile()
    {
      _ZeilenNummer = 0;
      if (ViewSource?.View != null)
        _ZeilenNummer = _ViewSource.View.CurrentPosition;
    }

    public void SetzeZeile()
    {
      if ((ViewSource != null) && (_Daten.Count > 0) && (_ZeilenNummer > 0))
      {
        if (_ZeilenNummer >= _Daten.Count)
          _ZeilenNummer = _Daten.Count - 1;
        _ViewSource.View.MoveCurrentToPosition(_ZeilenNummer);
      }
    }

    public void Add(K DatenSatz, bool Sichern = true)
    {
      var ds = _Db.Entry(DatenSatz);
      ds.Property("Id").CurrentValue = Guid.NewGuid();
      DbSichern.AbgleichEintragen((DatenAbgleich)ds.Property("DatenAbgleich").CurrentValue, EnumStatusDatenabgleich.Neu);
      _Db.Set<K>().Add(DatenSatz);
      _Daten.Add(DatenSatz);

      if (Sichern)
      {
        _Db.SaveChanges();
        _ViewSource.View.MoveCurrentTo(DatenSatz);
      }
    }

    public void GeheZuDatensatz(K DatenSatz, bool Sichern = false)
    {
      if (Sichern)
        _Db.SaveChanges();

      if (_ViewSource?.View != null)
      {
        _ViewSource?.View?.MoveCurrentTo(DatenSatz);

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
      _Db.Set<K>().Remove(DatenSatz ?? Current);
      _Db.SaveChanges();
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
      var ds = _Db.Entry(DatenSatz ?? Current);
      DbSichern.AbgleichEintragen((DatenAbgleich)ds.Property("DatenAbgleich").CurrentValue, EnumStatusDatenabgleich.Neu);
      _Db.SaveChanges();
    }

    public override void Refresh()
    {
      _ViewSource?.View?.Refresh();
    }
  }
}
