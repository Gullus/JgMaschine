using System;
using System.Linq;
using System.Net;
using System.Text;

namespace JgMaschineLib.TcpIp
{
  public static class Helper
  {
    public enum AnzeigeZeichenArt
    {
      String,
      Dezimal,
      Hexadezimal,
    }

    public static string AnzeigeZeichen(string Wert, AnzeigeZeichenArt AnzeigeArt)
    {
      StringBuilder sb = new StringBuilder();
      foreach (char c in Wert)
      {
        switch (AnzeigeArt)
        {
          case AnzeigeZeichenArt.Dezimal:
            sb.Append(string.Format("{0} ", Convert.ToInt32(c)));
            break;
          case AnzeigeZeichenArt.Hexadezimal:
            sb.Append(string.Format("{0:X} ", Convert.ToInt32(c)));
            break;
          case AnzeigeZeichenArt.String:
            if (Convert.ToInt32(c) < 32)
              sb.Append(string.Format("{0} ", Convert.ToInt32(c)));
            else
              sb.Append(c);
            break;
          default:
            break;
        }
      }

      return sb.ToString();
    }

    public static IPAddress GetIpAdressV4(string IpName)
    {
      IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
      return host.AddressList.FirstOrDefault(z => z.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
    }

    public static byte[] StringInBuffer(string Wert)
    {
      while (Wert.Contains(@"\x"))
      {
        var ergGesamt = Wert.Substring(Wert.IndexOf(@"\x"), 4);
        var hexValue = ergGesamt.Substring(2, 2);
        var erg = Int32.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
        Wert = Wert.Replace(ergGesamt, Convert.ToChar(erg).ToString());
      }

      return Encoding.ASCII.GetBytes(Wert);
    }

    public static string BufferInString(byte[] Buffer, int AnzahlZeichen)
    {
      return Encoding.ASCII.GetString(Buffer, 0, AnzahlZeichen);
    }
  }
}
