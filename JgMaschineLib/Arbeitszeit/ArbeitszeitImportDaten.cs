using System;

namespace JgMaschineLib.Arbeitszeit
{
  public class ArbeitszeitImportDaten
  {
    public enum EnumVorgang
    {
      Fehler,
      Komme,
      Gehen,
      Pause
    }

    public string MatchCode { get; set; }
    public DateTime Datum { get; set; }
    public Guid? IdStandort { get; set; } = null;
    public EnumVorgang Vorgang = EnumVorgang.Fehler;
    public string Version { get; set; }   // Optional
    public int GehGrund = -1;             // Optional

    public ArbeitszeitImportDaten()
    { }

    public ArbeitszeitImportDaten(string NeuMacthchCode, DateTime NeuDatum, Guid NeuIdStandort, EnumVorgang NeuVorgang)
    {
      MatchCode = NeuMacthchCode;
      Datum = NeuDatum;
      IdStandort = NeuIdStandort;
      Vorgang = NeuVorgang;
    }
  }
}
