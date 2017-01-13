using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace JgMaschineData
{
  public partial class tabMaschineMetaData
  {
    [Required]
    public object MaschinenName;
  }

  [MetadataType(typeof(tabMaschineMetaData))]
  public partial class tabMaschine
  {
    public bool Abbruch { get; set; }
    public string ProtokollAdd
    {
      get { return (this.eProtokoll == null) ? "" : this.eProtokoll.ProtokollText; }
      set
      {
        if (!string.IsNullOrWhiteSpace(value))
        {
          var lines = value.Split(new string[] { "\n" }, StringSplitOptions.None);
          var erg = DateTime.Now.ToString("dd:MM HH:mm:ss ") + lines[0] + "\n";
          for (int i = 1; i < lines.Length; i++)
            erg += "--             " + lines[i] + "\n";

          this.eProtokoll.ProtokollText += erg;
        }
      }
    }
    public bool IstReparatur
    {
      get { return this.fAktivReparatur != null; }
      set { }
    }

  }

  public partial class tabAnmeldungMaschine
  {
    public TimeSpan ZeitAngemeldet
    {
      get { return DateTime.Now - this.Anmeldung; }
    }
  }

  public partial class tabProtokoll
  {
    public string Laufzeit
    {
      get { return ((this.AuswertungStart != null) && (this.AuswertungEnde != null)) ? (this.AuswertungEnde - this.AuswertungStart).ToString() : ""; }
      set { }
    }

    public string StatusAsString
    {
      get { return this.Status.ToString(); }
    }
  }

  public partial class tabBedienerMetaData
  {
    [Required]
    [MinLength(3, ErrorMessage = "Mindestanzahl der Zeichen für den Vornamen ist {1}")]
    public object VorName;

    [Required]
    [MinLength(3, ErrorMessage = "Mindestanzahl der Zeichen für den Nachnamen ist {1}")]
    public object NachName;

    [Required]
    [MinLength(2, ErrorMessage = "Mindestanzahl der Zeichen für den Nachnamen ist {1}")]
    public object MatchCode;
  }

  [MetadataType(typeof(tabBedienerMetaData))]
  public partial class tabBediener : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private EnumStatusArbeitszeitAuswertung _StatusArbeitszeit = EnumStatusArbeitszeitAuswertung.Leer;
    public EnumStatusArbeitszeitAuswertung StatusArbeitszeit
    {
      get { return _StatusArbeitszeit; }
      set
      {
        if (value != _StatusArbeitszeit)
        {
          _StatusArbeitszeit = value;
          NotifyPropertyChanged();
        }
      }
    }

    public string Name { get { return this.NachName + ", " + VorName; } }
    public tabArbeitszeitAuswertung ErgebnisVorjahr { get; set; } = null;
  }

  public partial class tabReparatur
  { }

  public partial class tabAnmeldungReparatur
  {
    public bool IstAktiv { get { return this.Abmeldung == null; } }
  }

  public partial class tabStandortMetaData
  {
    [Required]
    [MinLength(5, ErrorMessage = "Mindestanzahl der Zeichen für den Standort ist {1}")]
    public object Bezeichnung;
  }

  [MetadataType(typeof(tabStandortMetaData))]
  public partial class tabStandort
  { }

  public partial class tabAuswertungMetaData
  {
    [Required]
    public object ReportName;
  }

  [MetadataType(typeof(tabAuswertungMetaData))]
  public partial class tabAuswertung
  { }

  public partial class tabBauteil
  {
    public double ZeitInSekunden
    {
      get
      {
        if (this.DatumEnde == null)
          return 0;
        else
          return ((DateTime)DatumEnde - DatumStart).TotalSeconds;
      }
    }

    public string Bediener
    {
      get
      {
        if (this.sBediener.Count == 0)
          return "";
        else
          return string.Join("; ", sBediener.Select(s => s.NachName + ", " + s.VorName).ToArray());
      }
    }
  }

  public partial class tabSollStunden
  {
    public string StundenAnzeige
    {
      get
      {
        return this.SollStunden ?? "00:00";
      }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
          SollStunden = zeit.AsString;
      }
    }
  }

  public partial class tabArbeitszeit
  {
    public TimeSpan Dauer { get { return ((this.Anmeldung != null) && (this.Abmeldung != null)) ? this.Abmeldung.Value - this.Anmeldung.Value : TimeSpan.Zero; } }

    public string DauerAnzeige { get { return  (Dauer == TimeSpan.Zero) ? "-" : ((int)Dauer.TotalHours).ToString("D2") + ":" + Dauer.Minutes.ToString("D2"); } }

    public bool AnmeldungIstLeer { get { return this.Anmeldung == null; } }

    public bool AbmeldungIstLeer { get { return this.Abmeldung == null; } }
  }

  public partial class tabArbeitszeitAuswertung
  {
    public string UeberstundenAnzeige
    {
      get { return this.Ueberstunden; }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
          this.Ueberstunden = zeit.AsString;
      }
    }
  }

  public partial class tabArbeitszeitTag : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public delegate void DelegateArbeitszeitTagGeaendert(tabArbeitszeitTag Sender, string PropertyName);
    public DelegateArbeitszeitTagGeaendert ArbeitszeitTagGeaendert { get; set; } = null;

    private void ArbeitszeitTagGeaendertAusloesen(string PropertyName)
    {
      ArbeitszeitTagGeaendert?.Invoke(this, PropertyName);
    }

    public string Wochentag { get; set; }

    public bool IstSonntag { get; set; } = false;
    public bool IstSonnabend { get; set; } = false;
    public bool IstFeiertag { get; set; } = false;
    public bool IstFehlerZeit { get; set; } = false;
    public bool IstFehlerNachtschicht { get; set; } = false;

    public bool IstZeitUngleich { get { return this.ZeitBerechnet != this.Zeit; } }
    public bool IstNachtschichtUngleich { get { return this.NachtschichtBerechnet != this.Nachtschicht; } }

    private string ZeitInString(TimeSpan ZeitWert)
    {
      return ((int)ZeitWert.TotalHours).ToString("D2") + ":" + (ZeitWert.Minutes < 0 ? -1 * ZeitWert.Minutes : ZeitWert.Minutes).ToString("D2");
    }

    public string PauseAnzeige
    {
      get { return ZeitInString(this.Pause); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.Pause = zeit.AsTime;
          ArbeitszeitTagGeaendertAusloesen("Pause");
          NotifyPropertyChanged("ZeitBerechnetString");
          NotifyPropertyChanged("IstZeitUngleich");
          NotifyPropertyChanged("NachtschichtBerechnetString");
          NotifyPropertyChanged("IstNachtschichtUngleich");
        }
      }
    }

    public string ZeitAnzeige
    {
      get { return ZeitInString(this.Zeit); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.Zeit = zeit.AsTime;
          ArbeitszeitTagGeaendertAusloesen("Zeit");
          NotifyPropertyChanged("IstZeitUngleich");
        }
      }
    }
    public TimeSpan ZeitBerechnet { get; set; } = TimeSpan.Zero;
    public string ZeitBerechnetString
    {
      get { return ZeitInString(ZeitBerechnet); }
    } 

    public string NachtschichtAnzeige
    {
      get { return ZeitInString(this.Nachtschicht); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.Nachtschicht = zeit.AsTime;
          ArbeitszeitTagGeaendertAusloesen("Nachtschicht");
          NotifyPropertyChanged("IstNachtschichtUngleich");
        }
      }
    }
    public TimeSpan NachtschichtBerechnet { get; set; } = TimeSpan.Zero;
    public string NachtschichtBerechnetString
    {
      get { return ZeitInString(NachtschichtBerechnet); }
    }

    public string FeiertagAnzeige
    {
      get { return ZeitInString(this.Feiertag); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.Feiertag = zeit.AsTime;
          ArbeitszeitTagGeaendertAusloesen("Feiertag");
        }
      }
    }

    public bool UrlaubAnzeige
    {
      get { return this.Urlaub; }
      set
      {
        if (value != this.Urlaub)
        {
          this.Urlaub = value;
          ArbeitszeitTagGeaendertAusloesen("Urlaub");
        }
      }
    }

    public bool KrankAnzeige
    {
      get { return this.Krank; }
      set
      {
        if (value != this.Krank)
        {
          this.Krank = value;
          ArbeitszeitTagGeaendertAusloesen("Krank");
        }
      }
    }

    public string Ueberstunden { get { return this.Zeit != TimeSpan.Zero ?  ZeitInString(this.Zeit + new TimeSpan(-8, 0, 0)) : "00:00"; } }
  }

  public class ZeitHelper
  {
    public bool IstOk = true;
    public int Stunde = 0;
    public int Minute = 0;

    public string AsString
    {
      get { return IstOk ? Stunde.ToString("D2") + ":" + (Minute < 0 ? -1 * Minute : Minute).ToString("D2") : null; }
    }

    public TimeSpan AsTime
    {
      get { return new TimeSpan(Stunde, Stunde < 0 ? -1 * Minute : Minute, 0); }
    }

    public ZeitHelper(string ZeitString, bool KontrolleZeit)
    {
      if (!string.IsNullOrWhiteSpace(ZeitString))
      {
        var werte = ZeitString.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (werte.Length > 0)
        {
          try
          {
            Stunde = Convert.ToInt32(werte[0]);
          }
          catch { IstOk = false; };

          if (IstOk && (werte.Length > 1))
          {
            try
            {
              Minute = Convert.ToInt32(werte[1]);
            }
            catch { IstOk = false; };
          }
        }

        if (KontrolleZeit)
        {
          var zeit = this.AsTime;
          if ((zeit < TimeSpan.Zero) || (zeit >= new TimeSpan(24, 0, 0)))
          {
            var msg = "Die Zeit muss zwischen 00:00 und 23:59 liegen !";
            MessageBox.Show(msg, "Fehler !", MessageBoxButton.OK);
            IstOk = false;
          }
        }
      }
    }
  }

  #region bei Speicherung Valedierungsfehler anzeigen
  
  public partial class JgModelContainer
  {
    private static string DbFehlerText(DbEntityValidationException ex)
    {
      // Retrieve the error messages as a list of strings.
      var errorMessages = ex.EntityValidationErrors
              .SelectMany(x => x.ValidationErrors)
              .Select(x => x.ErrorMessage);

      // Join the list to a single string.
      var fullErrorMessage = string.Join("; ", errorMessages);

      MessageBox.Show(fullErrorMessage);

      // Combine the original exception message with the new one.
      return string.Concat(ex.Message, " Fehler: ", fullErrorMessage);
    }

    public override int SaveChanges()
    {
      try
      {
        return base.SaveChanges();
      }
      catch (DbEntityValidationException ex)
      {
        // Throw a new DbEntityValidationException with the improved exception message.
        throw new DbEntityValidationException(DbFehlerText(ex), ex.EntityValidationErrors);
      }
    }

    public override Task<int> SaveChangesAsync()
    {
      try
      {
        return base.SaveChangesAsync();
      }
      catch (DbEntityValidationException ex)
      {
        // Throw a new DbEntityValidationException with the improved exception message.
        throw new DbEntityValidationException(DbFehlerText(ex), ex.EntityValidationErrors);
      }
    }
  }

  #endregion

}