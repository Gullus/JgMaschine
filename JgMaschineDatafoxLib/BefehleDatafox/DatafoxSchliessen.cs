using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JgMaschineDatafoxLib
{
  public static partial class ProgDatafox
  {
    /// <summary>
    /// Schliessen der Verbindung
    /// </summary>
    /// <param name="Optionen">Übertragungsoptionen</param>
    public static void DatafoxSchliessen(ZeitsteuerungDatafox Optionen)
    {
      DFComDLL.DFCComClose(Optionen.Datafox.ChannelId);
    }
  }
}
