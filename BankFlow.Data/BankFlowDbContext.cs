
using BankFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace BankFlow.Data
{
    public class BankFlowDbContext : DbContext
    {
        public BankFlowDbContext(
            DbContextOptions<BankFlowDbContext> options)
            : base(options)
        {
        }

        public DbSet<BankAccount> BankAccounts
            => Set<BankAccount>();

        public DbSet<BankTransaction> BankTransactions
            => Set<BankTransaction>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BankAccount>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BankAccount>()
                .HasIndex(a => a.AccountNumber)
                .IsUnique();

            modelBuilder.Entity<BankAccount>()
                .Property(a => a.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<BankTransaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BankTransaction>()
                .Property(t => t.TransactionType)
                .HasMaxLength(20)
                .IsRequired();
        }
    }
}
