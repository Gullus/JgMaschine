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
    
    public partial class tabReparatur
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tabReparatur()
        {
            this.sAnmeldungen = new HashSet<tabAnmeldungReparatur>();
            this.eAktivMaschine = new HashSet<tabMaschine>();
            this.DatenAbgleich = new DatenAbgleich();
        }
    
        public System.Guid Id { get; set; }
        public System.DateTime VorgangBeginn { get; set; }
        public Nullable<System.DateTime> VorgangEnde { get; set; }
        public string ProtokollText { get; set; }
        public Nullable<byte> CoilwechselAnzahl { get; set; }
        public EnumReperaturEreigniss Ereigniss { get; set; }
        public System.Guid fMaschine { get; set; }
        public Nullable<System.Guid> fVerursacher { get; set; }
        public Nullable<System.Guid> fProtokollant { get; set; }
    
        public DatenAbgleich DatenAbgleich { get; set; }
    
        public virtual tabBediener eVerursacher { get; set; }
        public virtual tabBediener eProtokollant { get; set; }
        public virtual tabMaschine eMaschine { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabAnmeldungReparatur> sAnmeldungen { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabMaschine> eAktivMaschine { get; set; }
    }
}
