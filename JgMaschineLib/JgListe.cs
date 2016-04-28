using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace JgMaschineLib
{
  public class JgListe<T> : ObservableCollection<T>
    where T : class
  {
    private bool isInRange = false;
    private DataGrid[] _ListeTabellen;
    private CollectionViewSource _ViewSource;
    private JgMaschineData.JgModelContainer _Db;

    private IQueryable<T> _MyQuery = null;
    public IQueryable<T> MyQuery
    {
      get { return _MyQuery; }
      set
      {
        _MyQuery = value;
        DatenGenerieren();
      }
    }

    public bool IsEmpty { get { return this.Count == 0; } }
    public T AktDatensatz { get { return (T)_ViewSource.View.CurrentItem; } }

    public JgListe(JgMaschineData.JgModelContainer Db, IQueryable<T> MyQuery, CollectionViewSource MyViewSource, params DataGrid[] MyTabels)
    {
      _Db = Db;
      _MyQuery = MyQuery;
      _ViewSource = MyViewSource;
      _ListeTabellen = MyTabels;
    }

    public async Task<int> Init()
    {
      await DatenGenerierenAsync();
      InitClass();
      return 0;
    }

    public async void DatenGenerieren()
    {
      await DatenGenerierenAsync();
    }

    public async Task<int> DatenGenerierenAsync()
    {
      Items.Clear();
      var daten = await MyQuery.ToListAsync();
      isInRange = true;
      foreach (T item in daten)
        Add(item);
      isInRange = false;

      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

      return 0;
    }

    public JgListe(JgMaschineData.JgModelContainer Db, IEnumerable<T> Items, CollectionViewSource MyViewSource, params DataGrid[] MyTabels)
      : base(Items)
    {
      _Db = Db;
      _ViewSource = MyViewSource;
      _ListeTabellen = MyTabels;
      InitClass();
    }

    private void InitClass()
    {
      //PropertyChanged  += (sen, erg) =>
      //{
      //  MessageBox.Show("Geaendert: " + erg.PropertyName);
      //};

      CollectionChanged += (sen, erg) =>
      {
        var ent = _Db.Set<T>();

        switch (erg.Action)
        {
          case NotifyCollectionChangedAction.Add:
            foreach (T ds in erg.NewItems)
            {
              var neu = _Db.Entry<T>(ds);
              var abgl = neu.Member<JgMaschineData.DatenAbgleich>("DatenAbgleich");
              JgMaschineLib.DbSichern.AbgleichEintragen(abgl.CurrentValue, JgMaschineData.EnumStatusDatenabgleich.Neu);
              ent.Add(neu.Entity);
            }
            _Db.SaveChanges();
            break;
          case NotifyCollectionChangedAction.Remove:
            foreach (T ds in erg.OldItems)
            {
              var geloescht = _Db.Entry<T>(ds);
              var abgl = geloescht.Member<JgMaschineData.DatenAbgleich>("DatenAbgleich");
              JgMaschineLib.DbSichern.AbgleichEintragen(abgl.CurrentValue, JgMaschineData.EnumStatusDatenabgleich.Geloescht);
            }
            _Db.SaveChanges();
            break;
          case NotifyCollectionChangedAction.Move:
          case NotifyCollectionChangedAction.Replace:
          case NotifyCollectionChangedAction.Reset:
          default:
            break;
        }
      };

      _ViewSource.Source = this;
      _ViewSource.View.CollectionChanged += (sen, erg) =>
      {
        if (erg.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
          _ViewSource.View.Refresh();
          _ViewSource.View.MoveCurrentToPosition(erg.NewStartingIndex);
          foreach (var mGrid in _ListeTabellen)
          {
            if (mGrid.SelectedItem != null)
              mGrid.ScrollIntoView(mGrid.SelectedItem);
          }
        }
      };
    }

    public void MoveTo(object ZuObject)
    {
      _ViewSource.View.MoveCurrentTo(ZuObject);
      foreach (var mGrid in _ListeTabellen)
      {
        if (mGrid.SelectedItem != null)
          mGrid.ScrollIntoView(mGrid.SelectedItem);
      }
    }

    public void Reload(JgMaschineData.JgModelContainer Db)
    {
      var ent = Db.Entry<T>((T)_ViewSource.View.CurrentItem);
      ent.Reload();
      _ViewSource.View.Refresh();
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
      if (!isInRange)
        base.OnCollectionChanged(e);
    }

    public void AddRange(IEnumerable<T> Items)
    {
      if (Items == null) throw new ArgumentNullException("IEnumerable");

      isInRange = true;
      foreach (T item in Items)
        Add(item);
      isInRange = false;

      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void RemoveRange(IEnumerable<T> items)
    {
      if (Items == null) throw new ArgumentNullException("IEnumerable");

      isInRange = true;
      foreach (T item in Items)
        Remove(item);
      isInRange = false;
    }

    public async void AktSichern(JgMaschineData.EnumStatusDatenabgleich Status)
    {
      var neu = _Db.Entry<T>(AktDatensatz);
      var abgl = neu.Member<JgMaschineData.DatenAbgleich>("DatenAbgleich");
      JgMaschineLib.DbSichern.AbgleichEintragen(abgl.CurrentValue, Status);
      await _Db.SaveChangesAsync();
    }

    public void Refresh()
    {
      _ViewSource.View.Refresh();
    }
  }
}
