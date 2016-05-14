using System;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

namespace JgMaschineTcpIpTest
{

  public partial class TcpIpClient : Window
  {
    private TcpClient _Client = null;
    private NetworkStream _NetworkStream = null;

    private JgMaschineLib.TcpIp.Helper.AnzeigeZeichenArt _AnzeigeArtText
    {
      get
      {
        var erg = JgMaschineLib.TcpIp.Helper.AnzeigeZeichenArt.Dezimal;
        if (rbHexadezimal.IsChecked ?? false)
          erg = JgMaschineLib.TcpIp.Helper.AnzeigeZeichenArt.Hexadezimal;
        return erg;
      }
      set
      {
        switch (value)
        {
          case JgMaschineLib.TcpIp.Helper.AnzeigeZeichenArt.Dezimal: rbDezimal.IsChecked = true; break;
          case JgMaschineLib.TcpIp.Helper.AnzeigeZeichenArt.Hexadezimal: rbHexadezimal.IsChecked = true; break;
        }
      }
    }

    public TcpIpClient()
    {
      InitializeComponent();
    }

    private void btnTextAnServerSenden_Click(object sender, RoutedEventArgs e)
    {
      string txtSenden = Properties.Settings.Default.ClientSendeText1;
      switch (Convert.ToByte((sender as Button).Tag))
      {
        case 2: txtSenden = Properties.Settings.Default.ClientSendeText2; break;
        case 3: txtSenden = Properties.Settings.Default.ClientSendeText3; break;
      }
      TextSenden(txtSenden);
    }


    private void Anzeige(string AnzeigeText, params object[] AnzeigeWerte)
    {
      tbRueckmeldung1.Text = string.Format(AnzeigeText, AnzeigeWerte) + System.Environment.NewLine + tbRueckmeldung1.Text;
      tbRueckmeldung2.Text = JgMaschineLib.TcpIp.Helper.AnzeigeZeichen(AnzeigeText, _AnzeigeArtText);
    }

    private async void TextSenden(string SendeText)
    {
      var senden = JgMaschineLib.TcpIp.Helper.StringInBuffer(SendeText);

      await _NetworkStream.WriteAsync(senden, 0, senden.Length);
      Anzeige("Daten an Sever gesendet...");
    }

    private async void btnClientStarten_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        _Client = new TcpClient(Properties.Settings.Default.ClientAdresseServer, Properties.Settings.Default.ClientPortServer);
        _NetworkStream = _Client.GetStream();
        Anzeige("Mir Server verbunden...");
      }
      catch (Exception f)
      {
        tbRueckmeldung1.Text += f.Message;
        return;
      }

      while (true)
      {
        var buffer = new byte[4096];
        var anzahlZeichen = await _NetworkStream.ReadAsync(buffer, 0, buffer.Length);
        Anzeige($"{anzahlZeichen} Zeichen vom Server empfangen.");
        Anzeige(JgMaschineLib.TcpIp.Helper.BufferInString(buffer, anzahlZeichen));
      }
    }

    private void btnClientBeenden_Click(object sender, RoutedEventArgs e)
    {
      _Client.Close();
      Anzeige("Von Server getrennt.");
    }
  }
}
