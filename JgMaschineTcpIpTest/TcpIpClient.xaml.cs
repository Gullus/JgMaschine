using System;
using System.Net.Sockets;
using System.Threading;
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

    private void Button_Click(object sender, RoutedEventArgs e)
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
      tbRueckmeldung2.Text = JgMaschineLib.TcpIp.Helper.AnzeigeZeichen(tbRueckmeldung1.Text, _AnzeigeArtText);
    }

    private async void TextSenden(string SendeText)
    {
      var senden = JgMaschineLib.TcpIp.Helper.StringInBuffer(SendeText);

      await _NetworkStream.WriteAsync(senden, 0, senden.Length);
      Anzeige("Daten an Sever gesendet...");

      if (Properties.Settings.Default.ClientRueckantwortVomServer)
      {
        var buffer = new byte[4096];
        var anzahlZeichen = await _NetworkStream.ReadAsync(buffer, 0, buffer.Length);

        var zurueck = JgMaschineLib.TcpIp.Helper.BufferInString(buffer, anzahlZeichen);
        Anzeige(zurueck);
      }

    }

    private void btnClientBeenden_Click(object sender, RoutedEventArgs e)
    {
      if (_Client != null)
      {
        btnClientBeenden.Content = "Client starten";
        _Client.Close();
        _NetworkStream.Close();
        _Client = null;
        tbRueckmeldung1.Text = "Von Server getrennt.";
      }
      else
      {
        btnClientBeenden.Content = "Client beenden";
        _Client = new TcpClient(Properties.Settings.Default.ClientAdresseServer, Properties.Settings.Default.ClientPortServer);
        _NetworkStream = _Client.GetStream();
        tbRueckmeldung1.Text = "Mir Server verbunden...";
      }
    }

    private void RadioButton_Click(object sender, RoutedEventArgs e)
    {
      tbRueckmeldung2.Text = JgMaschineLib.TcpIp.Helper.AnzeigeZeichen(tbRueckmeldung1.Text, _AnzeigeArtText);
    }

    private void btnWarteAufAction_Click(object sender, RoutedEventArgs e)
    {
      using (var client = new TcpClient())
      {
        client.Connect(Properties.Settings.Default.ClientAdresseServer, Properties.Settings.Default.ClientPortServer);
        using (var networkStream = client.GetStream())
        {
          while (true)
          {
            var buffer = new byte[4096];
            var anzahlZeichen = networkStream.Read(buffer, 0, buffer.Length);
            MessageBox.Show(string.Format("{0} Daten vom Server empfangen. Text: {1} ...", anzahlZeichen, JgMaschineLib.TcpIp.Helper.BufferInString(buffer, anzahlZeichen)));
          }
        }
      }
    }
  }
}
