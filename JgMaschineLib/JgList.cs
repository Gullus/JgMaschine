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
  public class JgList<T> : ObservableCollection<T>
    where T : class
  {
    private bool IstInit = false;
    private bool isInRange = false;
    private string _ConnectionString = "";

    public JgMaschineData.JgModelContainer Db { get; set; } = null;

    public IQueryable<T> MyQuery { get; set; } = null;
    public CollectionViewSource ViewSource { get; set; } = null;
    public DataGrid[] ListeTabellen { get; set; } = null;

    public bool IsEmpty { get { return this.Count == 0; } }
    public T AktDatensatz
    {
      get
      {
        if ((ViewSource != null) && (ViewSource.View != null))
          return (T)ViewSource.View.CurrentItem;
        else
          return null;
      }
    }

    public JgList(CollectionViewSource ViewSource, string ConnectionString = "")
    {
      Db = new JgMaschineData.JgModelContainer();
      if (_ConnectionString != "")
        Db.Database.Connection.ConnectionString = _ConnectionString;
      this.ViewSource = ViewSource;
    }

    public JgList(JgMaschineData.JgModelContainer Db, IEnumerable<T> Items, CollectionViewSource ViewSource)
      : base(Items)
    {
      this.Db = Db;
      this.ViewSource = ViewSource;
      InitClass();
    }

    public void ListClear() => this.Items.Clear();
    public async void DatenGenerieren() => await DatenGenerierenAsync();
    public void Refresh() => ViewSource.View.Refresh();

    public async Task<int> DatenGenerierenAsync()
    {
      if (MyQuery == null)
        throw new Exception("Ohne Angabe von IQuerable, kann keine Datenabfrage durchgeführt werden.");

      Items.Clear();
      isInRange = true;

      Db = new JgMaschineData.JgModelContainer();
      var daten = await MyQuery.ToListAsync();

      foreach (T item in daten)
        Add(item);

      isInRange = false;
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

      if (!IstInit)
        InitClass();

      return daten.Count;
    }

    private void InitClass()
    {
      IstInit = true;

      CollectionChanged += (sen, erg) =>
      {
        var ent = Db.Set<T>();

        switch (erg.Action)
        {
          case NotifyCollectionChangedAction.Add:
            foreach (T ds in erg.NewItems)
            {
              var entNeu = Db.Entry<T>(ds);
              var abgl = entNeu.Property<JgMaschineData.DatenAbgleich>("DatenAbgleich");
              DbSichern.AbgleichEintragen(abgl.CurrentValue, JgMaschineData.EnumStatusDatenabgleich.Neu);
              ent.Add(entNeu.Entity);
            }
            Db.SaveChanges();
            break;
          case NotifyCollectionChangedAction.Remove:
          case NotifyCollectionChangedAction.Move:
          case NotifyCollectionChangedAction.Replace:
          case NotifyCollectionChangedAction.Reset:
          default:
            break;
        }
      };

      if (ViewSource != null)
      {
        ViewSource.Source = this;
        ViewSource.View.CollectionChanged += (sen, erg) =>
        {
          if (erg.Action == NotifyCollectionChangedAction.Add)
          {
            ViewSource.View.Refresh();
            ViewSource.View.MoveCurrentToPosition(erg.NewStartingIndex);
            if (ListeTabellen != null)
            {
              foreach (var mGrid in ListeTabellen)
              {
                if (mGrid.SelectedItem != null)
                  mGrid.ScrollIntoView(mGrid.SelectedItem);
              }
            }
          }
        };
      }
    }

    public void MoveTo(object ZuObject)
    {
      ViewSource.View.MoveCurrentTo(ZuObject);
      foreach (var mGrid in ListeTabellen)
      {
        if (mGrid.SelectedItem != null)
          mGrid.ScrollIntoView(mGrid.SelectedItem);
      }
    }

    public void Reload(T Datensatz)
    {
      var ent = Db.Entry<T>(Datensatz);
      ent.Reload();
      ViewSource.View.Refresh();
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

    public void AktSichern(JgMaschineData.EnumStatusDatenabgleich Status)
    {
      var neu = Db.Entry<T>(AktDatensatz);
      DbSichern.DsSichern<T>(Db, neu.Entity, Status);
    }

    public async Task AktSichernAsync(JgMaschineData.EnumStatusDatenabgleich Status)
    {
      var neu = Db.Entry<T>(AktDatensatz);
      await DbSichern.DsSichernAsync<T>(Db, neu.Entity, Status);
    }

    public void DsSichern(object MeinObjekt, JgMaschineData.EnumStatusDatenabgleich Status)
    {
      DbSichern.DsSichern<T>(Db, MeinObjekt, Status);
    }

    public void DsReload<E>(E Datensatz)
      where E : class
    {
      var ent = Db.Entry<E>(Datensatz);
      ent.Reload();
      ViewSource.View.Refresh();
    }

    public async Task<E> DsAttachAsync<E>(Guid IdDatensatz)
      where E : class
    {
      var ent = Db.Set<E>();
      return await ent.FindAsync(IdDatensatz);
    }

    public E DsAttach<E>(Guid IdDatensatz)
    where E : class
    {
      var ent = Db.Set<E>();
      return ent.Find(IdDatensatz);
    }


    public void Delete()
    {
      //var abgl = AktDatensatz .Property<JgMaschineData.DatenAbgleich>("DatenAbgleich");
      //abgl.CurrentValue.Geloescht = true;
      //await DbSichern.DsSichernAsync<T>(Db, abgl.CurrentValue, JgMaschineData.EnumStatusDatenabgleich.Geaendert);
    }
  }
}
