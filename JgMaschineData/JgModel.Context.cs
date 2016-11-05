﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class JgModelContainer : DbContext
    {
        public JgModelContainer()
            : base("name=JgModelContainer")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tabMaschine> tabMaschineSet { get; set; }
        public virtual DbSet<tabBediener> tabBedienerSet { get; set; }
        public virtual DbSet<tabBauteil> tabBauteilSet { get; set; }
        public virtual DbSet<tabAnmeldungMaschine> tabAnmeldungMaschineSet { get; set; }
        public virtual DbSet<tabProtokoll> tabProtokollSet { get; set; }
        public virtual DbSet<tabReparatur> tabReparaturSet { get; set; }
        public virtual DbSet<tabStandort> tabStandortSet { get; set; }
        public virtual DbSet<tabAuswertung> tabAuswertungSet { get; set; }
        public virtual DbSet<tabArbeitszeit> tabArbeitszeitSet { get; set; }
        public virtual DbSet<tabAnmeldungReparatur> tabAnmeldungReparaturSet { get; set; }
        public virtual DbSet<tabArbeitszeitTag> tabArbeitszeitTagSet { get; set; }
        public virtual DbSet<tabFeiertage> tabFeiertageSet { get; set; }
        public virtual DbSet<tabArbeitszeitAuswertung> tabArbeitszeitAuswertungSet { get; set; }
        public virtual DbSet<tabSollStunden> tabSollStundenSet { get; set; }
    
        public virtual ObjectResult<Nullable<int>> BauteilInDaten(Nullable<System.DateTime> datum, Nullable<int> idPosition, Nullable<int> idMaschine)
        {
            var datumParameter = datum.HasValue ?
                new ObjectParameter("Datum", datum) :
                new ObjectParameter("Datum", typeof(System.DateTime));
    
            var idPositionParameter = idPosition.HasValue ?
                new ObjectParameter("IdPosition", idPosition) :
                new ObjectParameter("IdPosition", typeof(int));
    
            var idMaschineParameter = idMaschine.HasValue ?
                new ObjectParameter("IdMaschine", idMaschine) :
                new ObjectParameter("IdMaschine", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("BauteilInDaten", datumParameter, idPositionParameter, idMaschineParameter);
        }
    }
}
