using JgMaschineData;
using JgMaschineLib;
using JgMaschineLib.Email;

namespace JgMaschineServiceDatenabfrage.Maschinen
{
  public class Optionen
  {
    public Proto Protokoll { get; set; }
    public tabMaschine AktuelleMaschine { get; set; }

    public Optionen()
    {
      var email = new SendEmailOptionen()
      {

      };

      Protokoll = new Proto(Proto.KategorieArt.ServiceDatenabfrage, email);
    }
  }
}
