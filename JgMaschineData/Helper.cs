using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using JgZeitHelper;

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
        DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
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
        var sollStunden = JgZeit.StringInZeit(SollStunden);
        var zeit = JgZeit.StringInZeit(value, sollStunden);
        if (zeit != sollStunden)
        {
          SollStunden = JgZeit.ZeitInString(zeit);
          DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }
  }

  public partial class tabArbeitszeit : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public DateTime? AnzeigeAnmeldung
    {
      get { return Anmeldung; }
      set
      {
        if (value != Anmeldung)
        {
          Anmeldung = value;
          ManuelleAnmeldung = true;
          NotifyPropertyChanged();
          NotifyPropertyChanged("ManuelleAnmeldung");
          NotifyPropertyChanged("AnmeldungIstLeer");
          NotifyPropertyChanged("AnmeldungGerundet");
          NotifyPropertyChanged("DauerGerundetAnzeige");
        }
      }
    }

    public DateTime? AnzeigeAbmeldung
    {
      get { return Abmeldung; }
      set
      {
        if (value != Abmeldung)
        {
          Abmeldung = value;
          ManuelleAbmeldung = true;
          NotifyPropertyChanged();
          NotifyPropertyChanged("ManuelleAbmeldung");
          NotifyPropertyChanged("AbmeldungIstLeer");
          NotifyPropertyChanged("DauerGerundetAnzeige");
        }
      }
    }

    public bool AnmeldungIstLeer { get { return this.Anmeldung == null; } }
    public bool AbmeldungIstLeer { get { return this.Abmeldung == null; } }

    public TimeSpan Dauer { get { return ((this.Anmeldung != null) && (this.Abmeldung != null)) ? this.Abmeldung.Value - this.Anmeldung.Value : TimeSpan.Zero; } }
    public string DauerAnzeige { get { return (Dauer == TimeSpan.Zero) ? "-" : ((int)Dauer.TotalHours).ToString("D2") + ":" + Dauer.Minutes.ToString("D2"); } }

    public Nullable<DateTime> AnmeldungGerundetWert { get; set; } = null;  
    public Nullable<DateTime> AnmeldungGerundet { get { return this.AnmeldungGerundetWert ?? this.Anmeldung; } }
    public TimeSpan DauerGerundet { get { return ((this.AnmeldungGerundet != null) && (this.Abmeldung != null)) ? this.Abmeldung.Value - this.AnmeldungGerundet.Value : TimeSpan.Zero; } }
    public string DauerGerundetAnzeige { get { return (DauerGerundet == TimeSpan.Zero) ? "-" : ((int)DauerGerundet.TotalHours).ToString("D2") + ":" + DauerGerundet.Minutes.ToString("D2"); } }

    public bool AnzeigeGeloescht
    {
      get { return this.DatenAbgleich.Geloescht; }
      set
      {
        if (value != DatenAbgleich.Geloescht)
        {
          DatenAbgleich.Geloescht = value;
          NotifyPropertyChanged();
        }
      }
    }
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
      get { return SollStunden; }
      set
      {
        var stunden = JgZeit.StringInStringZeit(value, SollStunden);
        if (value != SollStunden)
        {
          SollStunden = stunden;
          NotifyPropertyChanged();
        }
      }
    }

    public string NachtschichtenAnzeige
    {
      get { return this.Nachtschichten; }
      set
      {
        var zeit = JgZeit.StringInStringZeit(value, Nachtschichten);
        if (zeit != Nachtschichten)
        {
          this.Nachtschichten = zeit;
          NotifyPropertyChanged();
        }
      }
    }

    public decimal NachtschichtGerundet { get { return JgZeit.AufHalbeStundeRunden(JgZeit.StringInZeit(NachtschichtenAnzeige)); } }

    public string FeiertageAnzeige
    {
      get { return this.Feiertage; }
      set
      {
        var zeit = JgZeit.StringInStringZeit(value, Feiertage);
        if (zeit != Feiertage)
        {
          this.Feiertage = zeit;
          NotifyPropertyChanged();
        }
      }
    }
    public decimal FeiertageGerundet { get { return JgZeit.AufHalbeStundeRunden(JgZeit.StringInZeit(FeiertageAnzeige)); } }

    public string AuszahlungUeberstundenAnzeige
    {
      get { return this.AuszahlungUeberstunden; }
      set
      {
        var zeit = JgZeit.StringInStringZeit(value, AuszahlungUeberstunden);
        if (zeit != AuszahlungUeberstunden)
        {
          this.AuszahlungUeberstunden = zeit;
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
        var zeit = JgZeit.StringInStringZeit(value, Ueberstunden);
        if (zeit != Ueberstunden)
        {
          this.Ueberstunden = zeit;
          NotifyPropertyChanged();
          NotifyPropertyChanged("IstStunden");
        }
      }
    }


    public string IstStunden { get { return JgZeit.ZeitInString(JgZeit.ZeitStringAddieren(SollStunden, Ueberstunden)); } } 

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
      get { return JgZeit.ZeitInString(this.Pause); }
      set
      {
        var zeit = JgZeit.StringInZeit24(value, Pause);
        if (zeit != Pause)
        {
          this.Pause = zeit;
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
      get { return JgZeit.ZeitInString(this.Zeit); }
      set
      {
        var zeit = JgZeit.StringInZeit24(value, Zeit);
        if (zeit != Zeit)
        {
          this.Zeit = zeit;
          ArbeitszeitTagGeaendertAusloesen("Zeit");
          NotifyPropertyChanged();
          NotifyPropertyChanged("Ueberstunden");
          NotifyPropertyChanged("IstZeitUngleich");
        }
      }
    }

    public TimeSpan ZeitBerechnet { get; set; } = TimeSpan.Zero;

    public string ZeitBerechnetAnzeige { get { return JgZeit.ZeitInString(ZeitBerechnet); } }

    public string NachtschichtAnzeige
    {
      get { return JgZeit.ZeitInString(this.Nachtschicht); }
      set
      {
        var zeit = JgZeit.StringInZeit24(value, Nachtschicht);
        if (zeit != Nachtschicht)
        {
          this.Nachtschicht = zeit;
          NotifyPropertyChanged();
          ArbeitszeitTagGeaendertAusloesen("Nachtschicht");
          NotifyPropertyChanged("IstNachtschichtUngleich");
        }
      }
    }

    public TimeSpan NachtschichtBerechnet { get; set; } = TimeSpan.Zero;

    public string NachtschichtBerechnetAnzeige { get { return JgZeit.ZeitInString(NachtschichtBerechnet); } }

    public string FeiertagAnzeige
    {
      get { return JgZeit.ZeitInString(this.Feiertag); }
      set
      {
        var zeit = JgZeit.StringInZeit24(value, Feiertag);
        if (zeit != Feiertag)
        {
          this.Feiertag = zeit;
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
      get { return Zeit != TimeSpan.Zero ? JgZeit.ZeitInString(Zeit + new TimeSpan(-8, 0, 0)) : "00:00"; }
    }

    public IEnumerable<tabArbeitszeit> sArbeitszeitenNichtGeloescht { get { return this.sArbeitszeiten.Where(w => !w.DatenAbgleich.Geloescht); } }
  }

  public partial class tabArbeitszeitRunden : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public JgZeit.Monate MonatAnzeige
    {
      get { return (JgZeit.Monate)this.Monat; }
      set
      {
        this.Monat = (byte)value;
        NotifyPropertyChanged();
        DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      }
    }

    public string AnzeigeZeitVon
    {
      get { return JgZeit.ZeitInString(this.ZeitVon); }
      set
      {
        var zeit = JgZeit.StringInZeit(value, ZeitVon);
        if (zeit != ZeitVon)
        {
          this.ZeitVon = zeit;
          NotifyPropertyChanged();
          DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    public string AnzeigeZeitBis
    {
      get { return JgZeit.ZeitInString(this.ZeitBis); }
      set
      {
        var zeit = JgZeit.StringInZeit(value, ZeitBis);
        if (zeit != ZeitBis)
        {
          this.ZeitBis = zeit;
          NotifyPropertyChanged();
          DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    public string AnzeigeRundenAufZeit
    {
      get { return JgZeit.ZeitInString(this.RundenAufZeit); }
      set
      {
        var zeit = JgZeit.StringInZeit(value, RundenAufZeit);
        if (zeit != RundenAufZeit)
        {
          this.RundenAufZeit = zeit;
          NotifyPropertyChanged();
          DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
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
        DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
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
      get { return JgZeit.ZeitInString(this.ZeitVon); }
      set
      {
        var zeit = JgZeit.StringInZeit(value, this.ZeitVon);
        if (zeit != ZeitVon)
        {
          this.ZeitVon = zeit;
          NotifyPropertyChanged();
          DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    public string AnzeigeZeitBis
    {
      get { return JgZeit.ZeitInString(this.ZeitBis); }
      set
      {
        var zeit = JgZeit.StringInZeit(value, ZeitBis);
        if (zeit != ZeitBis)
        {
          this.ZeitBis = zeit;
          NotifyPropertyChanged();
          DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
        }
      }
    }

    public string AnzeigePausenzeit
    {
      get { return JgZeit.ZeitInString(this.Pausenzeit); }
      set
      {
        var zeit = JgZeit.StringInZeit(value, Pausenzeit);
        if (zeit != this.Pausenzeit)
        {
          this.Pausenzeit = zeit;
          NotifyPropertyChanged();
          DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
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
        DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
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
        DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      }
    }

    public string AnzeigeBezeichnung
    {
      get { return this.Bezeichnung; }
      set
      {
        this.Bezeichnung = value;
        DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      }
    }

    public bool AnzeigeGeloescht
    {
      get { return this.DatenAbgleich.Geloescht; }
      set
      {
        this.DatenAbgleich.Geloescht = value;
        DbHelper.AbgleichEintragen(this.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
      }
    }
  }

  public class DbHelper
  {
    public static void AbgleichEintragen(JgMaschineData.DatenAbgleich DatenAbgl, JgMaschineData.EnumStatusDatenabgleich Status)
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