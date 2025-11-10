// ==========================================================
// Project: LedgerApp
// File: LedgerDbContext.cs
// Description: DbContext básico (mantido para compatibilidade, sem navegações).
// Author: Matheus Bittencourt
// Date: 06/11/2025
// ==========================================================

using LedgerApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LedgerApp.Data;

/// DbContext mínimo apenas para compatibilidade
public class LedgerDbContext : DbContext
{
    public LedgerDbContext(DbContextOptions<LedgerDbContext> options) : base(options) { }

    // DbSets opcionais 
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    // Configurações leves, sem navegação Account em Transaction
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tabela Accounts
        modelBuilder.Entity<Account>(e =>
        {
            e.ToTable("Accounts");
            e.HasKey(x => x.Id);
            e.Property(x => x.OwnerName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Balance).HasColumnType("decimal(18,2)");
            e.Property(x => x.Number).IsRequired();
        });

        // Tabela Transactions (sem navegar para Account)
        modelBuilder.Entity<Transaction>(e =>
        {
            e.ToTable("Transactions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.OccurredAt).IsRequired();
            e.Property(x => x.AccountId).IsRequired();
            e.Property(x => x.Type).IsRequired(); 
        });
    }
}
