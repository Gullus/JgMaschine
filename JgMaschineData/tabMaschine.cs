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
    
    public partial class tabMaschine
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tabMaschine()
        {
            this.VorgabeProStunde = 0m;
            this.sAnmeldungen = new HashSet<tabAnmeldungMaschine>();
            this.sBauteile = new HashSet<tabBauteil>();
            this.sReparaturen = new HashSet<tabReparatur>();
            this.sAktiveAnmeldungen = new HashSet<tabAnmeldungMaschine>();
            this.DatenAbgleich = new DatenAbgleich();
        }
    
        public System.Guid Id { get; set; }
        public string MaschinenName { get; set; }
        public EnumProtokollName ProtokollName { get; set; }
        public bool IstStangenschneider { get; set; }
        public string MaschineAdresse { get; set; }
        public Nullable<int> MaschinePortnummer { get; set; }
        public string PfadDaten { get; set; }
        public string PfadBediener { get; set; }
        public string ScannerNummer { get; set; }
        public bool ScannerMitDisplay { get; set; }
        public string Bemerkung { get; set; }
        public EnumStatusMaschine Status { get; set; }
        public decimal VorgabeProStunde { get; set; }
        public System.Guid fStandort { get; set; }
        public Nullable<System.Guid> fAktivBauteil { get; set; }
        public Nullable<System.Guid> fAktivReparatur { get; set; }
    
        public DatenAbgleich DatenAbgleich { get; set; }
    
        public virtual tabProtokoll eProtokoll { get; set; }
        public virtual tabStandort eStandort { get; set; }
        public virtual tabBauteil eAktivBauteil { get; set; }
        public virtual tabReparatur eAktivReparatur { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabAnmeldungMaschine> sAnmeldungen { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabBauteil> sBauteile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabReparatur> sReparaturen { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tabAnmeldungMaschine> sAktiveAnmeldungen { get; set; }
    }
}
