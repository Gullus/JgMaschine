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
    
    public partial class tabArbeitszeit
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tabArbeitszeit()
        {
            this.ManuelleAnmeldung = true;
            this.ManuelleAbmeldung = true;
            this.eAktivBediener = new HashSet<tabBediener>();
            this.DatenAbgleich = new DatenAbgleich();
        }
    
        public System.Guid Id { get; set; }
        public Nullable<System.DateTime> Anmeldung { get; set; }
        public Nullable<System.DateTime> Abmeldung { get; set; }
        public bool ManuelleAnmeldung { get; set; }
        public bool ManuelleAbmeldung { get; set; }
        public System.Guid fBediener { get; set; }
        public System.Guid fStandort { get; set; }
        public Nullable<System.Guid> fArbeitszeitAuswertung { get; set; }
    
        public DatenAbgleich DatenAbgleich { get; set; }
    
        public virtual tabBediener eBediener { get; set; }
        public virtual tabStandort eStandort { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabBediener> eAktivBediener { get; set; }
        public virtual tabArbeitszeitTag eArbeitszeitAuswertung { get; set; }
    }
}
