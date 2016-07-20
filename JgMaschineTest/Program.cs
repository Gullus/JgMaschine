using JgMaschineData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Data.Entity;
using System.IO;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using JgMaschineLib.Scanner;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Management;
using System.Net;
using System.Net.Mail;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main(string[] args)
    {

      Senden();

      Console.ReadKey();



      //var networkPath = @"\\gullus-server\Sicherungen";

      //var fileList = Directory.GetFiles(networkPath);

      //foreach (var file in fileList)
      //{
      //  Console.WriteLine("{0}", Path.GetFileName(file));
      //}

      //var credentials = new NetworkCredential("jg", "pezdx65");
      //try
      //{
      //  using (new JgMaschineLib.NetzwerkVerbindung(networkPath, credentials))
      //  {
      //    fileList = Directory.GetFiles(networkPath);

      //    foreach (var file in fileList)
      //    {
      //      Console.WriteLine("{0}", Path.GetFileName(file));
      //    }
      //  }
      //}
      //catch (Exception f)
      //{
      //  Console.WriteLine(f.Message);
      //}

      //string s = "Hallo\nBallo";
      //Console.WriteLine(s);

      //Console.ReadKey();


      //var p = Properties.Settings.Default;
      //string bvbs = "BF2D@HjTest14@r417@ia@p1@l1000@n10@e0.888@d12@g500S@s48@v@Gl400@w90@l600@w0@C88@";

      //try
      //{
      //  BenutzerAnmeldung.Anmeldung(p.BenutzerName, p.BenutzerKennwort, p.MaschinenAdresse, p.MaschinenPfad, bvbs);
      //}
      //catch (Exception f)
      //{
      //  Console.WriteLine(f.Message);
      //}

      //var dat = @"c\Progress\Pro2\impdata\Auftrag.txt";      //impdata
      //var domaene = p.MaschinenAdresse;

      //Console.WriteLine($"Maschinenadesse: {domaene}");

      //dat = string.Format(@"\\{0}\{1}", domaene, dat);

      //Console.WriteLine($"Programmpfad: {dat}");

      //try
      //{
      //  File.WriteAllText(dat, bvbs, Encoding.UTF8);
      //}
      //catch (Exception f)
      //{
      //  Console.WriteLine(f.Message);
      //}
      //Console.ReadKey();
    }


    public static void Senden()
    {
      var em = new SendEmail();
      em.SendErgebniss = (info, istFehler) =>
      {
        Console.WriteLine(info);
      };

      Console.WriteLine("Email Senden");
      em.Send();
      Console.WriteLine("nach send");
    }
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
        foreach(var adr in adrEmpfaenger)
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


