using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using JgMaschineData;
using JgMaschineDatafoxLib;
using JgMaschineLib;

namespace JgMaschineTest
{
  class Programm
  {
    static void Main(string[] args)
    {
      string skkk = "0.888";
      Console.WriteLine(skkk.Replace(".", ""));
      Console.WriteLine(Convert.ToInt32(skkk.Replace(".", "")));



      Console.ReadLine();

      return;

      var pfadStart = @"C:\Entwicklung\JgMaschine\Versuchsdaten\Schnell";

      Helper.SetzeBackflash(ref pfadStart);

      var dateienAuswertung = Directory.EnumerateFiles(pfadStart, "*.xml", SearchOption.AllDirectories).Select(s => s.ToUpper()).ToList();

      var heute = DateTime.Now.Date;
      var durchlauf = new DateTime(2015, 8, 1);

      while (durchlauf <= heute)
      {
        var datName = @"\" + durchlauf.ToString("yyyyMMdd") + "_PRODUZIONE.XML";
        var datei = dateienAuswertung.FirstOrDefault(f => f.Contains(datName));

        if (datei != null)
        {
          XDocument xdoc = XDocument.Load(datei);

          XElement root = xdoc.Root;
          var dataName = "{" + root.GetDefaultNamespace().NamespaceName + "}" + "DATA";
          XElement rootStart = root.Elements().FirstOrDefault(z => z.Name == dataName);

          //int startZeile = 0;
          //ErgebnisAbfrage ergNeu = null;

          Console.WriteLine("Datei: "  + datei +  "    " +  Path.GetFileName(datei));


          foreach (var t in rootStart.Elements())
          {
            var tempString = t.Attribute("date").Value;
            var datum = new DateTime(Convert.ToInt32(tempString.Substring(0, 4)), Convert.ToInt32(tempString.Substring(4, 2)), Convert.ToInt32(tempString.Substring(6, 2)));

            Console.WriteLine("Datum: " +  t.Attribute("date").Value + "  " + datum.ToString());
            foreach (var w in t.Elements())
            {
              var dt = new TimeSpan(0, 0, Convert.ToInt32(w.Attribute("time_work").Value));

              Console.WriteLine("   " + w.Attribute("time_work").Value + "   " + dt.ToString());
            }
          }
        }

        durchlauf = durchlauf.AddDays(1);
      }
      Console.ReadKey();
    }


    public DateTime DatumAusYyyyMMdd(string AusString)
    {
      return new DateTime(Convert.ToInt32(AusString.Substring(0, 4)), Convert.ToInt32(AusString.Substring(4, 2)), Convert.ToInt32(AusString.Substring(6, 2)));
    }


    private static void SetzeBackflash(ref string Pfad)
    {
      if (Pfad[Pfad.Length - 1] != '\\')
        Pfad += '\\';
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


