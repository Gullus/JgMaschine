using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JgMaschineData
{
  public partial class tabMaschineMetaData
  {
    [Required]
    public object MaschinenName;
  }

  [MetadataType(typeof(tabMaschineMetaData))]
  public partial class tabMaschine
  {
    public bool Abbruch { get; set; }

    public string ProtokollAdd
    {
      get { return (this.eProtokoll == null) ? "" : this.eProtokoll.ProtokollText; }
      set
      {
        if (!string.IsNullOrWhiteSpace(value))
        {
          var lines = value.Split(new string[] { "\n" }, StringSplitOptions.None);
          var erg = DateTime.Now.ToString("dd:MM HH:mm:ss ") + lines[0] + "\n";
          for (int i = 1; i < lines.Length; i++)
            erg += "--             " + lines[i] + "\n";

          this.eProtokoll.ProtokollText += erg;
        }
      }
    }
  }

  public partial class tabProtokollMetaData
  { }

  [MetadataType(typeof(tabProtokollMetaData))]
  public partial class tabProtokoll
  {
    public string Laufzeit
    {
      get { return ((this.AuswertungStart != null) && (this.AuswertungEnde != null)) ? (this.AuswertungEnde - this.AuswertungStart).ToString() : ""; }
      set { }
    }

    public string StatusAsString
    {
      get { return this.Status.ToString(); }
    }
  }

  public partial class tabBedienerMetaData
  { }

  [MetadataType(typeof(tabBedienerMetaData))]
  public partial class tabBediener
  {
    public string Name { get { return this.NachName + ", " + VorName; } }
  }

  public partial class tabReparaturMetaData
  { }

  [MetadataType(typeof(tabReparaturMetaData))]
  public partial class tabReparatur
  {
    public string Bediener
    {
      get
      {
        var ben = this.eMaschine.sAnmeldungen.Where(w => (w.Anmeldung < this.VorgangBeginn) && (w.Abmeldung > this.VorgangBeginn)).Select(s => s.eBediener.NachName + ", " + s.eBediener.VorName).Distinct().ToList();
        return string.Join("; ", ben); 
      }
      set { }
    }
  }
}
