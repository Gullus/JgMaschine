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
 
    public string Name { get { return this.NachName + ", " + VorName; } }

    // in Optionen Anzeige DatenStart aus Vorjahr, sonst ArbeitszeitAuswertung Aktuell 

    private tabArbeitszeitAuswertung fArbeitszeitHelper = null;
    public tabArbeitszeitAuswertung eArbeitszeitHelper
    {
      get { return fArbeitszeitHelper; }
      set
      {
        if (fArbeitszeitHelper != value)
        {
          fArbeitszeitHelper = value;
          NotifyPropertyChanged();
        }
      }
    }

    public byte AnzeigeUrlaubstage
    {
      get { return this.Urlaubstage; }
      set
      {
        this.Urlaubstage = value;
        ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      }
    }
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
  public partial class tabAuswertung : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string AnzeigeReportname
    {
      get { return this.ReportName; }
      set
      {
        this.ReportName = value;
        NotifyPropertyChanged();
      }
    }
  }

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
    public string AnzeigeStunden
    {
      get
      {
        return this.SollStunden ?? "00:00";
      }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          SollStunden = zeit.AsString;
          ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }
  }

  public partial class tabArbeitszeit
  {
    public bool AnmeldungIstLeer { get { return this.Anmeldung == null; } }
    public bool AbmeldungIstLeer { get { return this.Abmeldung == null; } }

    public TimeSpan Dauer { get { return ((this.Anmeldung != null) && (this.Abmeldung != null)) ? this.Abmeldung.Value - this.Anmeldung.Value : TimeSpan.Zero; } }
    public string DauerAnzeige { get { return (Dauer == TimeSpan.Zero) ? "-" : ((int)Dauer.TotalHours).ToString("D2") + ":" + Dauer.Minutes.ToString("D2"); } }

    public Nullable<DateTime> AnmeldungGerundetWert { get; set; } = null;  
    public Nullable<DateTime> AnmeldungGerundet { get { return this.AnmeldungGerundetWert ?? this.Anmeldung; } }
    public TimeSpan DauerGerundet { get { return ((this.AnmeldungGerundet != null) && (this.Abmeldung != null)) ? this.Abmeldung.Value - this.AnmeldungGerundet.Value : TimeSpan.Zero; } }
    public string DauerGerundetAnzeige { get { return (DauerGerundet == TimeSpan.Zero) ? "-" : ((int)DauerGerundet.TotalHours).ToString("D2") + ":" + DauerGerundet.Minutes.ToString("D2"); } }
  }

  public partial class tabArbeitszeitAuswertung : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string SollstundenAnzeige
    {
      get { return this.SollStunden; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this.SollStunden = zeit.AsString;
          NotifyPropertyChanged();
        }
      }
    }

    public string NachtschichtenAnzeige
    {
      get { return this.Nachtschichten; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this.Nachtschichten = zeit.AsString;
          NotifyPropertyChanged();
        }
      }
    }

    public string FeiertageAnzeige
    {
      get { return this.Feiertage; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this.Feiertage = zeit.AsString;
          NotifyPropertyChanged();
        }
      }
    }

    public string AuszahlungUeberstundenAnzeige
    {
      get { return this.AuszahlungUeberstunden; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this.AuszahlungUeberstunden = zeit.AsString;
          NotifyPropertyChanged();
        }
      }
    }

    public short UrlaubAnzeige
    {
      get { return this.Urlaub; }
      set
      {
        this.Urlaub = value;
        NotifyPropertyChanged();
      }
    }

    public short KrankAnzeige
    {
      get { return this.Krank; }
      set
      {
        this.Krank = value;
        NotifyPropertyChanged();
      }
    }

    public string UeberstundenAnzeige
    {
      get { return this.Ueberstunden; }
      set
      {
        var zeit = new ZeitHelper(value, false);
        if (zeit.IstOk)
        {
          this.Ueberstunden = zeit.AsString;
          NotifyPropertyChanged();
          NotifyPropertyChanged("IstStunden");
        }
      }
    }

    public string IstStunden { get { return ZeitHelper.ZeitInString(ZeitHelper.ZeitStringAddieren(SollStunden, Ueberstunden)); } } 

    public EnumStatusArbeitszeitAuswertung StatusAnzeige
    {
      get { return Status; }
      set
      {
        if (value != Status)
        {
          Status = value;
          NotifyPropertyChanged();
        }
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

    public string PauseAnzeige
    {
      get { return ZeitHelper.ZeitInString(this.Pause); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.Pause = zeit.AsTime;
          ArbeitszeitTagGeaendertAusloesen("Pause");
          NotifyPropertyChanged();
          NotifyPropertyChanged("ZeitBerechnetAnzeige");
          NotifyPropertyChanged("IstZeitUngleich");
          NotifyPropertyChanged("IstPauseUngleich");
        }
      }
    }

    public bool IstPauseUngleich
    {
      get { return PauseBerechnet != Pause; }
    }

    public TimeSpan PauseBerechnet { get; set; } = TimeSpan.Zero;

    public string ZeitAnzeige
    {
      get { return ZeitHelper.ZeitInString(this.Zeit); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.Zeit = zeit.AsTime;
          ArbeitszeitTagGeaendertAusloesen("Zeit");
          NotifyPropertyChanged();
          NotifyPropertyChanged("Ueberstunden");
          NotifyPropertyChanged("IstZeitUngleich");
        }
      }
    }

    public TimeSpan ZeitBerechnet { get; set; } = TimeSpan.Zero;

    public string ZeitBerechnetAnzeige { get { return ZeitHelper.ZeitInString(ZeitBerechnet); } }

    public string NachtschichtAnzeige
    {
      get { return ZeitHelper.ZeitInString(this.Nachtschicht); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.Nachtschicht = zeit.AsTime;
          NotifyPropertyChanged();
          ArbeitszeitTagGeaendertAusloesen("Nachtschicht");
          NotifyPropertyChanged("IstNachtschichtUngleich");
        }
      }
    }

    public TimeSpan NachtschichtBerechnet { get; set; } = TimeSpan.Zero;

    public string NachtschichtBerechnetAnzeige { get { return ZeitHelper.ZeitInString(NachtschichtBerechnet); } }

    public string FeiertagAnzeige
    {
      get { return ZeitHelper.ZeitInString(this.Feiertag); }
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

    public string Ueberstunden
    {
      get
      {
        return Zeit != TimeSpan.Zero ? ZeitHelper.ZeitInString(Zeit + new TimeSpan(-8, 0, 0)) : "00:00";
      }
    }
  }

  public partial class tabArbeitszeitRunden : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ZeitHelper.Monate MonatAnzeige
    {
      get { return (ZeitHelper.Monate)this.Monat; }
      set
      {
        this.Monat = (byte)value;
        NotifyPropertyChanged();
        ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      }
    }

    public string AnzeigeZeitVon
    {
      get { return ZeitHelper.ZeitInString(this.ZeitVon); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.ZeitVon = zeit.AsTime;
          NotifyPropertyChanged();
          ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    public string AnzeigeZeitBis
    {
      get { return ZeitHelper.ZeitInString(this.ZeitBis); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.ZeitBis = zeit.AsTime;
          NotifyPropertyChanged();
          ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    public string AnzeigeRundenAufZeit
    {
      get { return ZeitHelper.ZeitInString(this.RundenAufZeit); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.RundenAufZeit = zeit.AsTime;
          NotifyPropertyChanged();
          ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    public bool AnzeigeGeloescht
    {
      get { return this.DatenAbgleich.Geloescht; }
      set
      {
        this.DatenAbgleich.Geloescht = value;
        NotifyPropertyChanged();
        ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      }
    }
  }

  public partial class tabPausenzeit : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string AnzeigeZeitVon
    {
      get { return ZeitHelper.ZeitInString(this.ZeitVon); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.ZeitVon = zeit.AsTime;
          NotifyPropertyChanged();
          ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    public string AnzeigeZeitBis
    {
      get { return ZeitHelper.ZeitInString(this.ZeitBis); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.ZeitBis = zeit.AsTime;
          NotifyPropertyChanged();
          ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    public string AnzeigePausenzeit
    {
      get { return ZeitHelper.ZeitInString(this.Pausenzeit); }
      set
      {
        var zeit = new ZeitHelper(value, true);
        if (zeit.IstOk)
        {
          this.Pausenzeit = zeit.AsTime;
          NotifyPropertyChanged();
          ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    public bool AnzeigeGeloescht
    {
      get { return this.DatenAbgleich.Geloescht; }
      set
      {
        this.DatenAbgleich.Geloescht = value;
        NotifyPropertyChanged();
        ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      }
    }
  }

  public partial class tabFeiertage
  {
    public DateTime AnzeigeDatum
    {
      get { return this.Datum; }
      set
      {
        this.Datum = value;
        ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      }
    }

    public string AnzeigeBezeichnung
    {
      get { return this.Bezeichnung; }
      set
      {
        this.Bezeichnung = value;
        ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      }
    }

    public bool AnzeigeGeloescht
    {
      get { return this.DatenAbgleich.Geloescht; }
      set
      {
        this.DatenAbgleich.Geloescht = value;
        ZeitHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      }
    }
  }

  public class ZeitHelper
  {
    public enum Monate : byte { Januar = 1, Februar, März, April, Mai, Juni, Juli, August, Septemper, Oktober, November, Dezember }

    public bool IstOk = true;
    public int Stunde = 0;
    public int Minute = 0;

    public string AsString
    {
      get
      {
        if (IstOk)
        {
          var zeit = Stunde.ToString("D2") + ":" + (Minute < 0 ? -1 * Minute : Minute).ToString("D2");
          if ((Stunde == 0) && (Minute < 0))
            zeit = "-" + zeit;
          return zeit;
        }
        return "00:00";
      }
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

    public static TimeSpan StringInZeit(string ZeitString)
    {
      if (!string.IsNullOrWhiteSpace(ZeitString))
      {
        var werte = ZeitString.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (werte.Length > 0)
        {
          try
          {
            var stunde = Convert.ToInt32(werte[0]);
            var minute = Convert.ToInt32(werte[1]);
            return new TimeSpan(stunde, minute, 0);
          }
          catch { };
        }
      }
      return TimeSpan.Zero;
    }

    public static string ZeitInString(TimeSpan ZeitWert)
    {
      var stunde = (int)ZeitWert.TotalHours;
      var zeit = stunde.ToString("D2") + ":" + (ZeitWert.Minutes < 0 ? -1 * ZeitWert.Minutes : ZeitWert.Minutes).ToString("D2");
      if ((stunde == 0) && (ZeitWert.Minutes < 0))
        zeit = "-" + zeit;
      return zeit;
    }

    public static TimeSpan ZeitStringAddieren(params string[] ZeitString)
    {
      var erg = TimeSpan.Zero;
      foreach (var zeit in ZeitString)
        erg += StringInZeit(zeit);
      return erg;
    }

    public static void AbgleichEintragen(DatenAbgleich DatenAbgl, EnumStatusDatenabgleich Status)
    {
      DatenAbgl.Status = Status;
      DatenAbgl.Datum = DateTime.Now;
      DatenAbgl.Bearbeiter = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
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