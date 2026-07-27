using Microsoft.EntityFrameworkCore;
using TransferPlatform.Data.Entities;

namespace TransferPlatform.Data.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LedgerEntry>()
            .HasIndex(x => x.RequestId)
            .IsUnique();

        modelBuilder.Entity<LedgerEntry>()
            .HasIndex(x => x.FromAccountId);
        
        modelBuilder.Entity<LedgerEntry>()
            .HasIndex(x => x.ToAccountId);

        modelBuilder.Entity<LedgerEntry>()
            .HasIndex(x => x.CreatedUtc);

        modelBuilder.Entity<Account>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<LedgerEntry>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<LedgerEntry>()
            .HasOne(x => x.FromAccount)
            .WithMany(x => x.OutgoingTransfers)
            .HasForeignKey(x => x.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<LedgerEntry>()
            .HasOne(x => x.ToAccount)
            .WithMany(x => x.IncomingTransfers)
            .HasForeignKey(x => x.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Account>()
            .Property(x => x.Balance)
            .HasColumnType("numeric(18,2)");

        modelBuilder.Entity<LedgerEntry>()
            .Property(x => x.Amount)
            .HasColumnType("numeric(18,2)");
    }
}