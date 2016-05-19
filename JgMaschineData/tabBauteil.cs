//------------------------------------------------------------------------------
// <auto-generated>
//     Der Code wurde von einer Vorlage generiert.
//
//     Manuelle Änderungen an dieser Datei führen möglicherweise zu unerwartetem Verhalten der Anwendung.
//     Manuelle Änderungen an dieser Datei werden überschrieben, wenn der Code neu generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JgMaschineData
{
    using System;
    using System.Collections.Generic;
    
    public partial class tabBauteil
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tabBauteil()
        {
            this.IdStahlPosition = -1;
            this.IdStahlBauteil = -1;
            this.BtAnzahl = -1;
            this.BtLaenge = -1;
            this.BtGewicht = -1;
            this.BtDurchmesser = -1;
            this.sBediener = new HashSet<tabBediener>();
            this.eBauteilAktuell = new HashSet<tabMaschine>();
            this.DatenAbgleich = new DatenAbgleich();
        }
    
        public System.Guid Id { get; set; }
        public System.DateTime DatumStart { get; set; }
        public System.DateTime DatumEnde { get; set; }
        public int IdStahlPosition { get; set; }
        public int IdStahlBauteil { get; set; }
        public int BtAnzahl { get; set; }
        public int BtLaenge { get; set; }
        public int BtGewicht { get; set; }
        public int BtDurchmesser { get; set; }
        public string Kunde { get; set; }
        public string Auftrag { get; set; }
        public string NummerBauteil { get; set; }
        public string NummerPosition { get; set; }
        public string Buegelname { get; set; }
        public bool IstHandeingabe { get; set; }
        public byte AnzahlBediener { get; set; }
        public byte AnzahlBiegungen { get; set; }
        public System.Guid fMaschine { get; set; }
    
        public DatenAbgleich DatenAbgleich { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabBediener> sBediener { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabMaschine> eBauteilAktuell { get; set; }
        public virtual tabMaschine eMaschine { get; set; }
    }
}
