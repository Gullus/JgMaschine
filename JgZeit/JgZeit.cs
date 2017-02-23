﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace JgZeitHelper
{
  public class JgZeit : INotifyPropertyChanged
  {
    public delegate void DelegateNeuerWert(DateTime Datum, TimeSpan Zeit);
    public DelegateNeuerWert OnNeuerWert = null;

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public enum Monate : byte { Januar = 1, Februar, März, April, Mai, Juni, Juli, August, Septemper, Oktober, November, Dezember }

    public enum KontrolleZeiten : byte { Keine, Positiv24 }

    private KontrolleZeiten KontrolleZeit { get; set; } = KontrolleZeiten.Keine;
    private DateTime Datum { get; set; } = DateTime.MinValue;
    private TimeSpan Zeit { get; set; } = TimeSpan.Zero;
    public bool IstOk { get; set; } = true;

    private DateTime DatumZeit
    {
      get { return Datum + Zeit; }
      set
      {
        Datum = value.Date;
        Zeit = DatumInZeit(value);
      }
    }

    public DateTime AnzeigeDatumZeit
    {
      get { return DatumZeit; }
      set
      {
        if (DatumZeit != value)
        {
          DatumZeit = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("AnzeigeDatum");
          NotifyPropertyChanged("AnzeigeZeit");
          NotifyPropertyChanged("AnzeigeZeit24");

          OnNeuerWert?.Invoke(Datum, Zeit);
        }
      }
    }

    public DateTime AnzeigeDatum
    {
      get { return Datum; }
      set
      {
        if (Datum != value)
        {
          Datum = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("AnzeigeDatumZeit");
          OnNeuerWert?.Invoke(Datum, Zeit);
        }
      }
    }

    public string AnzeigeZeit
    {
      get { return ZeitInString(Zeit); }
      set
      {
        var z = StringInZeit(value);
        if (z != Zeit)
        {
          Zeit = z;
          NotifyPropertyChanged();
          NotifyPropertyChanged("AnzeigeDatumZeit");
          NotifyPropertyChanged("AnzeigeZeit24");

          OnNeuerWert?.Invoke(Datum, Zeit);
        }
      }
    }

    public string AnzeigeZeit24
    {
      get { return ZeitInString(Zeit); }
      set
      {
        var z = StringInZeit(value);
        if (z != Zeit)
        {
          if ((z >= TimeSpan.Zero) && (z < new TimeSpan(24, 0, 0)))
          {
            Zeit = z;
            NotifyPropertyChanged("AnzeigeDatumZeit");
            NotifyPropertyChanged("AnzeigeZeit");

            OnNeuerWert?.Invoke(Datum, Zeit);
          }
          else
            MessageBox.Show("Zeit muss zwischen 0 und 24 Uhr liegen", "Warnung !", MessageBoxButton.OK, MessageBoxImage.Warning);
          NotifyPropertyChanged();
        }
      }
    }

    public JgZeit()
    { }

    public JgZeit(string ZeitString, KontrolleZeiten KontrolleZeit)
    {
      this.KontrolleZeit = KontrolleZeit;
      AnzeigeZeit = ZeitString;

      if (KontrolleZeit == KontrolleZeiten.Positiv24)
      {
        if ((Zeit < TimeSpan.Zero) || (Zeit >= new TimeSpan(24, 0, 0)))
        {
          var msg = "Die Zeit muss zwischen 00:00 und 23:59 liegen !";
          MessageBox.Show(msg, "Fehler !", MessageBoxButton.OK);
          IstOk = false;
        }
      }
    }

    public static TimeSpan StringInZeit(string ZeitString, TimeSpan? ZeitAlt = null)
    {
      if (!string.IsNullOrWhiteSpace(ZeitString))
      {
        var werte = ZeitString.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (werte.Length > 0)
        {
          try
          {
            var minute = 0;
            var stunde = Convert.ToInt32(werte[0]);
            if (werte.Length > 1)
              minute = stunde < 0 ? -1 * Convert.ToInt32(werte[1]) : Convert.ToInt32(werte[1]);
            return new TimeSpan(stunde, minute, 0);
          }
          catch { };
        }
      }
      if (ZeitAlt != null)
        return ZeitAlt.Value;
      else
        return TimeSpan.Zero;
    }

    public static TimeSpan StringInZeit24(string ZeitString, TimeSpan? ZeitAlt = null)
    {
      var zeit = StringInZeit(ZeitString, ZeitAlt);
      if ((zeit < TimeSpan.Zero) || (zeit >= new TimeSpan(24, 0, 0)))
      {
        MessageBox.Show("Die Zeit muss sich zwischen 0.00 und 24:00 Uhr befinden!", "Warnung !", MessageBoxButton.OK);
        if (ZeitAlt != null)
          return ZeitAlt.Value;
        return TimeSpan.Zero;
      }
      return zeit;
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

    public static decimal AufHalbeStundeRunden(TimeSpan Zeitwert)
    {
      var stunden = (int)Zeitwert.TotalHours;
      if ((Zeitwert.Minutes >= 15) && (Zeitwert.Minutes < 45))
        return Convert.ToDecimal(stunden + 0.5);
      else if ((Zeitwert.Minutes >= 45) && (Zeitwert.Minutes <= 59))
        return stunden + 1;
      return stunden;
    }

    public static TimeSpan DatumInZeit(DateTime DatumZeit)
    {
      return new TimeSpan(DatumZeit.Hour, DatumZeit.Minute, DatumZeit.Second);
    }

    public static DateTime ErsterImMonat(DateTime Datum)
    {
      return ErsterImMonat(Datum.Year, Datum.Month);
    }
    public static DateTime ErsterImMonat(int Jahr, int Monat)
    {
      return new DateTime(Jahr, Monat, 1);
    }

    public static DateTime LetzterImMonat(DateTime Datum)
    {
      return LetzerImMonat(Datum.Year, Datum.Month);
    }
    public static DateTime LetzerImMonat(int Jahr, int Monat)
    {
      return new DateTime(Jahr, Monat, DateTime.DaysInMonth(Jahr, Monat), 23, 59, 59);
    }
    public static string StringInStringZeit(string StringNeu, string StringAlt = null)
    {
      var zeitAlt = StringInZeit(StringAlt ?? "00:00");
      var zeit = StringInZeit(StringNeu, zeitAlt);
      return ZeitInString(zeit);
    }

    public static bool AbfrageZeit(string AnzeigeText, string Caption, ref DateTime Datum)
    {
      var fo = new Fenster.FormDatumZeit(Caption, AnzeigeText, Datum);
      if (fo.ShowDialog() ?? false)
      {
        Datum = fo.DatumZeit;
        return true;
      }
      else
        return false;
    }

    public static bool AbfrageZeit(string AnzeigeText, string Caption, ref DateTime DatumVon, ref DateTime DatumBis)
    {
      var fo = new Fenster.FormDatumZeitVonBis(Caption, AnzeigeText, DatumVon, DatumBis);
      if (fo.ShowDialog() ?? false)
      {
        DatumVon = fo.DatumZeitVon;
        DatumBis = fo.DatumZeitBis;
        return true;
      }
      else
        return false;
    }
  }
}