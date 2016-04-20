using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JgMaschineLib.Zeit
{
  public class JgFormDatumZeit
  {
    private DateTime _Datum;
    public DateTime Datum { get { return _Datum; } }

    public JgFormDatumZeit()
    { }

    public bool Anzeigen(string Caption, string AnzeigeText, DateTime DatumZeit)
    {
      JgMaschineLib.Zeit.FormAuswahlDatumZeit form = new Zeit.FormAuswahlDatumZeit(Caption, AnzeigeText, DatumZeit);
      if (form.ShowDialog() ?? false)
      {
        _Datum = form.DatumZeit;
        return true;
      }
      return false;
    }
  }

  public class JgDatumZeit : INotifyPropertyChanged 
  {
    public delegate void DelegateNeuerWert(DateTime Datum);
    public DelegateNeuerWert NeuerWert = null;

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    private DateTime _DatumZeit;
    public DateTime DatumZeit
    {
      get { return _DatumZeit; }
      set 
      { 
        _DatumZeit = value;
        NotifyPropertyChanged();
        NotifyPropertyChanged("Stunde");
        NotifyPropertyChanged("Minute");
        NotifyPropertyChanged("Datum");

        if (NeuerWert != null)
          NeuerWert(value);
      }
    }

    public DateTime Datum 
    {
      get { return _DatumZeit; }
      set { DatumZeit = new DateTime(value.Year, value.Month, value.Day, _DatumZeit.Hour, _DatumZeit.Minute, _DatumZeit.Second); }
    }

    public int Stunde
    {
      get { return _DatumZeit.Hour; }
      set { DatumZeit = new DateTime(_DatumZeit.Year, _DatumZeit.Month, _DatumZeit.Day, value, _DatumZeit.Minute, _DatumZeit.Second); }
    }

    public int Minute
    {
      get { return _DatumZeit.Minute; }
      set { DatumZeit = new DateTime(_DatumZeit.Year, _DatumZeit.Month, _DatumZeit.Day, _DatumZeit.Hour, value, _DatumZeit.Second); }
    }

    public int[] VorgabeStunden
    {
      get
      {
        var stund = new int[24];
        for (int i = 0; i < 24; i++)
          stund[i] = i;
        return stund;
      }
    }

    public static int[] VorgabeMinuten
    {
      get
      {
        var min = new int[60];
        for (int i = 0; i < 60; i++)
          min[i] = i;
        return min;
      }
    }

    public JgDatumZeit()
     : this (DateTime.Now)
    { }

    public JgDatumZeit(DateTime NeuDatum)
    {
      DatumZeit = NeuDatum;
    }
  }

  public class JgZeit : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    private TimeSpan _Zeit;
    public TimeSpan Zeit
    {
      get { return _Zeit; }
      set
      {
        _Zeit = value;
        NotifyPropertyChanged();
        NotifyPropertyChanged("Stunde");
        NotifyPropertyChanged("Minute");
        NotifyPropertyChanged("Sekunde");
      }
    }

    public int Stunde
    {
      get { return _Zeit.Hours; }
      set { Zeit = new TimeSpan(value, Minute, Sekunde); }
    }

    public int Minute
    {
      get { return _Zeit.Minutes; }
      set { Zeit = new TimeSpan(Stunde, value, Sekunde); }
    }

    public int Sekunde
    {
      get { return _Zeit.Seconds; }
      set { Zeit = new TimeSpan(Stunde, Minute, value); }
    }

    public JgZeit()
      : this(DateTime.Now)
    { }

    public JgZeit(DateTime AktuellesDatum)
    {
      _Zeit = DatumInZeit(AktuellesDatum);
    }

    public static TimeSpan DatumInZeit(DateTime Datum)
    {
      return new TimeSpan(Datum.Hour, Datum.Minute, Datum.Second);
    }
  }
}
