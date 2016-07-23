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
    
    public partial class tabBediener
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tabBediener()
        {
            this.sBauteile = new HashSet<tabBauteil>();
            this.sArbeitszeiten = new HashSet<tabArbeitszeit>();
            this.sAnmeldungenMaschine = new HashSet<tabAnmeldungMaschine>();
            this.sAnmeldungenReparatur = new HashSet<tabAnmeldungReparatur>();
            this.sReparaturVerursacher = new HashSet<tabReparatur>();
            this.sReparaturProtokollanten = new HashSet<tabReparatur>();
            this.DatenAbgleich = new DatenAbgleich();
        }
    
        public System.Guid Id { get; set; }
        public string NachName { get; set; }
        public string VorName { get; set; }
        public string Bemerkung { get; set; }
        public string MatchCode { get; set; }
        public EnumStatusBediener Status { get; set; }
        public Nullable<System.Guid> fAktivArbeitszeit { get; set; }
        public System.Guid fStandort { get; set; }
    
        public DatenAbgleich DatenAbgleich { get; set; }
    
        public virtual tabStandort eStandort { get; set; }
        public virtual tabArbeitszeit eAktivArbeitszeit { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabBauteil> sBauteile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabArbeitszeit> sArbeitszeiten { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabAnmeldungMaschine> sAnmeldungenMaschine { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabAnmeldungReparatur> sAnmeldungenReparatur { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabReparatur> sReparaturVerursacher { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabReparatur> sReparaturProtokollanten { get; set; }
    }
}
