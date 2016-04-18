using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace JgMaschineLib.TcpIp
{
  public class TcpIpClient : TcpIpStamm
  {
    public string Adresse { get; set; }

    public TcpIpClient(string Adresse, int Portnummer )
    {
      this.Adresse = Adresse;
      this.Port = Portnummer;
    }

    public async Task<string> SchreibenAsycn(string TextSenden)
    {
      using (var client = new TcpClient())
      {
        await client.ConnectAsync(Adresse, Port);
        using (var networkStream = client.GetStream())
        {
          var senden = Helper.StringInBuffer(TextSenden);
          await networkStream.WriteAsync(senden, 0, senden.Length);
          Console.WriteLine("Daten an Sever gesendet...");

          var buffer = new byte[4096];
          var anzahlZeichen = await networkStream.ReadAsync(buffer, 0, buffer.Length);
          Console.WriteLine("{0} Daten vom Server empfangen. Text: {1} ...", anzahlZeichen, Helper.BufferInString(buffer, anzahlZeichen));

          return Helper.BufferInString(buffer, anzahlZeichen);
        }
      }
    }

    public string Schreiben(string TextSenden, bool AufAntwortWarten = true)
    {
      try
      {
        using (var client = new TcpClient())
        {
          client.Connect(Adresse, Port);
          using (var networkStream = client.GetStream())
          {
            var senden = Helper.StringInBuffer(TextSenden);
            networkStream.Write(senden, 0, senden.Length);
            Console.WriteLine("Daten an Sever gesendet...");

            var buffer = new byte[4096];
            var anzahlZeichen = networkStream.Read(buffer, 0, buffer.Length);
            Console.WriteLine("{0} Daten vom Server empfangen. Text: {1} ...", anzahlZeichen, Helper.BufferInString(buffer, anzahlZeichen));

            return Helper.BufferInString(buffer, anzahlZeichen);
          }
        }
      }
      catch (Exception f)
      {
        MessageBox.Show("Fehler beim senden !\r\nGrund :", f.Message);
      }

      return "";
    }
  }
}
