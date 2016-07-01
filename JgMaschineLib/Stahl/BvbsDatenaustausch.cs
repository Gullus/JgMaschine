using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace JgMaschineLib.Stahl
{
  public class BvbsDatenaustausch
  {
    public enum Bloecke
    {
      Start,
      Header,
      Geometrie,
      Pruefsumme
    }

    public enum Obergruppen
    {
      BF2D, // ebene Biegeform
      BF3D, // räumliche Biegeform
      BFWE, // Wendeln/Spiralen
      BFMA, // Betonstahlmatten
      BFGT  // Gitterträger
    }

    private string _BvbsString = null;
    public string BvbsString
    {
      get { return _BvbsString; }
      set { _BvbsString = value; }
    }

    public Obergruppen Obergruppe { get; set; }

    public string ProjektNummer { get; set; }
    public string PlanNummer { get; set; }
    public string IndexPlan { get; set; }
    public string Position { get; set; }
    public string StahlGuete { get; set; }
    public string MattenTyp { get; set; }

    public int? Anzahl { get; set; }
    public int? Laenge { get; set; }         // mm
    public decimal? Gewicht { get; set; }    // kg
    public int GewichtInKg
    {
      get { return Convert.ToInt32(Gewicht * 1000); }
    }
    public int? Biegerollendurchmesser { get; set; }
    public int? Durchmesser { get; set; }
    public int? BreiteMatte { get; set; }
    public int? HoeheGittertraeger { get; set; }

    public string Lage { get; set; }
    public string DeltaIfuerStaffeleisen { get; set; }

    public int? PruefsummeAusBvsStream { get; set; } 

    public List<BvbsGeometrie> ListeGeometrie = new List<BvbsGeometrie>();

    public BvbsDatenaustausch()
    {
      ProjektNummer = null;
      PlanNummer = null;
      IndexPlan = null;
      Position = null;
      StahlGuete = null;
      MattenTyp = null;

      Anzahl = null;
      Laenge = null;                       // mm
      Gewicht = null;                      // kg
      Biegerollendurchmesser = null;
      Durchmesser = null;
      BreiteMatte = null;
      HoeheGittertraeger = null;

      Lage = null;
      DeltaIfuerStaffeleisen = null;

      PruefsummeAusBvsStream = null;
    }

    public BvbsDatenaustausch(string BvbsString)
       : this() 
    {
      _BvbsString = BvbsString;
      BvsStringInObjet();
    }

    public void BvsStringInObjet()
    {
      try
      {
        string[] felder = _BvbsString.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);

        if (felder[0].Length != 4)
          throw new Exception(string.Format("Obergruppenname {0} ist ungeleich 4", felder[0]));

        try
        {
          Obergruppe = (Obergruppen)Enum.Parse(typeof(Obergruppen), felder[0], true);
        }
        catch
        {
          throw new Exception(string.Format("Obergruppenname {0} nicht vorhanden", felder[0]));
        }

        Bloecke block = Bloecke.Start;
        char auswahl = ' ';
        string wert = "";
        bool neuerBlock = false;

        for (int dl = 1; dl < felder.Length; dl++)
        {
          var feld = felder[dl];

          if (feld.Contains(System.Environment.NewLine))
            return;

          neuerBlock = true;

          switch (feld[0])
          {
            case 'H': block = Bloecke.Header; break;
            case 'G': block = Bloecke.Geometrie; break;
            case 'C': block = Bloecke.Pruefsumme; break;
            default: neuerBlock = false; break;
          }

          if (neuerBlock)
          {
            auswahl = feld[1];
            wert = feld.Substring(2);
          }
          else
          {
            auswahl = feld[0];
            wert = feld.Substring(1);
          }

          switch (block)
          {
            case Bloecke.Header:
              switch (auswahl)
              {
                case 'j': ProjektNummer = wert; break;
                case 'r': PlanNummer = wert; break;
                case 'i': IndexPlan = wert; break;
                case 'p': Position = wert; break;
                case 'l': Laenge = Convert.ToInt32(wert); break;
                case 'n': Anzahl = Convert.ToInt32(wert); break;
                case 'e': Gewicht = Convert.ToDecimal(wert, CultureInfo.InvariantCulture); break;
                case 'd': Durchmesser = Convert.ToInt32(wert); break;
                case 'g': StahlGuete = wert; break;
                case 's': Biegerollendurchmesser = Convert.ToInt32(wert); break;
                case 'm': MattenTyp = wert; break;
                case 'b': BreiteMatte = Convert.ToInt32(wert); break;
                case 'h': HoeheGittertraeger = Convert.ToInt32(wert); break;
                case 'a': Lage = wert; break;
                case 't': DeltaIfuerStaffeleisen = wert; break;
              }
              break;
            case Bloecke.Geometrie:
              ListeGeometrie.Add(new BvbsGeometrie(auswahl, wert, felder[dl + 1].Substring(1)));
              dl++;
              break;
            case Bloecke.Pruefsumme:
              PruefsummeAusBvsStream = Convert.ToInt32(feld.Substring(1));
              break;
          }
        }
      }
      catch (Exception f)
      {
        throw new Exception(f.Message);
      }
    }
  
    public string WertErstellen(char Zeichen, object Wert)
    {
      return Wert != null ? Zeichen + Wert.ToString() + "@" : ""; 
    }

    public string ObjectInBvsStream()
    {
      var erg = new StringBuilder();

      erg.Append(Obergruppe.ToString() + "@");
      erg.Append("H");
      erg.Append(WertErstellen('j', ProjektNummer));
      erg.Append(WertErstellen('r', PlanNummer));
      erg.Append(WertErstellen('i', IndexPlan));
      erg.Append(WertErstellen('p', Position));
      erg.Append(WertErstellen('l', Laenge));
      erg.Append(WertErstellen('n', Anzahl));
      erg.Append(Gewicht != null ? 'e' + Convert.ToString(Gewicht, CultureInfo.InvariantCulture) + "@": "");
      erg.Append(WertErstellen('d', Durchmesser));
      erg.Append(WertErstellen('g', StahlGuete));
      erg.Append(WertErstellen('s', Biegerollendurchmesser));
      erg.Append(WertErstellen('m', MattenTyp));
      erg.Append(WertErstellen('b', BreiteMatte));
      erg.Append(WertErstellen('h', HoeheGittertraeger));
      erg.Append(WertErstellen('a', Lage));
      erg.Append(WertErstellen('t', DeltaIfuerStaffeleisen));

      erg.Append("v@");

      erg.Append("G");

      foreach(var ds in ListeGeometrie)
      {
        switch (ds.Geometrie)
        {
          case BvbsGeometrie.Koerper.Stab: erg.Append("l"); break;
          case BvbsGeometrie.Koerper.Bogen: erg.Append("r"); break;
          default: throw new Exception("Biegkörper in Geometrie nich vorhanden.");
        }

        erg.Append(ds.LaengeRadius.ToString() + "@w" + ds.Winkel.ToString() + "@");
      }

      erg.Append("C");
      erg.AppendLine(GetPruefsumme(erg.ToString()) + "@");

      return erg.ToString();
    }

    public static int GetPruefsumme(string PruefText)
    {
      int sum = 0;
      foreach (var c in PruefText)
        sum = sum + Convert.ToInt32(c);

      return 96 - (sum % 32);
    }
  }

  public class BvbsGeometrie
  {
    public enum Koerper
    {
      Unbekannt,
      Stab,
      Bogen
    }

    public Koerper Geometrie { get; set; }
    public int LaengeRadius { get; set; }
    public int Winkel { get; set; }

    public BvbsGeometrie()
    { }

    public BvbsGeometrie(char AuswahlGeomtrie, string FeldLaengeRadius, string FeldWinkel)
    {
      switch (AuswahlGeomtrie)
      {
        case 'l': Geometrie = Koerper.Stab; break;
        case 'r': Geometrie = Koerper.Bogen; break;
        default: Geometrie = Koerper.Unbekannt; break;
      }

      LaengeRadius = Convert.ToInt32(FeldLaengeRadius);
      Winkel = Convert.ToInt32(FeldWinkel);
    }

    public override string ToString()
    {
      var erg = 'F';
      switch (Geometrie)
      {
        case Koerper.Stab: erg = 'l'; break;
        case Koerper.Bogen: erg = 'r'; break;
      }
      return erg + Winkel.ToString() + '@';
    }
  }
}
