using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using System.Configuration;
using JgZeitHelper;
using System.Text;
using JgMaschineData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.ServiceModel;
using System.Messaging;
using System.Threading;
using System.Data.SqlClient;
using System.Data.Entity;

namespace JgMaschineTest
{
    class Programm
    {
        static void Main(string[] args)
        {
            //using (var db = new JgMaschineData.JgModelContainer())
            //{
            //    var datMonatAnfang = new DateTime(2017, 12, 1);
            //    var datMonatEnde = datMonatAnfang.AddMonths(1).AddSeconds(-1);

            //    var li =  db.tabBedienerSet.Where(w => (w.Status == EnumStatusBediener.Aktiv) && (w.DatumEinstellung < datMonatEnde) && ((w.DatumEntlassung == null) || (w.DatumEntlassung > datMonatAnfang)) && !w.DatenAbgleich.Geloescht).OrderBy(o => o.NachName).ToList();

            //    foreach (var ben in li)
            //        Console.WriteLine(ben.NachName);
            //}

            Console.WriteLine(IstVorhanden(new DateTime(2017,12,1), new DateTime(2017, 11, 1), new DateTime(2018, 11, 30)));



            Console.ReadKey();


            //  Logger.SetLogWriter(new LogWriterFactory().Create());
            //ExceptionPolicy.SetExceptionManager(new ExceptionPolicyFactory().CreateManager(), false);

            //Logger.Write("Hallo Ballo");


            //var i = 0;

            //try
            //{
            //  Console.WriteLine(10 / i);
            //}
            //catch (Exception f)
            //{
            //  ExceptionPolicy.HandleException(f, "Policy");
            //}
        }

        public static bool IstVorhanden(DateTime AktDatum, DateTime Eintritt, DateTime? Austritt)
        {
            var datErster = new DateTime(AktDatum.Year, AktDatum.Month, 1);
            var datLetzter = datErster.AddMonths(1).AddSeconds(-1);

            Console.WriteLine($"{datErster} {datLetzter}    {Eintritt}  {Austritt}");

            if ((Eintritt <= datLetzter) && ((Austritt == null) || (Austritt.Value >= datErster)))
                return true;

            return false;
        }
    }

    public class TestDatum
    {
        public TestDatum(DateTime von, DateTime bis)
        { 
            Von = von;
            Bis = bis;
        }

        public DateTime Von { get; }
        public DateTime Bis { get; }
    }

    public class DatenKontrolle<T>
      where T : class
    {
        public class DatenVergleich
        {
            public Guid Id { get; set; }
            public DateTime DatenAbgleich_Datum { get; set; }
        }

        private JgModelContainer _Db;
        private System.Data.Entity.DbSet _Entity;

        public string SqlText = null;

        public DatenKontrolle(JgModelContainer Db)
        {
            _Db = Db;
            _Entity = _Db.Set<T>();
        }

        public void Pruefen(params SqlParameter[] Parameter)
        {
            var daten = _Db.Database.SqlQuery<DatenVergleich>(SqlText, Parameter).ToList();
            foreach (var ds in daten)
            {
                var vorhanden = _Entity.Find(ds.Id);
                if (vorhanden != null)
                {
                    var entVorhanden = _Db.Entry(vorhanden);
                    if (entVorhanden.ComplexProperty("DatenAbgleich").Property("Datum").CurrentValue != null)
                    {
                        if (Convert.ToDateTime(entVorhanden.ComplexProperty("DatenAbgleich").Property("Datum").CurrentValue) != ds.DatenAbgleich_Datum)
                            entVorhanden.Reload();
                    }
                }
            }
        }
    }


    public class TestMaschine
    {
        public Guid Id { get; set; }
        public DateTime Datum { get; set; }
    }


    [ServiceContract]
    public interface IMsmqDaten
    {
        [OperationContract(IsOneWay = true)]
        void SendRequest(string first_name, string last_name);

        [OperationContract(IsOneWay = true)]
        void GetReponse(string first_name, string last_name);
    }

    public class MsmqIn : IMsmqDaten
    {
        public void SendRequest(string first_Name, string last_Name)
        {
            var binding = new NetMsmqBinding(NetMsmqSecurityMode.None);
            var address = new EndpointAddress("net.msmq://localhost/private/OutSchlange");

            var proxy = ChannelFactory<IMsmqDaten>.CreateChannel(binding, address);
            proxy.GetReponse(first_Name, last_Name);

            Console.WriteLine($"Request Namen {first_Name} {last_Name}");
        }

        public void GetReponse(string first_name, string last_name)
        {
            Console.WriteLine($"Request Namen {first_name} {last_name}");
        }


    }


    public class c1
    {
        private string _Test1 = "Hallo";
        public string Test1
        {
            get
            {
                return _Test1;
            }

            set
            {
                _Test1 = value;
            }
        }
    }

    public class c2 : c1
    {

    }

    public class c3 : c1
    {
        private string _Test2 = "Ballo";
        public new string Test1
        {
            get
            {
                return _Test2;
            }

            set
            {
                _Test2 = value;
            }
        }
    }

    //public class MsmqOut : IMsmqDaten
    //{
    //  public void SendRequest(string first_Name, string last_Name)
    //  {
    //    var binding = new NetMsmqBinding(NetMsmqSecurityMode.None);
    //    var address = new EndpointAddress("net.msmq://localhost/private/InSchlange");

    //    var proxy = ChannelFactory<IMsmqDaten>.CreateChannel(binding, address);
    //    proxy.GetReponse(first_Name, last_Name);

    //    Console.WriteLine($"Request Namen {first_Name} {last_Name}");
    //  }

    //  public void GetReponse(string first_name, string last_name)
    //  {
    //    Console.WriteLine($"Request Namen {first_name} {last_name}");
    //  }
    //}


    public class JgMaschineDatenHandy
    {
        //"Manage NuGet packages" -> Search for "newtonsoft json".

        private const string _Lc = "JgDatenHandy";
        private string _ConnectionString;
        private int _PortNummer;

        private TcpListener _Listener;

        public class HandyOptionen
        {
            public const string Lc = "JgDatenHandy";
            public string ConnectionString;

            public TcpClient Client = null;

            public HandyOptionen(string ConnectionStringDb, TcpClient ClientHandy)
            {
                Client = ClientHandy;
                ConnectionString = ConnectionStringDb;
            }
        }

        public class HandyDaten
        {
            public string IdMitarbeiter { get; set; }
            public string Zeitpunkt { get; set; }
            public string IstAnmeldung { get; set; }
        }

        public JgMaschineDatenHandy(string ConnectionStringDb, int PortNummerServer)
        {
            _PortNummer = PortNummerServer;
            _ConnectionString = ConnectionStringDb;
        }

        public async void Start()
        {
            string msg = $"Starte Server !";
            Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);

            string hostName = Dns.GetHostName();
            var hostIp = JgMaschineLib.TcpIp.Helper.GetIpAdressV4(Dns.GetHostName());

            try
            {
                _Listener = new TcpListener(hostIp, _PortNummer);
                _Listener.Start(200);

                msg = $"Listener gestartet:\n  Server: {hostIp} Port: {_PortNummer}";
                Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);

                while (true)
                {
                    var client = await _Listener.AcceptTcpClientAsync();

                    var hOpt = new HandyOptionen(_ConnectionString, client);

                    await Task.Factory.StartNew(handyOpt =>
                    {
                        var ho = (HandyOptionen)handyOpt;

                        NetworkStream nwStream = null;

                        try
                        {
                            var clientIp = ((IPEndPoint)ho.Client.Client.RemoteEndPoint).Address.ToString();
                            var clientPort = ((IPEndPoint)ho.Client.Client.RemoteEndPoint).Port;

                            msg = $"Client verbunden:\nPort: {clientIp} Port: {clientPort}";
                            Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);

                            while (true)
                            {
                                var buffer = new byte[4096];
                                nwStream = client.GetStream();
                                var anzahlZeichen = nwStream.Read(buffer, 0, buffer.Length);

                                var empf = JgMaschineLib.TcpIp.Helper.BufferInString(buffer, anzahlZeichen);
                                msg = $"{anzahlZeichen} Zeichnen vom Server Empfangen.\nText: {empf}";
                                Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);

                                var senden = JgMaschineLib.TcpIp.Helper.StringInBuffer($"{anzahlZeichen} Zeichen empfangen!");
                                nwStream.WriteAsync(senden, 0, senden.Length);
                            }
                        }
                        catch (ObjectDisposedException ex)
                        {
                            msg = $"DisposeException.\nGrund: {ex.Message}";
                            Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);
                            return;
                        }
                        catch (SocketException ex)
                        {
                            msg = $"Client Socket Abbruch.\nGrund: {ex.Message}";
                            Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);
                            return;
                        }
                        catch (IOException ex)
                        {
                            msg = $"Client IO Abbruch.\nGrund: {ex.Message}";
                            Logger.Write(msg, _Lc, 0, 0, TraceEventType.Information);
                            return;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Fehler bei Clientverrbeitung !", ex);
                        }
                        finally
                        {
                            ho.Client.Close();
                            nwStream?.Close();
                        }
                    }, hOpt, TaskCreationOptions.LongRunning);
                }
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, "Policy");
            }
            finally
            {
                _Listener.Stop();
            }
        }
    }
}

