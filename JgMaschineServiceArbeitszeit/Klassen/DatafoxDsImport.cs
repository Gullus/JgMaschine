using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JgMaschineServiceArbeitszeit
{
  public class DatafoxDsImport
  {
    public enum EnumVorgang
    {
      Fehler,
      Komme,
      Gehen,
      Pause
    }

    public Guid IdStandort;
    public string Version;
    public string MatchCode;
    public DateTime Datum;
    public EnumVorgang Vorgang;
    public int GehGrund;
  }
}
