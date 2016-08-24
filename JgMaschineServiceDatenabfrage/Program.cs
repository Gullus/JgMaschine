using System;
using System.Linq;
using System.Net.NetworkInformation;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineServiceDatenabfrage.Maschinen;

namespace JgMaschineServiceDatenabfrage
{
  class Program
  {
    static void Main(string[] args)
    {
      var abfrage = new MaschineDatenabfrage();
      abfrage.Start();
      Console.ReadKey();
    }
  }

  public class MaschineDatenabfrage
  {
    private Proto _Protokoll;
    public Proto Protokoll { get { return _Protokoll; } }

    public MaschineDatenabfrage()
    { }
    public void Start()
    {
      string msg = "";
      _Protokoll = new Proto(Proto.KategorieArt.ServiceDatenabfrage, null);

      while (true)
      {
        using (var db = new JgModelContainer())
        {
          var standort = db.tabStandortSet.FirstOrDefault(w => w.Bezeichnung.ToUpper() == Properties.Settings.Default.Standort);
          if (standort == null)
          {
            msg = $"Standort {Properties.Settings.Default.Standort} nicht gefunden!";
            _Protokoll.Set(msg, Proto.ProtoArt.Fehler);
            break;
          }

          var lMaschinen = db.tabMaschineSet.Where(w => (w.fStandort == standort.Id) && (w.Status != EnumStatusMaschine.Stillgelegt) 
            && (w.MaschinenArt != EnumMaschinenArt.Handbiegung) && (w.ProdDatenabfrage)).ToList();
          if (lMaschinen.Count == 0)
          {
            msg = $"Für Standort {standort.Bezeichnung} sind keine Maschinen vorhanden!";
            _Protokoll.Set(msg, Proto.ProtoArt.Warnung);
            break;
          }

          MaschinenStamm maStamm = null;
          MaschineEvg maEvg = null;
          MaschineSchnell maSchnell = null;
          MaschineProgress maProgress = null;

          foreach (var maschine in lMaschinen)
          {
            var pr = maschine.eProtokoll;
            pr.AuswertungStart = DateTime.Now;
            pr.AnzahlDurchlauf++; 

            if (!IstPingOk(maschine, _Protokoll))
            {
              maschine.eProtokoll.FehlerVerbindungMaschine++;
              continue;
            }

            switch (maschine.MaschinenArt)
            {
              case EnumMaschinenArt.Evg:
                if (maEvg == null)
                  maEvg = new MaschineEvg(db, _Protokoll, Properties.Settings.Default.PfadDatenEvG);
                maStamm = maEvg;
                break;
              case EnumMaschinenArt.Progress:
                if (maProgress == null)
                  maProgress = new MaschineProgress(db, _Protokoll, Properties.Settings.Default.PfadDatenProgress);
                maStamm = maProgress;
                break;
              case EnumMaschinenArt.Schnell:
                if (maSchnell == null)
                  maSchnell = new MaschineSchnell(db, _Protokoll, Properties.Settings.Default.PfadDatenSchnell);
                maStamm = maSchnell;
                break;
            }

            try
            {
              maStamm.Start(maschine);
            }
            catch (Exception f)
            {
              msg = $"Fehler import Daten in Maschine {maschine.MaschinenName}";
              maschine.eProtokoll.FehlerDatenImport++;
              maschine.ProtokollAdd = msg;
              _Protokoll.Set(msg, f);
              continue;
            }
            finally
            {
              pr.AuswertungEnde = DateTime.Now;
            }

            try
            {
              db.SaveChanges();
            }
            catch (Exception f)
            {
              msg = $"Fehler beim speichern des Maschinenprotokolls  {maschine.MaschinenName}";
              _Protokoll.Set(msg, f);
              break;
            }
          }
        }
      }
    }

    private bool IstPingOk(tabMaschine Maschine, Proto Protokoll)
    {
      string msg = "";

      if (string.IsNullOrWhiteSpace(Maschine.MaschineAdresse))
      {
        msg = $"Ip Adresse von {Maschine.MaschinenName} ist leer!";
        Protokoll.Set(msg, Proto.ProtoArt.Warnung);
      }
      else
      {
        var sender = new Ping();
        PingReply result = null;
        try
        {
          result = sender.Send(Maschine.MaschineAdresse);

          if (result.Status == IPStatus.Success)
            return true;
          else
          {
            msg = $"Bing {Maschine.MaschineAdresse} mit Status {result.Status} bei Maschine {Maschine.MaschinenName} fehlgeschlagen!";
            Protokoll.Set(msg, Proto.ProtoArt.Kommentar);
          }
        }
        catch (Exception f)
        {
          msg = $"Fehler bei Pingabfrage {Maschine.MaschineAdresse} zu Maschine {Maschine.MaschinenName}!";
          Protokoll.Set(msg, f);
        }
      }

      return false;
    }
  }
}
