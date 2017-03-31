using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace JgMaschineData
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

    public Obergruppen Obergruppe { get; set; }

    public string ProjektNummer { get; set; } = null;
    public string PlanNummer { get; set; } = null;
    public string IndexPlan { get; set; } = null;
    public string Position { get; set; } = null;
    public string StahlGuete { get; set; } = null;
    public string MattenTyp { get; set; } = null;

    public int? Anzahl { get; set; } = null;
    public int? Laenge { get; set; } = null;        // mm
    public decimal? Gewicht { get; set; } = null;   // kg
    public int? Durchmesser { get; set; } = null;
    public int? BreiteMatte { get; set; } = null;
    public int? HoeheGittertraeger { get; set; } = null;
    public int? Biegerollendurchmesser { get; set; } = null;

    public string Lage { get; set; } = null;
    public string DeltaIfuerStaffeleisen { get; set; } = null;

    public int? PruefsummeAusBvsStream { get; set; } = null;

    private string _BvbsString = null;
    public string BvbsString
    {
      get { return _BvbsString; }
    }

    public List<BvbsGeometrie> ListeGeometrie = new List<BvbsGeometrie>();

    public BvbsDatenaustausch()
    { }

    public BvbsDatenaustausch(string NeuBvbsString, bool GeometrieDatenErstellen)
       : this()
    {
      _BvbsString = NeuBvbsString;
      BvsStringInObjet(GeometrieDatenErstellen);
    }

    public void BvsStringInObjet(bool GeometrieDatenErstellen)
    {
      string[] felder = _BvbsString.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);

      if (felder[0].Length != 4)
        throw new Exception(string.Format("Länge Obergruppenname {0} ist ungeleich 4", felder[0]));

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

        if (!GeometrieDatenErstellen && (block != Bloecke.Header))
          return;

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

        try
        {
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
                case 'e':
                  wert = wert.Replace(".", "");
                  Gewicht = Convert.ToInt32(wert); break;
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
        catch (Exception f)
        {
          var msg = $"Fehler bei der Konvertierung Bvbs Code !\nBlock: {block} Auswahl: {auswahl} Feld: {feld}\nWert: '{wert}'\nFehler: {f.Message}";
          throw new Exception(msg);
        }
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
      erg.Append(Gewicht != null ? 'e' + Convert.ToDecimal(Gewicht).ToString("N3", CultureInfo.InvariantCulture) + "@" : "0");
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

      foreach (var ds in ListeGeometrie)
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
