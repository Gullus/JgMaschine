using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
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
      var test = Properties.Settings.Default.Properties["ScannerAdresse"].DefaultValue;
      Console.WriteLine(test);

      //using (EventLog eventLog = new EventLog("Application"))
      //{
      //  eventLog.Source = "Application";
      //  eventLog.WriteEntry("Log message example", EventLogEntryType.Information);
      //}


      //var zs = new ArbeitszeitErfassen(new ZeitsteuerungDatafox()
      //{
      //  Standort = "Heidenau",
      //  PfadUpdateBediener = @"C:\Users\jg\Desktop\Test",
      //  TimerIntervall = 10000
      //});

      //var zo = new ZeitsteuerungDatafox()
      //{
      //  OptionenDatafox = new DatafoxOptionen(),
      //  Standort = "Heidenau",
      //  PfadUpdateBediener = @"C:\Users\jg\Desktop\Test",
      //  TimerIntervall = 10000
      //};

      ////ProgDatafox.ListenInTerminalSchreiben(zo.OptionenDatafox, zo.PfadUpdateBediener);

      //var timerUhrzeit = new System.Threading.Timer(OnTimedEvent, zo, 3000, zo.TimerIntervall);

      Console.ReadKey();
    }

    //private static void OnTimedEvent(object state)
  //  {
  //    var zo = (ZeitsteuerungDatafox)state;
  //    Helper.Protokoll("Durchlauf", Helper.ProtokollArt.Info);
  //    zo.ZaehlerDatumAktualisieren++;

  //    // Zeit mit Termimal abgeleichem

  //    if (zo.ZaehlerDatumAktualisieren > 50)
  //    {
  //      zo.ZaehlerDatumAktualisieren = 0;
  //      ProgDatafox.ZeitEinstellen(zo.OptionenDatafox, DateTime.Now);
  //      Helper.Protokoll("Zeit Datafox gestellt!", Helper.ProtokollArt.Info);
  //      return;
  //    }

  //    using (var Db = new JgModelContainer())
  //    {
  //      if (zo.VerbindungsString != "")
  //        Db.Database.Connection.ConnectionString = zo.VerbindungsString;

  //      var standort = Db.tabStandortSet.FirstOrDefault(f => f.Bezeichnung == zo.Standort);
  //      if (standort == null)
  //      {
  //        Helper.Protokoll($"Standort {zo.Standort} nicht gefunden !");
  //        return;
  //      }
  //      var idStandort = standort.Id;

  //      // Kontrolle, ob Benutzer im Termanl geändert werden müssen

  //      if (standort.UpdateBedienerDatafox)
  //      {
  //        standort.UpdateBedienerDatafox = false;
  //        DbSichern.AbgleichEintragen(standort.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
  //        Db.SaveChanges();

  //        var bediener = Db.tabBedienerSet.Where(w => (w.Status != EnumStatusBediener.Stillgelegt) && (w.fStandort == idStandort)).ToList();
  //        ProgDatafox.BedienerInDatafoxDatei(bediener, zo.PfadUpdateBediener);
  //        ProgDatafox.ListenInTerminalSchreiben(zo.OptionenDatafox, zo.PfadUpdateBediener);
  //        return;
  //      }

  //      // Anmeldungen aus Terminal auslesen

  //      var dsVomTerminal = ProgDatafox.ListeAusTerminalAuslesen(zo.OptionenDatafox);
  //      if (dsVomTerminal.Count > 0)
  //      {
  //        var anmeldTermial = ProgDatafox.KonvertDatafoxExport(dsVomTerminal, "MITA_");

  //        var idisAktiveAnmeldungen = Db.tabBedienerSet.Where(w => w.fAktivArbeitszeit != null).Select(s => s.fAktivArbeitszeit).ToArray();
  //        var aktAnmeldungen = Db.tabArbeitszeitSet.Where(w => idisAktiveAnmeldungen.Contains(w.Id)).ToList();

  //        var matchCodes = "'" + string.Join("','", anmeldTermial.Select(s => s.MatchCode).Distinct().ToArray()) + "'";
  //        var alleBediener = Db.tabBedienerSet.Where(w => matchCodes.Contains(w.MatchCode)).ToList();

  //        foreach (var anmeld in anmeldTermial)
  //        {
  //          var bediener = alleBediener.FirstOrDefault(f => f.MatchCode == anmeld.MatchCode);
  //          if (bediener == null)
  //          {
  //            Helper.Protokoll($"Bediner {anmeld.MatchCode} aus Terminal nicht bekannt!", Helper.ProtokollArt.Warnung);
  //            continue;
  //          }

  //          var arbeitzeitVorhanden = aktAnmeldungen.FirstOrDefault(f => f.eBediener.MatchCode == anmeld.MatchCode);

  //          if (anmeld.Vorgang == DatafoxDsExport.EnumVorgang.Komme)
  //          {
  //            if (arbeitzeitVorhanden != null)
  //            {
  //              if (anmeld.Datum.AddHours(1) < DateTime.Now)
  //                continue;
  //            }

  //            var arbZeit = new tabArbeitszeit()
  //            {
  //              Id = Guid.NewGuid(),
  //              fBediener = bediener.Id,
  //              fStandort = idStandort,
  //              Anmeldung = anmeld.Datum,
  //              ManuelleAnmeldung = false,
  //              ManuelleAbmeldung = false,
  //            };
  //            DbSichern.AbgleichEintragen(arbZeit.DatenAbgleich, EnumStatusDatenabgleich.Neu);
  //            Db.tabArbeitszeitSet.Add(arbZeit);
  //            bediener.fAktivArbeitszeit = arbZeit.Id;
  //            DbSichern.AbgleichEintragen(bediener.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
  //          }
  //          else if (anmeld.Vorgang == DatafoxDsExport.EnumVorgang.Gehen)
  //          {
  //            if (arbeitzeitVorhanden != null)
  //            {
  //              arbeitzeitVorhanden.Abmeldung = anmeld.Datum;
  //              arbeitzeitVorhanden.ManuelleAbmeldung = true;
  //              DbSichern.AbgleichEintragen(arbeitzeitVorhanden.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
  //              bediener.fAktivArbeitszeit = null;
  //              DbSichern.AbgleichEintragen(bediener.DatenAbgleich, EnumStatusDatenabgleich.Geaendert);
  //            }
  //            else
  //            {
  //              var arbZeit = new tabArbeitszeit()
  //              {
  //                Id = Guid.NewGuid(),
  //                fBediener = bediener.Id,
  //                fStandort = idStandort,
  //                Abmeldung = anmeld.Datum,
  //                ManuelleAnmeldung = false,
  //                ManuelleAbmeldung = false,
  //              };
  //              DbSichern.AbgleichEintragen(arbZeit.DatenAbgleich, EnumStatusDatenabgleich.Neu);
  //              Db.tabArbeitszeitSet.Add(arbZeit);
  //            }
  //          }

  //          Db.SaveChanges();
  //          Helper.Protokoll($"{dsVomTerminal.Count} Arbeitszeiten ins System übertragen!", Helper.ProtokollArt.Info);
  //        }
  //      }
  //    }
  //  }
  }

  //public class ZeitsteuerungDatafox
  //{
  //  internal string Standort = "";
  //  internal string PfadUpdateBediener = "";
  //  internal JgModelContainer Db = null;
  //  internal DatafoxOptionen OptionenDatafox;
  //  internal string VerbindungsString = "";
  //  internal int ZaehlerDatumAktualisieren = 0;
  //  internal int TimerIntervall = 10000;

  //  public ZeitsteuerungDatafox()
  //  { }
  //}




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


