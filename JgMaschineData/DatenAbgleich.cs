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
    
    public partial class DatenAbgleich
    {
        public DatenAbgleich()
        {
            this.Geloescht = false;
        }
    
        public System.DateTime Datum { get; set; }
        public EnumStatusDatenabgleich Status { get; set; }
        public string Bearbeiter { get; set; }
        public bool Geloescht { get; set; }
    }
}
