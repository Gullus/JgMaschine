using System;
using System.Collections.Generic;
using JgMaschineData;
using System.Linq;

namespace JgMaschineServiceArbeitszeit
{

  public class OptionenArbeitszeit
  {
    internal List<tabBediener> ListeBediener;
    internal List<tabStandort> ListeStandorte;
    internal List<tabArbeitszeitTerminal> ListeTerminals;
    internal List<DatafoxDsImport> ListeAnmeldungen = new List<DatafoxDsImport>(); 

    public string VerbindungsString = "";
    public string PfadUpdateBediener = "";
    public int TimerIntervall = 10000;

    public int Terminal_TimeOut = 3000;

    internal bool UpdateBenutzerOk = false;
    internal bool DatumZeitAktualisieren = false;
    internal int ZaehlerDatumAktualisieren = 100; // Damit wird zeit beim Start aktualisiert

    public OptionenArbeitszeit()
    { }

    public string GetStandort(Guid IdStandort)
    {
      return ListeStandorte.First(f => f.Id == IdStandort).Bezeichnung;
    }
  }
}
