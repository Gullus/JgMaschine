using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JgMaschineTcpIpTest
{
    public partial class MainWindow : Window
    {
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

        private TcpClient _Client;
        private NetworkStream _Stream;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnNeuerClient_Click(object sender, RoutedEventArgs e)
        {
            var client = new TcpIpClient();
            client.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private async void btnServerStarten_Click(object sender, RoutedEventArgs e)
        {
            string hostName = Dns.GetHostName();
            var hostIp = JgMaschineLib.TcpIp.Helper.GetIpAdressV4(Dns.GetHostName());

            tbAdresseName.Text = hostName;
            tbAdresseIp.Text = hostIp.ToString();

            try
            {
                var _Listener = new TcpListener(hostIp, Properties.Settings.Default.ServerPort);
                _Listener.Start(200);

                Anzeige("Server gestartet.");
                _Client = await _Listener.AcceptTcpClientAsync();

                Anzeige("Client verbunden ...");

                _Stream = _Client.GetStream();
                var buffer = new byte[4096];
                var anzahlEmpfangen = await _Stream.ReadAsync(buffer, 0, buffer.Length);
                if (anzahlEmpfangen > 0)
                {
                    var empfangen = JgMaschineLib.TcpIp.Helper.BufferInString(buffer, anzahlEmpfangen);
                    Anzeige(string.Format("{0} Zeichen von Client,  Text: {1}", anzahlEmpfangen, empfangen));
                }

                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ServerSendeTextAutomatisch))
                {
                    var senden = JgMaschineLib.TcpIp.Helper.StringInBuffer(Properties.Settings.Default.ServerSendeTextAutomatisch);
                    await _Stream.WriteAsync(senden, 0, senden.Length);
                }
            }
            catch (Exception f)
            {
                Anzeige("Serverfehler " + f.Message);
            }
        }

        private void Anzeige(string AnzeigeText)
        {
            //if (!tbRueckmeldung1.Dispatcher.CheckAccess())
            //{
            //  this.Dispatcher.Invoke((Action<string>)Anzeige, AnzeigeText);
            //  return;
            //}

            tbRueckmeldung1.Text = AnzeigeText + System.Environment.NewLine + tbRueckmeldung1.Text;
            tbRueckmeldung2.Text = JgMaschineLib.TcpIp.Helper.AnzeigeZeichen(AnzeigeText, _AnzeigeArtText);
        }

        private async void ServerStarten()
        {
            string hostName = Dns.GetHostName();
            var hostIp = JgMaschineLib.TcpIp.Helper.GetIpAdressV4(Dns.GetHostName());

            tbAdresseName.Text = hostName;
            tbAdresseIp.Text = hostIp.ToString();

            var _Listener = new TcpListener(hostIp, Properties.Settings.Default.ServerPort);

            _Listener.Start(200);
            Anzeige("Server gestartet.");

            while (true)
            {
                try
                {
                    var tcpClient = await _Listener.AcceptTcpClientAsync();
                    Anzeige("Client verbunden ...");

                    var task = Task.Factory.StartNew(client =>
                      {
                          try
                          {
                              using (var networkStream = tcpClient.GetStream())
                              {
                                  while (true)
                                  {
                                      var buffer = new byte[4096];
                                      var anzahlEmpfangen = networkStream.Read(buffer, 0, buffer.Length);
                                      if (anzahlEmpfangen > 0)
                                      {
                                          var empfangen = JgMaschineLib.TcpIp.Helper.BufferInString(buffer, anzahlEmpfangen);
                                          Anzeige(string.Format("{0} Zeichen von Client,  Text: {1}", anzahlEmpfangen, empfangen));
                                      }

                                      if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ServerSendeTextAutomatisch))
                                      {
                                          var senden = JgMaschineLib.TcpIp.Helper.StringInBuffer(Properties.Settings.Default.ServerSendeTextAutomatisch);
                                          networkStream.Write(senden, 0, senden.Length);
                                      }
                                  }
                              }
                          }
                          catch
                          {
                              Anzeige("Verbindung Close");
                          }
                      }, tcpClient);
                }
                catch (Exception f)
                {
                    Anzeige("Serverfehler " + f.Message);
                }
            }
        }

        private async void ServerSendText(object sender, RoutedEventArgs e)
        {
            string s = "";
            switch (Convert.ToByte((sender as Button).Tag))
            {
                case 1: s = Properties.Settings.Default.ServerSendeText1; break;
                case 2: s = Properties.Settings.Default.ServerSendeText2; break;
                case 3: s = Properties.Settings.Default.ServerSendeText3; break;
            }

            var senden = JgMaschineLib.TcpIp.Helper.StringInBuffer(s);
            await _Stream.WriteAsync(senden, 0, senden.Length);

            var buffer = new byte[4096];
            var anzahlEmpfangen = await _Stream.ReadAsync(buffer, 0, buffer.Length);
            if (anzahlEmpfangen > 0)
            {
                var empfangen = JgMaschineLib.TcpIp.Helper.BufferInString(buffer, anzahlEmpfangen);
                Anzeige(string.Format("{0} Zeichen von Client,  Text: {1}", anzahlEmpfangen, empfangen));
            }
        }
    }
}
