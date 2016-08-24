using JgMaschineData;
using JgMaschineLib;
using JgMaschineLib.Email;

namespace JgMaschineServiceDatenabfrage.Maschinen
{
  public class OptioneAbfrage
  {
    public string PfadEvg { get; set; } = "";
    public string PfadSchnell { get; set; } = "";
    public string PfadProgress { get; set; } = "";

    public string DatenbankVerbindungsString { get; set; } = "";
    public Proto Protokoll { get; set; }
    public tabMaschine AktuelleMaschine { get; set; }
  }
}
