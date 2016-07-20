using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace JgMaschineLib
{
  class SendEmail
  {
    public delegate void SendEmailErgebnissDelegate(string InfoText, bool Fehler = false);
    public SendEmailErgebnissDelegate SendErgebniss { get; set; } = null;

    public string AdresseAbsender { get; set; }
    public string AdressenEmpfaenger { get; set; }
    public string Betreff { get; set; }
    public string Koerper { get; set; } // "<html><body>UtilMailMessage001 - success</body></html>";

    public string ServerAdresse { get; set; }
    public int ServerPort { get; set; } = 25;
    public string ServerBenutzername { get; set; }
    public string ServerPasswort { get; set; }

    private class TSendHelper
    {
      public MailMessage EmailMessage { get; set; }
      public SmtpClient EmailClient { get; set; }
      public SendEmailErgebnissDelegate EmailErgebniss { get; set; } = null;
    }

    private TSendHelper SendHelper = new TSendHelper();

    public SendEmail()
    { }

    public void Send()
    {
      MailMessage myMail = new MailMessage();
      try
      {
        myMail.From = new MailAddress(AdresseAbsender);

        var adrEmpfaenger = AdressenEmpfaenger.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var adr in adrEmpfaenger)
          myMail.To.Add(new MailAddress(adr));
        myMail.Priority = MailPriority.High;

        myMail.Subject = Betreff;
        myMail.IsBodyHtml = true;
        myMail.Body = Koerper;
      }
      catch (Exception f)
      {
        SendErgebniss?.Invoke($"Fehler Message: {f.Message}", true);
        return;
      }

      //MailAttachment myAttachment = new MailAttachment("c:\attach\attach1.txt", MailEncoding.Base64);
      //myMail.Attachments.Add(myAttachment);

      try
      {
        SendHelper.EmailMessage = myMail;
        SendHelper.EmailErgebniss = SendErgebniss;
        SendHelper.EmailClient = new SmtpClient(ServerAdresse, ServerPort);
        SendHelper.EmailClient.Credentials = new System.Net.NetworkCredential(ServerBenutzername, ServerPasswort);
      }
      catch (Exception f)
      {
        SendErgebniss?.Invoke($"Init Email: {f.Message}", true);
        return;
      }

      var task = Task.Factory.StartNew((Helper) =>
      {
        var helper = (TSendHelper)Helper;
        try
        {
          helper.EmailClient.Send(helper.EmailMessage);
          helper.EmailErgebniss?.Invoke("Email gesendet");
        }
        catch (Exception f)
        {
          helper.EmailErgebniss?.Invoke($"Fehler Email: {f.Message}", true);
        }

      }, SendHelper);
    }
  }
}
