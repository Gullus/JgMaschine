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
    
    public partial class tabPausenzeit
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tabPausenzeit()
        {
            this.DatenAbgleich = new DatenAbgleich();
        }
    
        public System.Guid Id { get; set; }
        public System.TimeSpan ZeitVon { get; set; }
        public System.TimeSpan ZeitBis { get; set; }
        public System.TimeSpan Pausenzeit { get; set; }
    
        public DatenAbgleich DatenAbgleich { get; set; }
    }
}