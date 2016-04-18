using System;
using System.Activities;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace JgMaschineWorkflow.Aktivitaeten
{

  public sealed class caMachineOnline : CodeActivity
  {
    public InArgument<string> ComputerAdresse { get; set; }
    public OutArgument<bool> IstFehler { get; set; }
    public OutArgument<string> ProtokollText { get; set; }

    protected override void Execute(CodeActivityContext context)
    {
      string computerName = context.GetValue(this.ComputerAdresse);
      bool istFehler = true;
      string protokollText = "";

      Ping pingSender = new Ping();
      PingOptions options = new PingOptions() { DontFragment = true };

      string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
      byte[] buffer = Encoding.ASCII.GetBytes(data);
      int timeout = 120;

      try
      {
        if (string.IsNullOrWhiteSpace(computerName))
          throw new Exception("Computeradresse ist leer !");

        PingReply reply = pingSender.Send(computerName, timeout, buffer, options);

        if (reply.Status == IPStatus.Success)
        {
          istFehler = false;
          protokollText = "Verbindung Ok\n"
            + string.Format("Address:        {0}", reply.Address.ToString()) + "\n"
            + string.Format("RoundTrip time: {0}", reply.RoundtripTime) + "\n"
            + string.Format("Buffer size:    {0}", reply.Buffer.Length);

          if (reply.Options != null)
            protokollText += string.Format("Time to live:   {0}", reply.Options.Ttl) + "\n"
              + string.Format("Don't fragment: {0}", reply.Options.DontFragment);
        }
        else
          protokollText = string.Format("Fehler Kontrolle Maschine Online: {0}", reply.Status);
      }
      catch (Exception f)
      {
        istFehler = true;
        protokollText = string.Format("Fehler beim ausführen des Pings !\n{0}\n{1}", f.Message, f.InnerException == null ? "" : f.InnerException.Message);
      }

      context.SetValue(this.IstFehler, istFehler);
      context.SetValue(this.ProtokollText, protokollText);
    }
  }
}
