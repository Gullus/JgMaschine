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
using System.Collections;
using System.Linq.Expressions;
using System.Windows.Data;
using System.Data.Entity.Core.Objects;

namespace JgMaschineTest
{
  class Programm
  {

    static void Main(string[] args)
    {
      using (var Db = new JgModelContainer())
      {
        var tl = new TestList<tabStandort, tabBediener>(Db, "tabStandortSet")
        {
          Dedend_EntitiDataset = "tabBedienerSet",
          Depend_SqlWhereString = $"fStandort In ({TestList<tabStandort, tabBediener>.Depend_IdPlatzhallter})",
        };

        tl.Daten = Db.tabStandortSet.ToList();

        while (true)
        {
          foreach (var ds in tl.Daten)
          {
            Console.WriteLine(ds.Bezeichnung);

            foreach (var it in ds.sBediener)
              Console.WriteLine("   " + it.NachName);
          }
          Console.WriteLine("---------------");

          Console.ReadKey();

          tl.DatenAktualisieren();
        }
      }
    }
  }

  public class TestList<Prinzipal, Dependency>
    where Prinzipal : class
    where Dependency : class
  {
    private JgModelContainer _Db;
    private string _EntityDataset;
    private Guid[] _IdisDaten;

    private string _SqlWhereString { get; set; } = "";
    public List<Prinzipal> Daten { get; set; }

    public static string Depend_IdPlatzhallter = "#IdisPlatzHalter#";
    public string Dedend_EntitiDataset { get; set; } = "";
    public string Depend_SqlWhereString { get; set; } = "";

    public TestList(JgModelContainer Db, string EntityDataset, string SqlWehreString = "")
    {
      _Db = Db;
      _EntityDataset = EntityDataset;
      _SqlWhereString = SqlWehreString;
    }

    private Dictionary<Guid, DateTime> ListeAusDatenbank(SqlConnection SqlVerbindung, string EntityDataset, string WhereString)
    {
      var dbDaten = new Dictionary<Guid, DateTime>();

      var sqlWhere = (WhereString == "") ? "" : " Where " + WhereString;
      var sqlString = $"Select Id, DatenAbgleich_Datum From {EntityDataset}{sqlWhere}";
      var com = new SqlCommand(sqlString, SqlVerbindung);

      var reader = com.ExecuteReader();
      while (reader.Read())
        dbDaten.Add((Guid)reader[0], (DateTime)reader[1]);

      return dbDaten;
    }

    private void AktPrinzipal(SqlConnection SqlVerbindung, string EntityDataset, string WhereString)
    {
      var listeDb = ListeAusDatenbank(SqlVerbindung, EntityDataset, WhereString);
      _IdisDaten = listeDb.Keys.ToArray();      

      var listeLoeschen = new List<Prinzipal>();
      var idisVorhanden = new SortedSet<Guid>();
      foreach (var ds in Daten)
      {
        var dsEntity = _Db.Entry(ds);
        var id = (Guid)dsEntity.Property("Id").CurrentValue;

        if (listeDb.ContainsKey(id))
        {
          idisVorhanden.Add(id);
          if (dsEntity.Property<DatenAbgleich>("DatenAbgleich").CurrentValue.Datum != listeDb[id])
            _Db.Entry(ds).Reload();
        }
        else
          listeLoeschen.Add(ds);
      }

      foreach (var ds in listeLoeschen)
        Daten.Remove(ds);

      var idisNeu = _IdisDaten.Where(w => !idisVorhanden.Contains(w)).ToArray();
      foreach (var id in idisNeu)
        Daten.Add(_Db.Set<Prinzipal>().Find(id));
    }

    private void AktDepend(SqlConnection SqlVerbindung, string EntityDataset, string WhereString)
    {
      var listeDb = ListeAusDatenbank(SqlVerbindung, EntityDataset, WhereString);

      foreach (var ds in listeDb)
      {
        var kontr = _Db.Set<Dependency>().Find(ds.Key);
        if (kontr != null)
        {
          if (_Db.Entry<Dependency>(kontr).Property<DatenAbgleich>("DatenAbgleich").CurrentValue.Datum != ds.Value)
            _Db.Entry<Dependency>(kontr).Reload();
        }
      }
    }

    public void DatenAktualisieren()
    {
      var con = new SqlConnection(_Db.Database.Connection.ConnectionString);
      con.Open();

      AktPrinzipal(con, _EntityDataset, _SqlWhereString);

      if (Dedend_EntitiDataset != null)
      {
        var sIdis = "'" + string.Join("','", _IdisDaten) + "'";
        var sqlText = Depend_SqlWhereString.Replace(Depend_IdPlatzhallter, sIdis);
        AktDepend(con, Dedend_EntitiDataset, sqlText);
      }

      con.Close();
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


