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
    
    public partial class tabStandort
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tabStandort()
        {
            this.AuszahlungGehalt = 1;
            this.sMaschinen = new HashSet<tabMaschine>();
            this.sArbeitszeiten = new HashSet<tabArbeitszeit>();
            this.sBediener = new HashSet<tabBediener>();
            this.sArbeitzzeitRunden = new HashSet<tabArbeitszeitRunden>();
            this.DatenAbgleich = new DatenAbgleich();
        }
    
        public System.Guid Id { get; set; }
        public string Bezeichnung { get; set; }
        public byte AuszahlungGehalt { get; set; }
        public string Bemerkung { get; set; }
        public bool UpdateBedienerDatafox { get; set; }
    
        public DatenAbgleich DatenAbgleich { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabMaschine> sMaschinen { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabArbeitszeit> sArbeitszeiten { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabBediener> sBediener { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabArbeitszeitRunden> sArbeitzzeitRunden { get; set; }
    }
}
