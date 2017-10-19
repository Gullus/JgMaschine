using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace JgMaschineLib.SqlVerbindung
{
  public class _SqlVerbindung
  {
    public enum EnumVerbindungen
    {
      JgData,
      JgMaschine
    }
  
    private SortedDictionary<EnumVerbindungen, string> _Verbindungen = null;

    private string _FileNameDatei = AppDomain.CurrentDomain.BaseDirectory + @"JgMaschineSetup.Xml";
    public string FileNameDatei
    {
      get { return _FileNameDatei; }
      set { _FileNameDatei = value; }
    }

    public _SqlVerbindung(string FileName)
      : base()
    {
      _FileNameDatei = FileName;
    }

    public _SqlVerbindung()
    { }

    private void VerbindungenLaden()
    {
      if (_Verbindungen == null)
      {
        _Verbindungen = new SortedDictionary<EnumVerbindungen, string>();

        if (File.Exists(_FileNameDatei))
        {
          try
          {
            XDocument doc = XDocument.Load(_FileNameDatei);

            var ergList = (from v in doc.Root.Elements("Verbindung")
                           select new { key = (string)v.Attribute("Id"), value = (string)v.Element("VerbindungsString") });

            foreach (var erg in ergList)
              _Verbindungen.Add((EnumVerbindungen)Enum.Parse(typeof(EnumVerbindungen), erg.key), erg.value);
          }
          catch
          { }
        }
      }
    }

    public string Get(EnumVerbindungen Verbinung)
    {
      VerbindungenLaden();
      if (_Verbindungen.ContainsKey(Verbinung))
        return _Verbindungen[Verbinung]; 

      return null;
    }

    public void Set(EnumVerbindungen Verbinung, string VerbindungsString)
    {
      VerbindungenLaden();

      if (_Verbindungen.ContainsKey(Verbinung))
        _Verbindungen[Verbinung] = VerbindungsString;
      else
        _Verbindungen.Add(Verbinung, VerbindungsString);
    }

    public void Save(string SpeichernUnter = "")
    {
      if (SpeichernUnter == "")
        SpeichernUnter = _FileNameDatei;

      XDocument doc = new XDocument(
        new XDeclaration("1.0", "utf-8", "yes"),
        new XComment("Verbindungszeichenfolgen für JgMaschine"),
        new XElement("Verbindungen",
          from verb in _Verbindungen
          select new XElement("Verbindung", new XAttribute("Id", verb.Key),
            new XElement("VerbindungsString", verb.Value)
          )
        )
      );
      doc.Save(_FileNameDatei);
    }

    public void VerbindungBearbeiten(EnumVerbindungen Verbindung)
    {
      FormDatenbankVerbindung fo = new FormDatenbankVerbindung(Get(Verbindung));
      if (fo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        Set(Verbindung, fo.ConnectionString);
        Save();
      }
    }

    public static string FormVerbindung(string  VerbindungsString)
    {
      FormDatenbankVerbindung fo = new FormDatenbankVerbindung(VerbindungsString);
      if (fo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        return fo.ConnectionString;
      else
        return VerbindungsString;
    }
  }
}
