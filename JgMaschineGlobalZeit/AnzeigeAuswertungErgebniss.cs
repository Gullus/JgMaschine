using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JgGlobalZeit
{
  public class AnzeigeAuswertungErgebniss : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private TimeSpan _UeberstundenKumulativ;
    public TimeSpan UeberstundenKumulativ
    {
      get
      {
        return _UeberstundenKumulativ;
      }

      set
      {
        if (value != _UeberstundenKumulativ)
        {
          _UeberstundenKumulativ = value;
          NotifyPropertyChanged();

        }
      }
    }

    private TimeSpan _UeberstundenVormonat;
    public TimeSpan UeberstundenVormonat
    {
      get
      {
        return _UeberstundenVormonat;
      }

      set
      {
        if (value != _UeberstundenVormonat)
        {
          _UeberstundenVormonat = value;
          NotifyPropertyChanged();

        }
      }
    }

    private int _UrlaubKumulativ;
    public int UrlaubKumulativ
    {
      get
      {
        return _UrlaubKumulativ;
      }

      set
      {
        if (value != _UrlaubVormonat)
        {
          _UrlaubKumulativ = value;
          NotifyPropertyChanged();

        }
      }
    }

    private int _UrlaubVormonat;
    public int UrlaubVormonat
    {
      get
      {
        return _UrlaubVormonat;
      }

      set
      {
        if (value != _UrlaubVormonat)
        {
          _UrlaubVormonat = value;
          NotifyPropertyChanged();

        }
      }
    }

    private int _KrankVormonat;
    public int KrankVormonat
    {
      get
      {
        return _KrankVormonat;
      }

      set
      {
        if (value != _KrankVormonat)
        {
          _KrankVormonat = value;
          NotifyPropertyChanged();

        }
      }
    }

    private int _KrankAktuell;
    public int KrankAktuell
    {
      get
      {
        return _KrankAktuell;
      }

      set
      {
        if (value != _KrankAktuell)
        {
          _KrankAktuell = value;
          NotifyPropertyChanged();

        }
      }
    }
  }
}
