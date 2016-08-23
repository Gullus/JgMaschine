using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JgMaschineLib.Scanner
{
  public class ScannerOptionen
  {
    public Proto Protokoll = null;
    public string CradleIpAdresse = "";
    public string CradleTextAnmeldung = "";
    public int CradlePortNummer = 0;

    public string DbVerbindungsString  = "";

    public string EvgPfadProduktionsListe = "";
    public string EvgDateiProduktionsAuftrag = "";
    public string ProgressPfadProduktionsListe = "";
  }
}
