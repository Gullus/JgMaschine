using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using JgMaschineData;

namespace JgMaschineLib
{
  public delegate IEnumerable<E> JgEntityListDatenErstellenDeleget<E>(JgModelContainer Db);

  public class JgEntityView<K>
    where K : class
  {
    private int _ZeilenNummer = 0;

    private JgModelContainer _Db;
    public JgModelContainer Db { get { return _Db; } }

    public CollectionViewSource ViewSource { get; set; } = null;

    private ObservableCollection<K> _Daten = new ObservableCollection<K>();
    public IEnumerable<K> Daten
    {
      get { return _Daten; }
      set
      {
        if (value == null)
          _Daten.Clear();
        else
        {
          _Daten = new ObservableCollection<K>(value);
          if (ViewSource != null)
            ViewSource.Source = _Daten;
        }
      }
    }

    public JgEntityListDatenErstellenDeleget<K> DatenErstellen;

    public DataGrid[] Tabellen { get; set; }
    public K Current
    {
      get { return (K)ViewSource?.View?.CurrentItem; }
    }

    public JgEntityView(string SqlVerbindungsString = "")
    {
      _Db = new JgModelContainer();
      if (SqlVerbindungsString != "")
        _Db.Database.Connection.ConnectionString = SqlVerbindungsString;
    }

    public void DatenAktualisieren()
    {
      _Db = new JgModelContainer();
      MerkeZeile();
      Daten = DatenErstellen(_Db);
      ZeileEinstellen();
    }

    private void MerkeZeile()
    {
      _ZeilenNummer = 0;
      if (ViewSource?.View != null)
        _ZeilenNummer = ViewSource.View.CurrentPosition;
    }

    private void ZeileEinstellen()
    {
      if (_Daten.Count > 0)
      {
        if (_ZeilenNummer >= _Daten.Count)
          _ZeilenNummer = _Daten.Count - 1;
        ViewSource?.View?.MoveCurrentToPosition(_ZeilenNummer);
      }
    }

    public void Add(K DatenSatz)
    {
      var ds = _Db.Entry(DatenSatz);
      ds.Property("Id").CurrentValue = Guid.NewGuid();
      _Db.Set<K>().Add(DatenSatz);
      _Db.SaveChanges();

      _Daten.Add(DatenSatz);

      if (ViewSource?.View != null)
      {
        ViewSource.View.MoveCurrentTo(DatenSatz);

        foreach (var mGrid in Tabellen)
        {
          if (mGrid.SelectedItem != null)
            mGrid.ScrollIntoView(mGrid.SelectedItem);
        }
      }
    }

    //public void Delete(K DatenSatz = null)
    //{
    //  MerkeZeile();
    //  _Db.Set<K>().Remove(DatenSatz ?? Current);
    //  _Db.SaveChanges();
    //  ViewSource?.View?.Refresh();
    //  ZeileEinstellen();
    //}

    public void AlsGeloeschtKennzeichnen(K DatenSatz = null)
    {
      var entr = _Db.Entry(DatenSatz ?? Current);
      var abgl = (DatenAbgleich)entr.Property<DatenAbgleich>("DatenAbgleich").CurrentValue;
      abgl.Geloescht = true;
      if (entr.State != System.Data.Entity.EntityState.Modified)
        entr.State = System.Data.Entity.EntityState.Modified;
      _Db.SaveChanges();
    }

    public void Reload(K DatenSatz = null)
    {
      MerkeZeile();
      _Db.Entry(DatenSatz ?? Current).Reload();
      ViewSource?.View?.Refresh();
      ZeileEinstellen();
    }

    public void DsSave(K DatenSatz = null)
    {
      var entr = _Db.Entry(DatenSatz ?? Current);
      if (entr.State != System.Data.Entity.EntityState.Modified)
        entr.State = System.Data.Entity.EntityState.Modified;
      _Db.SaveChanges();
    }

    public void Remove(K DatenSatz)
    {
      var entr = _Db.Entry(DatenSatz ?? Current);
      _Daten.Remove(entr.Entity);
    }

    public void Refresh()
    {
      MerkeZeile();
      ViewSource?.View?.Refresh();
      ZeileEinstellen();
    }
  }
}
