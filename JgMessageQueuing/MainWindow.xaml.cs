using System;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JgMessageQueuing
{
  public partial class MainWindow : Window
  {
    private StringBuilder _Text = new StringBuilder();
    private MessageQueue msgQ = null;


    public MainWindow()
    {
      InitializeComponent();
    }

    public void TextEintragen(string MeinText)
    {
      _Text.Insert(0, MeinText + "\n");
      tbAnzeige.Text = _Text.ToString();
    }

    private async void ButtonStart_Click(object sender, RoutedEventArgs e)
    {
      TextEintragen(" ---- Starte Abfrage ---- ");

      var pathQ = Properties.Settings.Default.PfadZuMessageQueung;
      msgQ = new MessageQueue(pathQ);
      msgQ.Formatter = new XmlMessageFormatter(new String[] { "System.String, mscorlib" });

      while (true)
      {
        var ss = await WarteAufNachricht(msgQ);
        TextEintragen(ss);
      };
    }

    private static Task<string> WarteAufNachricht(MessageQueue MyQueue)
    {
      var t = new Task<string>((myQueue) =>
      {
        var mq = (MessageQueue)myQueue;
        var myMessage = mq.Receive();
        return myMessage.Body.ToString();
      }, MyQueue);

      t.Start();
      return t;
    }

    private void ButtonPause_Click(object sender, RoutedEventArgs e)
    {
      msgQ.Close();
      TextEintragen("---- Pause ----");
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      msgQ.Close();
      Properties.Settings.Default.Save();
    }
  }
}
