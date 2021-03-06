﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WindowsFormsApplication1
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EXANTE_Entities : DbContext
    {
        public EXANTE_Entities(string s)
            : base("name=EXANTE_Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Ctrade> Ctrades { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<counterparty> counterparties { get; set; }
        public DbSet<CpTrade> CpTrades { get; set; }
        public DbSet<PenPaperTrade> PenPaperTrades { get; set; }
        public DbSet<Mapping> Mappings { get; set; }
        public DbSet<Reconcilation> Reconcilations { get; set; }
        public DbSet<ABN_cashposition> ABN_cashposition { get; set; }
        public DbSet<DailyCheck> DailyChecks { get; set; }
        public DbSet<ABNReconResult> ABNReconResults { get; set; }
        public DbSet<CpPosition> CpPositions { get; set; }
        public DbSet<Ftbo> Ftboes { get; set; }
        public DbSet<DBBORecon_mapping> DBBORecon_mapping { get; set; }
        public DbSet<ColumnMapping> ColumnMappings { get; set; }
        public DbSet<InitialTrade> InitialTrades { get; set; }
        public DbSet<param> @params { get; set; }
        public DbSet<ADSSCashGroupped> ADSSCashGroupped { get; set; }
        public DbSet<RowBalance> RowBalance { get; set; }
        public DbSet<CashMapping> CashMapping { get; set; }
        public DbSet<CorporateActions> CorporateActions { get; set; }
        public DbSet<QtyByAccounts> QtyByAccounts { get; set; }
        public DbSet<cpmapping> cpmapping { get; set; }
        public DbSet<CpCost> CpCost { get; set; }
        public DbSet<FT> FT { get; set; }
        public DbSet<Emir> Emir { get; set; }
        public DbSet<Emir_Mapping> Emir_Mapping { get; set; }
        public DbSet<md_swaps_googlespsh> md_swaps_googlespsh { get; set; }
        public DbSet<RJO_listccy> RJO_listccy { get; set; }
        public DbSet<Axi_SettlingTrades> Axi_SettlingTrades { get; set; }
        public DbSet<Axi_Trades> Axi_Trades { get; set; }
        public DbSet<Axi_RolloverTrades> Axi_RolloverTrades { get; set; }
    }
}
