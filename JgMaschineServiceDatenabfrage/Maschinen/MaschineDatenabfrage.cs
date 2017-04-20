using System;
using System.Linq;
using JgMaschineData;
using JgMaschineLib;
using JgMaschineServiceDatenabfrage.Maschinen;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace JgMaschineServiceDatenabfrage
{
  public class MaschineDatenabfrage
  {
    private OptioneAbfrage _Optionen;
  
    public MaschineDatenabfrage(OptioneAbfrage OptionenAbfrage)
    { 
      _Optionen = OptionenAbfrage;
    }

    public void Start()
    {
      string msg = "";

      while (true)
      {
        using (var db = new JgModelContainer())
        {
          var standort = db.tabStandortSet.FirstOrDefault(w => w.Bezeichnung.ToUpper() == Properties.Settings.Default.Standort);
          if (standort == null)
          {
            msg = $"Standort {Properties.Settings.Default.Standort} nicht gefunden!";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Error);
            break;
          }

          var lMaschinen = db.tabMaschineSet.Where(w => (w.fStandort == standort.Id) && (w.Status != EnumStatusMaschine.Stillgelegt)
            && (w.MaschinenArt != EnumMaschinenArt.Handbiegung) && (w.ProdDatenabfrage)).ToList();
          if (lMaschinen.Count == 0)
          {
            msg = $"Für Standort {standort.Bezeichnung} sind keine Maschinen vorhanden!";
            Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
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

            if (!Helper.IstPingOk(maschine.MaschineAdresse, out msg))
            {
              maschine.eProtokoll.FehlerVerbindungMaschine++;
              continue;
            }

            switch (maschine.MaschinenArt)
            {
              case EnumMaschinenArt.Evg:
                if (maEvg == null)
                  maEvg = new MaschineEvg(db, _Optionen.PfadEvg);
                maStamm = maEvg;
                break;
              case EnumMaschinenArt.Progress:
                if (maProgress == null)
                  maProgress = new MaschineProgress(db, _Optionen.PfadProgress);
                maStamm = maProgress;
                break;
              case EnumMaschinenArt.Schnell:
                if (maSchnell == null)
                  maSchnell = new MaschineSchnell(db, _Optionen.PfadSchnell);
                maStamm = maSchnell;
                break;
            }

            try
            {
              maStamm.Start(maschine);
            }
            catch (Exception f)
            {
              msg = $"Fehler import Daten in Maschine {maschine.MaschinenName}\nGrund: {f.Message}";
              maschine.eProtokoll.FehlerDatenImport++;
              Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Warning);
              continue;
            }
            finally
            {
              pr.AuswertungEnde = DateTime.Now;
            }

            try
            {
              msg = $"Protokoll für Maschine {maschine.MaschinenName} gesichert.";
              Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Verbose);
            }
            catch (Exception f)
            {
              msg = $"Fehler beim speichern des Maschinenprotokolls: {maschine.MaschinenName}\nGrund: {f.Message}";
              Logger.Write(msg, "Service", 1, 0, System.Diagnostics.TraceEventType.Error);
              break;
            }
          }
        }
      }
    }
  }
}
