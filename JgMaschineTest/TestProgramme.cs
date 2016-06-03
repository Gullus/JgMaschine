using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JgMaschineTest
{
  class TestProgramme
  {
    private void XmlDateiEinlesen()
    {
      //string datXml = @"C:\Entwicklung\JgMaschine\JgMaschineTest\bin\Debug\20151102_produzione.xml";
      //XDocument xdoc = XDocument.Load(datXml);

      //XElement root = xdoc.Root;
      //var dataName = "{" + root.GetDefaultNamespace().NamespaceName + "}" + "DATA";
      //XElement rootStart = root.Elements().FirstOrDefault(z => z.Name == dataName);

      //foreach (var t in rootStart.Elements())
      //{
      //  Console.WriteLine(t.Attribute("date").Value + "  " + t.Attribute("time_on").Value);
      //  foreach (var w in t.Elements())
      //  {
      //    Console.WriteLine("   " + w.Attribute("time_work").Value);
      //  }
      //}
    }

    private static void AddDs<T>(T Datensatz)
      where T : class
    {
      //    using (var db = new JgModelContainer())
      //    {
      //      var g = db.Set<T>();
      //      var jj = db.Entry<T>(Datensatz);
      //      g.Add(jj.Entity);
      //      db.SaveChanges();
      //    }
    }

    private static void TcpIpSenden()
    {
      //var adresse = Properties.Settings.Default.ScannerAdresse;
      //var port = Properties.Settings.Default.ScannerPort;

      //try
      //{
      //  Console.WriteLine("Client starten");

      //  var cl = new TcpClient(adresse, port);
      //  var ns = cl.GetStream();

      //  Console.WriteLine("Client gestartet");

      //  while (true)
      //  {
      //    var buffer = new byte[4096];
      //    var anz = ns.Read(buffer, 0, buffer.Length);

      //    var returndata = Encoding.UTF8.GetString(buffer, 0, anz);

      //    Console.WriteLine(returndata);

      //    var sendBytes = Encoding.UTF8.GetBytes(Convert.ToChar(13).ToString());
      //    ns.Write(sendBytes, 0, sendBytes.Length);
      //  }
      //}
      //catch (Exception f)
      //{
      //  Console.WriteLine(f.Message);
      //}
    }

    private void DatenbankAbgleich()
    {
      //static void Main()
      //{
      //  var adresse = Properties.Settings.Default.Adresse;
      //  var port = Properties.Settings.Default.PortNummer;

      //  var dl = new DataLogicScanner(adresse, port, null, true);
      //  dl.Start();

      //  Console.ReadKey();
      //}


      //public class DbMerge
      //{
      //  private JgModelContainer _DbClient;
      //  private JgModelContainer _DbServer;

      //  public DbMerge(string ConnectioStringClient, string ConectionStringServer)
      //  {
      //    _DbClient = new JgModelContainer();
      //    _DbClient.Database.Connection.ConnectionString = ConnectioStringClient;
      //    _DbServer = new JgModelContainer();
      //    _DbServer.Database.Connection.ConnectionString = ConectionStringServer;
      //  }

      //  private void Fehler(string Wert)
      //  {
      //    Console.WriteLine(Wert);
      //  }

      //  /// <summary>
      //  /// Das Datum wird zum Vergeleich bei vorhanden Datensätzen genutz. Der Datensatz mit dem älteren Datum wird kopiert.
      //  /// Fehlt ein Datensatz in DatensatzNach, wird der aus DatensatzVon in DatensatzNach kopiert. 
      //  /// </summary>
      //  /// <typeparam name="T"></typeparam>
      //  /// <param name="DbVon"></param>
      //  /// <param name="ListeVon">Ausgangsliste</param>
      //  /// <param name="DbNach"></param>
      //  /// <param name="ListeNach">Liste der Daten, in die Liste Von kopiert werden soll</param>
      //  /// <param name="FelderAusschluss">Liste der Felder, die nicht kopiert werden sollen.</param>
      //  public void DatenEintragen<T>(JgModelContainer DbVon, List<T> ListeVon, JgModelContainer DbNach, Dictionary<string, T> ListeNach, params string[] FelderAusschluss)
      //    where T : class
      //  {
      //    var t = typeof(T);

      //    var lNach = new SortedList<string, T>(ListeNach, StringComparer.CurrentCultureIgnoreCase);

      //    string idVon = "";
      //    DatenAbgleich datAbgleichVon = null;
      //    DatenAbgleich datAbgleichNach = null;
      //    DbEntityEntry<T> entVon = null;
      //    DbEntityEntry<T> entNach = null;

      //    var felder = typeof(T).GetProperties().Select(s => s.Name).Except(FelderAusschluss).Union(new string[] { "Id" });

      //    foreach (var dsVon in ListeVon)
      //    {
      //      entVon = DbVon.Entry<T>(dsVon);
      //      idVon = entVon.Property<Guid>("Id").CurrentValue.ToString();
      //      datAbgleichVon = entVon.Property<DatenAbgleich>("DatenAbgleich").CurrentValue;
      //      datAbgleichVon.Status = EnumStatusDatenabgleich.Abgeglichen;

      //      if (lNach.ContainsKey(idVon))
      //      {
      //        entNach = DbNach.Entry<T>(lNach[idVon]);
      //        datAbgleichNach = entNach.Property<DatenAbgleich>("DatenAbgleich").CurrentValue;
      //        datAbgleichNach.Status = EnumStatusDatenabgleich.Abgeglichen;

      //        if ((datAbgleichNach.Status != EnumStatusDatenabgleich.Abgeglichen) && (datAbgleichVon.Datum < datAbgleichNach.Datum))
      //        {
      //          entVon = DbNach.Entry<T>(lNach[idVon]);
      //          entNach = DbVon.Entry<T>(dsVon);
      //        }

      //        foreach (var feld in felder)
      //        {
      //          if (entNach.Property(feld).CurrentValue != entVon.Property(feld).CurrentValue)
      //            entNach.Property(feld).CurrentValue = entVon.Property(feld).CurrentValue;
      //        }
      //      }
      //      else
      //      {
      //        var dsNeu = DbNach.Set<T>().Create<T>();
      //        var entNeu = DbNach.Entry(dsNeu);
      //        entNeu.Property("Id").CurrentValue = entVon.Property("Id").CurrentValue;
      //        foreach (var feld in felder)
      //          entNeu.Property(feld).CurrentValue = entVon.Property(feld).CurrentValue;
      //        DbNach.Set<T>().Add(entNeu.Entity);
      //      }
      //    }

      //    DbVon.SaveChanges();
      //    DbNach.SaveChanges();
      //  }

      //  private bool KontrolleDatenbankVerbindungFehler()
      //  {
      //    var vs = _DbServer.Database.Connection.ConnectionString;
      //    var ss = "Server";
      //    for (int i = 0; i < 2; i++)
      //    {
      //      if (i == 1)
      //      {
      //        vs = _DbClient.Database.Connection.ConnectionString;
      //        ss = "Client";
      //      }

      //      SqlConnection conServer = new SqlConnection(vs);
      //      try
      //      {
      //        conServer.Open();
      //        conServer.Close();
      //      }
      //      catch (Exception f)
      //      {
      //        Fehler($"Fehler beim öffnen der {ss}bindung mit Verbindungsstring: {vs}.\n\rGrunf: {f.Message}");
      //        return true;
      //      }
      //    }
      //    return false;
      //  }

      //  public async void Start()
      //  {
      //    if (KontrolleDatenbankVerbindungFehler())
      //      return;

      //    #region Standort abgleichen

      //    var sNameTabelle = "Standorte";
      //    var felderAuschliessen = new string[] { "sMaschinen", "sArbeitszeiten", "sBediener" };
      //    try
      //    {
      //      var lServer = await _DbServer.tabStandortSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lServer.Count > 0)
      //      {
      //        var idsNach = lServer.Select(s => s.Id).ToArray();
      //        var lClient = await _DbClient.tabStandortSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabStandort>(_DbServer, lServer, _DbClient, lClient, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Server -> Client {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    try
      //    {
      //      var lClient = await _DbClient.tabStandortSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lClient.Count > 0)
      //      {
      //        var idsNach = lClient.Select(s => s.Id).ToArray();
      //        var lServer = await _DbServer.tabStandortSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabStandort>(_DbClient, lClient, _DbServer, lServer, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Client -> Server {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    #endregion

      //    #region Bediener abgleichen

      //    sNameTabelle = "Bediener";
      //    felderAuschliessen = new string[] { "Name", "eAktuelleAnmeldungMaschine", "eStandort", "sAnmeldungen", "sArbeitszeiten", "sBauteile", "sReparaturProtokollanten", "sReparaturVerursacher" };
      //    try
      //    {
      //      var lServer = await _DbServer.tabBedienerSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lServer.Count > 0)
      //      {
      //        var idsNach = lServer.Select(s => s.Id).ToArray();
      //        var lClient = await _DbClient.tabBedienerSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabBediener>(_DbServer, lServer, _DbClient, lClient, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Server -> Client {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    try
      //    {
      //      var lClient = await _DbClient.tabBedienerSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lClient.Count > 0)
      //      {
      //        var idsNach = lClient.Select(s => s.Id).ToArray();
      //        var lServer = await _DbServer.tabBedienerSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabBediener>(_DbClient, lClient, _DbServer, lServer, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Client -> Server {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    #endregion

      //    #region Maschine abgleichen

      //    sNameTabelle = "Maschine";
      //    felderAuschliessen = new string[] { "ProtokollAdd", "eLetztesBauteil", "eProtokoll", "sAktuelleBediener", "sAnmeldungen", "sBauteile", "sReparaturen", "eStandort" };
      //    try
      //    {
      //      var lServer = await _DbServer.tabMaschineSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lServer.Count > 0)
      //      {
      //        var idsNach = lServer.Select(s => s.Id).ToArray();
      //        var lClient = await _DbClient.tabMaschineSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabMaschine>(_DbServer, lServer, _DbClient, lClient, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Server -> Client {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    try
      //    {
      //      var lClient = await _DbClient.tabMaschineSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lClient.Count > 0)
      //      {
      //        var idsNach = lClient.Select(s => s.Id).ToArray();
      //        var lServer = await _DbServer.tabMaschineSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabMaschine>(_DbClient, lClient, _DbServer, lServer, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Client -> Server {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    #endregion

      //    #region Protokoll abgleichen

      //    sNameTabelle = "Protokolle";
      //    felderAuschliessen = new string[] { "StatusAsString", "eMaschine" };
      //    try
      //    {
      //      var lServer = await _DbServer.tabProtokollSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lServer.Count > 0)
      //      {
      //        var idsNach = lServer.Select(s => s.Id).ToArray();
      //        var lClient = await _DbClient.tabProtokollSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabProtokoll>(_DbServer, lServer, _DbClient, lClient, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Server -> Client {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    try
      //    {
      //      var lClient = await _DbClient.tabProtokollSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lClient.Count > 0)
      //      {
      //        var idsNach = lClient.Select(s => s.Id).ToArray();
      //        var lServer = await _DbServer.tabProtokollSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabProtokoll>(_DbClient, lClient, _DbServer, lServer, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Client -> Server {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    #endregion

      //    #region Auswertung abgleichen

      //    sNameTabelle = "Auswertung";
      //    try
      //    {
      //      var lServer = await _DbServer.tabAuswertungSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lServer.Count > 0)
      //      {
      //        var idsNach = lServer.Select(s => s.Id).ToArray();
      //        var lClient = await _DbClient.tabAuswertungSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabAuswertung>(_DbServer, lServer, _DbClient, lClient);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Server -> Client {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    try
      //    {
      //      var lClient = await _DbClient.tabAuswertungSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lClient.Count > 0)
      //      {
      //        var idsNach = lClient.Select(s => s.Id).ToArray();
      //        var lServer = await _DbServer.tabAuswertungSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabAuswertung>(_DbClient, lClient, _DbServer, lServer);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Client -> Server {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    #endregion

      //    #region Arbeitszeit abgleichen

      //    sNameTabelle = "Arbeitszeit";
      //    felderAuschliessen = new string[] { "eBediener", "eStandort" };
      //    try
      //    {
      //      var lServer = await _DbServer.tabArbeitszeitSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lServer.Count > 0)
      //      {
      //        var idsNach = lServer.Select(s => s.Id).ToArray();
      //        var lClient = await _DbClient.tabArbeitszeitSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabArbeitszeit>(_DbServer, lServer, _DbClient, lClient, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Server -> Client {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    try
      //    {
      //      var lClient = await _DbClient.tabArbeitszeitSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lClient.Count > 0)
      //      {
      //        var idsNach = lClient.Select(s => s.Id).ToArray();
      //        var lServer = await _DbServer.tabArbeitszeitSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabArbeitszeit>(_DbClient, lClient, _DbServer, lServer, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Client -> Server {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    #endregion

      //    #region AnmeldungMaschine abgleichen

      //    sNameTabelle = "AnmeldungMaschine";
      //    felderAuschliessen = new string[] { "eBediener", "eMaschine" };
      //    try
      //    {
      //      var lServer = await _DbServer.tabAnmeldungMaschineSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lServer.Count > 0)
      //      {
      //        var idsNach = lServer.Select(s => s.Id).ToArray();
      //        var lClient = await _DbClient.tabAnmeldungMaschineSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabAnmeldungMaschine>(_DbServer, lServer, _DbClient, lClient, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Server -> Client {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    try
      //    {
      //      var lClient = await _DbClient.tabAnmeldungMaschineSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lClient.Count > 0)
      //      {
      //        var idsNach = lClient.Select(s => s.Id).ToArray();
      //        var lServer = await _DbServer.tabAnmeldungMaschineSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabAnmeldungMaschine>(_DbClient, lClient, _DbServer, lServer, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Client -> Server {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    #endregion

      //    #region Bauteil abgleichen

      //    sNameTabelle = "Bauteil";
      //    felderAuschliessen = new string[] { "eBauteil", "eMaschien", "sBediener" };
      //    try
      //    {
      //      var lServer = await _DbServer.tabBauteilSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lServer.Count > 0)
      //      {
      //        var idsNach = lServer.Select(s => s.Id).ToArray();
      //        var lClient = await _DbClient.tabBauteilSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabBauteil>(_DbServer, lServer, _DbClient, lClient, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Server -> Client {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    try
      //    {
      //      var lClient = await _DbClient.tabBauteilSet.Where(w => (w.DatenAbgleich.Status != EnumStatusDatenabgleich.Abgeglichen)).ToListAsync();
      //      if (lClient.Count > 0)
      //      {
      //        var idsNach = lClient.Select(s => s.Id).ToArray();
      //        var lServer = await _DbServer.tabBauteilSet.Where(w => idsNach.Contains(w.Id)).ToDictionaryAsync(s => s.Id.ToString(), s => s);
      //        DatenEintragen<tabBauteil>(_DbClient, lClient, _DbServer, lServer, felderAuschliessen);
      //      }
      //    }
      //    catch (Exception f)
      //    {
      //      Fehler($"Fehler beim Datenabgleich Client -> Server {sNameTabelle}.\n\rGrund: {f.Message}");
      //    }

      //    #endregion
      //  }
      //}
    }
  }
}
