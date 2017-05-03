using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public bool IstReparatur
    {
      get { return this.fAktivReparatur != null; }
      set { }
    }
  }

  public partial class tabAnmeldungMaschine : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public TimeSpan ZeitAngemeldet
    {
      get { return DateTime.Now - this.Anmeldung; }
    }

    public DateTime AnzeigeAnmeldung
    {
      get => Anmeldung;
      set
      {
        if (value != Anmeldung)
        {
          Anmeldung = value;
          NotifyPropertyChanged();
        }
      }
    }

    public DateTime? AnzeigeAbmeldung
    {
      get => Abmeldung;
      set
      {
        if (value != Abmeldung)
        {
          Abmeldung = value;
          NotifyPropertyChanged();
        }
      }
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
    public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string Name { get { return this.NachName + ", " + VorName; } }

    // in Optionen Anzeige DatenStart aus Vorjahr, sonst ArbeitszeitAuswertung Aktuell 

    public EnumStatusBediener StatusAnzeige
    {
      get { return this.Status; }
      set
      {
        if (value != this.Status)
        {
          this.Status = value;
          NotifyPropertyChanged();
        }
      }
    }

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
  }

  public partial class tabReparatur : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public DateTime AnzeigeVorgangBeginn
    {
      get => this.VorgangBeginn;
      set
      {
        if (value != this.VorgangBeginn)
        {
          this.VorgangBeginn = value;
          NotifyPropertyChanged();
        }
      }
    }

    public DateTime? AnzeigeVorgangEnde
    {
      get => this.VorgangEnde;
      set
      {
        if (value != this.VorgangEnde)
        {
          this.VorgangEnde = value;
          NotifyPropertyChanged();
        }
      }
    }

    public byte? AnzeigeCoilwechselAnzahl
    {
      get => this.CoilwechselAnzahl;
      set
      {
        if (value != this.CoilwechselAnzahl)
        {
          this.CoilwechselAnzahl = value;
          NotifyPropertyChanged();
        }
      }
    }

    public tabBediener AnzeigeVerursacher
    {
      get => this.eVerursacher;
      set
      {
        if (value != this.eVerursacher)
        {
          this.eVerursacher = value;
          NotifyPropertyChanged();
        }
      }
    }

    public tabBediener AnzeigeProtokollant
    {
      get => this.eProtokollant;
      set
      {
        if (value != this.eProtokollant)
        {
          this.eProtokollant = value;
          NotifyPropertyChanged();
        }
      }
    }

    public string AnzeigeProtokollText
    {
      get => this.ProtokollText;
      set
      {
        if (value != this.ProtokollText)
        {
          this.ProtokollText = value;
          NotifyPropertyChanged();
        }
      }
    }
  }

  public partial class tabAnmeldungReparatur : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool IstAktiv { get { return this.Abmeldung == null; } }

    public DateTime AnzeigeAnmeldung
    {
      get => Anmeldung;
      set
      {
        if (value != Anmeldung)
        {
          Anmeldung = value;
          NotifyPropertyChanged();
        }
      }
    }

    public DateTime? AnzeigeAbmeldung
    {
      get => Abmeldung;
      set
      {
        if (value != Abmeldung)
        {
          Abmeldung = value;
          NotifyPropertyChanged();
        }
      }
    }
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
    public BvbsDatenaustausch BvbsDaten { get; set; } = null;

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

    public void LoadBvbsDaten(bool GeometriedatenErstellen)
    {
      BvbsDaten = new BvbsDatenaustausch(this.BvbsCode, GeometriedatenErstellen);
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
          SollStunden = JgZeit.ZeitInString(zeit);
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
    public string DauerAnzeige { get { return (Dauer == TimeSpan.Zero) ? "-" : JgZeit.ZeitInString(Dauer); } }

    public DateTime? AnmeldungGerundetWert { get; set; } = null;
    public DateTime? AnmeldungGerundet
    {
      get
      {
        if (this.AnmeldungGerundetWert == null)
          return this.Anmeldung;
        return this.AnmeldungGerundetWert;
      }
    }
    public TimeSpan DauerGerundet { get { return ((this.AnmeldungGerundet != null) && (this.Abmeldung != null)) ? this.Abmeldung.Value - this.AnmeldungGerundet.Value : TimeSpan.Zero; } }
    public string DauerGerundetAnzeige { get { return (DauerGerundet == TimeSpan.Zero) ? "-" : JgZeit.ZeitInString(DauerGerundet); } }

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

    public TimeSpan NachtschichtZuschlaegeGerundet { get { return JgZeit.AufHalbeStundeRunden(JgZeit.StringInZeit(NachtschichtZuschlaege)); } }

    public TimeSpan FeiertagZuschlaegeGerundet { get { return JgZeit.AufHalbeStundeRunden(JgZeit.StringInZeit(FeiertagZuschlaege)); } }

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

    // Nur zur Anzeige in der Optionen 
    public string UeberstundenAnzeige
    {
      get => Ueberstunden;
      set
      {
        if (value != Ueberstunden)
        {
          Ueberstunden = JgZeit.StringInStringZeit(value, Ueberstunden);
          NotifyPropertyChanged();
        }
      }
    }
  }

  public partial class tabArbeitszeitTag : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public delegate void DelegateArbeitszeitTagGeaendert(tabArbeitszeitTag Sender, string PropertyName);

    public DelegateArbeitszeitTagGeaendert ArbeitszeitTagGeaendert = null;

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

    public bool IstZeitUngleich
    {
      get { return this.ZeitBerechnet != this.Zeit; }
    }

    public bool IstNachtschichtUngleich
    {
      get { return this.NachtschichtBerechnet != this.NachtschichtZuschlag; }
    }

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
          NotifyPropertyChanged("IstZeitUngleich");
        }
      }
    }

    public TimeSpan ZeitBerechnet { get; set; } = TimeSpan.Zero;

    public string ZeitBerechnetAnzeige
    {
      get { return JgZeit.ZeitInString(ZeitBerechnet); }
    }

    public string NachtschichtZuschlagAnzeige
    {
      get { return JgZeit.ZeitInString(this.NachtschichtZuschlag); }
      set
      {
        var zeit = JgZeit.StringInZeit24(value, NachtschichtZuschlag);
        if (zeit != NachtschichtZuschlag)
        {
          this.NachtschichtZuschlag = zeit;
          NotifyPropertyChanged();
          ArbeitszeitTagGeaendertAusloesen("NachtschichtZuschlag");
          NotifyPropertyChanged("IstNachtschichtUngleich");
        }
      }
    }

    public TimeSpan NachtschichtBerechnet { get; set; } = TimeSpan.Zero;

    public string NachtschichtBerechnetAnzeige
    {
      get { return JgZeit.ZeitInString(NachtschichtBerechnet); }
    }

    public string FeiertagZuschlagAnzeige
    {
      get { return JgZeit.ZeitInString(this.FeiertagZuschlag); }
      set
      {
        var zeit = JgZeit.StringInZeit24(value, FeiertagZuschlag);
        if (zeit != FeiertagZuschlag)
        {
          this.FeiertagZuschlag = zeit;
          ArbeitszeitTagGeaendertAusloesen("FeiertagZuschlag");
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

    public bool FeiertagAnzeige
    {
      get { return this.Feiertag; }
      set
      {
        if (value != this.Feiertag)
        {
          Feiertag = value;
          ArbeitszeitTagGeaendertAusloesen("Feiertag");
        }
      }
    }

    public string Ueberstunden
    {
      get { return Zeit != TimeSpan.Zero ? JgZeit.ZeitInString(Zeit + new TimeSpan(-8, 0, 0)) : "00:00"; }
    }

    public IEnumerable<tabArbeitszeit> sArbeitszeitenNichtGeloescht
    {
      get { return this.sArbeitszeiten.Where(w => !w.DatenAbgleich.Geloescht); }
    }
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
        }
      }
    }

    public string AnzeigeRundenBeginn
    {
      get { return JgZeit.ZeitInString(this.RundenArbeitszeitBeginn); }
      set
      {
        var zeit = JgZeit.StringInZeit(value, RundenArbeitszeitBeginn);
        if (zeit != RundenArbeitszeitBeginn)
        {
          this.RundenArbeitszeitBeginn = zeit;
          NotifyPropertyChanged();
        }
      }
    }

    public string AnzeigeRundenLaenge
    {
      get { return JgZeit.ZeitInString(this.RundenArbeitszeitLaenge.Value); }
      set
      {
        var zeit = JgZeit.StringInZeit(value, RundenArbeitszeitLaenge);
        if (zeit != RundenArbeitszeitLaenge)
        {
          this.RundenArbeitszeitLaenge = zeit;
          NotifyPropertyChanged();
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
      }
    }
  }

  public partial class tabArbeitszeitTerminalMetaData
  {
    [Required]
    [MinLength(3, ErrorMessage = "Mindestanzahl der Zeichen für den Vornamen ist {1}")]
    public object Bezeichnung;

    [Required]
    public object IpNummer;
  }

  [MetadataType(typeof(tabArbeitszeitTerminalMetaData))]
  public partial class tabArbeitszeitTerminal
  {
    // Wenn die Daten im Terminal erfolgreich aktualisiert wurden, kommt hier ein True und der Datensatz wird gespeichert.
    public bool TerminaldatenWurdenAktualisiert = false;

    // Muss so sein, da Fehlerzähler bei Addierung Status nicht als Modifiesd gekennzeichnet wird;
    public bool FehlerTerminalAusgeloest = false;
  }
}