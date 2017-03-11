using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using System.Configuration;
using JgZeitHelper;
using System.Text;
using JgMaschineData;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main(string[] args)
    {
      using (var db = new JgMaschineData.JgModelContainer())
      {
        var dl = new DatenListe<tabStandort>(db)
        {
          OnDatenLaden = (d, p) =>
          {
            var c = p["Wert1"].ToString();
            return db.tabStandortSet.Where(w => w.Bezeichnung == c).ToList();
          }
        };
        dl.Parameter.Add("Wert1", "Heidenau");
        dl.DatenLaden();

        foreach (var f in dl.Daten)
        {
          Console.WriteLine($"{f.Bezeichnung}");
        }
      }

      Console.ReadKey();
    }
  }

  public class DatenListe<T>
  {
    public delegate IEnumerable<T> DatenLadenDelegate(JgModelContainer Db, Dictionary<string, object> Parameter);
    public DatenLadenDelegate OnDatenLaden = null;

    public JgModelContainer Db;

    public Dictionary<string, object> Parameter = new Dictionary<string, object>();

    private ObservableCollection<T> _Daten = new ObservableCollection<T>();
    public ObservableCollection<T> Daten
    {
      get { return _Daten; }
      set { _Daten = new ObservableCollection<T>(value); }
    }

    public DatenListe(JgModelContainer Db)
    {
      DatenLaden();
    }

    public void DatenLaden()
    {
      if (OnDatenLaden != null)
      {
        _Daten = new ObservableCollection<T>(OnDatenLaden(Db, Parameter));
      }
    }
  }
}

