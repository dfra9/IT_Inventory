﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DBIT_Inventory
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using IT_Inventory;

    public partial class DBIT_Inventory : DbContext
    {
        public DBIT_Inventory()
            : base("name=DBIT_Inventory")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<C__EFMigrationsHistory> C__EFMigrationsHistory { get; set; }
        public virtual DbSet<Asset> Asset { get; set; }
        public virtual DbSet<Asset_History> Asset_History { get; set; }
        public virtual DbSet<City> City { get; set; }
        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<Departement> Departement { get; set; }
        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<Log_Transaction> Log_Transaction { get; set; }
        public virtual DbSet<Material_Code> Material_Code { get; set; }
        public virtual DbSet<Material_Group> Material_Group { get; set; }
        public virtual DbSet<UoM> UoM { get; set; }
        public virtual DbSet<Users> Users { get; set; }
    }
}
