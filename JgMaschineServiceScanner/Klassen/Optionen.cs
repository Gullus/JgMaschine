using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using JgMaschineData;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceScanner
{
  public class ScannerOptionen
  {
    public string VorgabeStandort = null;
    public string DbVerbindungsString = null;
    public JgModelContainer DbScann = null;

    public TcpClient Client;

    public string CradleIpAdresse = null;
    public int CradlePortNummer = 0;

    public string CradleTextAnmeldung = "";

    public string EvgPfadProduktionsListe = null;
    public string EvgDateiProduktionsAuftrag = null;
    public string ProgressPfadProduktionsListe = null;

    private tabStandort _Standort;
    public tabStandort Standort { get => _Standort; }

    internal void InitScanner()
    {
      var msg = "";

      try
      {
        using (var db = new JgModelContainer())
        {
          if (DbVerbindungsString != "")
            db.Database.Connection.ConnectionString = DbVerbindungsString;

          // Standort einlesen und damit gleichzeitg Datenbank prüfen

          try
          {
            _Standort = db.tabStandortSet.FirstOrDefault(f => f.Bezeichnung == VorgabeStandort);
            if (_Standort != null)
            {
              msg = $"Anmeldung Entity Framework für Standort: {_Standort.Bezeichnung}";
              Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Information);
            }
            else
              throw new Exception($"In Config eingetragener Standort {VorgabeStandort} ist in der Datenbank nicht vorhanden.");
          }
          catch (Exception ex)
          {
            msg = $"Beim auslesen des Standortes trat ein Fehler auf!";
            throw new Exception(msg, ex);
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception("Fehler im Initialisierungsblock !", ex);
      }
    }
  }
}
