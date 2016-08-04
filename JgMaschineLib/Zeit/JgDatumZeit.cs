using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;

namespace JgMaschineLib.Zeit
{
  public class JgDatumZeitControl : FrameworkElement
  {
    public static readonly DependencyProperty DatumProperty = DependencyProperty.Register("Datum", typeof(DateTime), typeof(JgDatumZeitControl)
      , new FrameworkPropertyMetadata()
      {
        DefaultValue = DateTime.Now.Date,
        BindsTwoWayByDefault = true
      });

    public DateTime Datum
    {
      get { return (DateTime)GetValue(DatumProperty); }
      set
      {
        SetValue(DatumProperty, value);
        _DatumZeit.DatumZeit = value;
      }
    }

    private JgDatumZeitDatensatz _DatumZeit = new JgDatumZeitDatensatz();

    public DateTimePicker DatumPicker { get; set; } = null;
    public ComboBox ComboBoxStunde { get; set; } = null;
    public ComboBox ComboBoxMinute { get; set; } = null;
    public ComboBox ComboboxSekunde { get; set; } = null;

    public JgDatumZeitControl()
    {
      _DatumZeit.PropertyChanged += (sen, erg) =>
      {
        Datum = _DatumZeit.DatumZeit;
      };

      if (DatumPicker != null)
      {
        DatumPicker.DataBindings.Add("Value", _DatumZeit, "Datum");
      }

      if (ComboBoxStunde != null)
      {
        for (int i = 0; i < 24; i++)
          ComboBoxStunde.Items.Add(i);
        ComboBoxStunde.DataBindings.Add("SelectedItem", _DatumZeit, "Stunde", true, DataSourceUpdateMode.OnPropertyChanged, "-", "D2");
      }

      if (ComboBoxMinute != null)
      {
        for (int i = 0; i < 59; i++)
          ComboBoxMinute.Items.Add(i);
        ComboBoxStunde.DataBindings.Add("SelectedItem", _DatumZeit, "Minute", true, DataSourceUpdateMode.OnPropertyChanged, "-", "D2");
      }

      if (ComboboxSekunde != null)
      {
        for (int i = 0; i < 59; i++)
          ComboboxSekunde.Items.Add(i);
        ComboBoxStunde.DataBindings.Add("SelectedItem", _DatumZeit, "Sekunde", true, DataSourceUpdateMode.OnPropertyChanged, "-", "D2");
      }
    }
  }

  public class JgDatumZeitDatensatz : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        NotifyPropertyChanged("Sekunde");
      }
    }

    public DateTime Datum
    {
      get { return _DatumZeit; }
      set
      {
        DatumZeit = new DateTime(value.Year, value.Month, value.Day, _DatumZeit.Hour, _DatumZeit.Minute, _DatumZeit.Second);
        NotifyPropertyChanged();
      }
    }

    public int Stunde
    {
      get { return _DatumZeit.Hour; }
      set
      {
        DatumZeit = new DateTime(_DatumZeit.Year, _DatumZeit.Month, _DatumZeit.Day, value, _DatumZeit.Minute, _DatumZeit.Second);
        NotifyPropertyChanged();
      }
    }

    public int Minute
    {
      get { return _DatumZeit.Minute; }
      set
      {
        DatumZeit = new DateTime(_DatumZeit.Year, _DatumZeit.Month, _DatumZeit.Day, _DatumZeit.Hour, value, _DatumZeit.Second);
        NotifyPropertyChanged();
      }
    }

    public int Sekunde
    {
      get { return _DatumZeit.Second; }
      set
      {
        DatumZeit = new DateTime(_DatumZeit.Year, _DatumZeit.Month, _DatumZeit.Day, _DatumZeit.Hour, _DatumZeit.Minute, value);
        NotifyPropertyChanged();
      }
    }

    public JgDatumZeitDatensatz()
     : this(DateTime.Now)
    { }

    public JgDatumZeitDatensatz(DateTime NeuDatum)
    {
      DatumZeit = NeuDatum;
    }
  }
}
