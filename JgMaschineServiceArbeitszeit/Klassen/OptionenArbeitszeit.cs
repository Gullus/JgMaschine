using System;
using System.Collections.Generic;
using JgMaschineData;
using JgMaschineLib.Arbeitszeit;

namespace JgMaschineServiceArbeitszeit
{
  public class OptionenArbeitszeit
  {
    internal List<tabBediener> ListeBediener;
    internal List<tabArbeitszeitTerminal> ListeTerminals;
    internal List<ArbeitszeitImportDaten> ListeAnmeldungen = new List<ArbeitszeitImportDaten>(); 

    public string VerbindungsString = "";
    public string PfadUpdateBediener = "";
    public int Terminal_TimeOut = 3000;
    public int AuslesIntervallTerminal = 10000;


    internal bool UpdateBenutzerAusfuehren = false;

    internal Int16 AnzahlBisFehlerAusloesen = 5;
    internal bool ErsterDurchlauf = true;          // Fehler bei Neustart zurücksetzen

    public OptionenArbeitszeit()
    { }

    // Damit Zeit nicht bei jedem Durchlauf aktualisiert wird

    internal int ZaehlerDatum = 9;
    internal bool DatumZeitAktualisieren { get { return ZaehlerDatum == 10; } }

    public void ZaehlerZeitErhoehen()
    {
      if (ZaehlerDatum > 10)
        ZaehlerDatum = 0;
      else
        ++ZaehlerDatum;
    }
  }
}
