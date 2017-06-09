using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JgMaschineServiceScanner
{
  public enum VorgangScanner
  {
    CRADDLEANMELDUNG,
    FEHLER,
    PROG,
    MITA,  // Mitarbeiter Anmeldung an Maschine  
    BF2D,
    BF3D,
    TEST,
    SCHALTER,
    VERBUNDEN
  }

  public enum VorgangProgram
  {
    CRADDLEANMELDUNG,
    FEHLER,
    BAUTEIL,
    ANMELDUNG,
    ABMELDUNG,
    COILSTART,
    REPASTART,
    WARTSTART,
    REPA_ENDE,
    SCHALTER,
    VERBUNDEN,
    TEST
  }
}
