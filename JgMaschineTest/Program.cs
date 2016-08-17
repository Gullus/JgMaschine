using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using JgMaschineData;
using JgMaschineDatafoxLib;
using JgMaschineLib;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main(string[] args)
    {



      JgFastCube.JgFastCubeStart(Properties.Settings.Default.FastCubeConnection, "Select * From tabBauteilSet", @"C:\Entwicklung\JgMaschine\JgMaschineTest\FcOptionen");

      //using (var db = new JgMaschineData.JgModelContainer())
      //{
      //  var gui = Guid.NewGuid();

      //  var letzteArbeitszeit = db.tabArbeitszeitSet.Where(w => (w.fBediener == gui) && (w.Abmeldung != null)).Max(m => m.Abmeldung);

      //  Console.WriteLine(letzteArbeitszeit);
      //  Console.ReadLine();
      //}

      Console.ReadKey();

    }
  }


  public static class JgFastCube
  {
    [DllImport("JgFastCube.dll")]
    public static extern void JgFastCubeStart([MarshalAs(UnmanagedType.BStr)]string ConnectionString, [MarshalAs(UnmanagedType.BStr)] string SqlText, [MarshalAs(UnmanagedType.BStr)] string PfadOptione);

    [DllImport("JgFastCube.dll")]
    public static extern void MessageTest1();

    [DllImport("JgFastCube.dll")]
    public static extern void MessageTest2([MarshalAs(UnmanagedType.BStr)]string MeinText);
  }

  class SendEmail
  {
    public delegate void SendEmailErgebnissDelegate(string InfoText, bool Fehler = false);
    public SendEmailErgebnissDelegate SendErgebniss { get; set; } = null;

    public string AdresseAbsender { get; set; } = "JgSupport@gullus.de";
    public string AdressenEmpfaenger { get; set; } = "joerggullus@gmail.com";
    public string Betreff { get; set; } = "Betreff Test";
    public string Koerper { get; set; } = "<html><body>UtilMailMessage001 - success</body></html>";

    public string ServerAdresse { get; set; } = "smtp.1und1.de";
    public int ServerPort { get; set; } = 25;
    public string ServerBenutzername { get; set; } = "fehlermeldung@jgdata.de";
    public string ServerPasswort { get; set; } = "fehler123";

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


