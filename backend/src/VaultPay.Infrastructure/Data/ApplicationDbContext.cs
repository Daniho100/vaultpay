using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Domain.Entities;

namespace VaultPay.Infrastructure.Data;

public class DbContextTransactionProxy : IDbContextTransactionProxy
{
    private readonly IDbContextTransaction _transaction;

    public DbContextTransactionProxy(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }
}

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<FixedDeposit> FixedDeposits => Set<FixedDeposit>();

    public async Task<IDbContextTransactionProxy> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var txn = await Database.BeginTransactionAsync(cancellationToken);
        return new DbContextTransactionProxy(txn);
    }

    public async Task<Account?> GetAccountForUpdateAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Account configurations
        builder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.AccountNumber).IsUnique();
            entity.Property(a => a.AccountNumber).IsRequired().HasMaxLength(20);
            entity.Property(a => a.AccountName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.CurrencyCode).IsRequired().HasMaxLength(5);
            entity.Property(a => a.Balance).HasColumnType("decimal(18,2)");
            entity.Property(a => a.ConcurrencyToken).IsConcurrencyToken();

            entity.HasOne(a => a.User)
                  .WithMany(u => u.Accounts)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Transaction configurations
        builder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Reference).IsUnique();
            entity.Property(t => t.Reference).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.CurrencyCode).IsRequired().HasMaxLength(5);

            entity.HasOne(t => t.FromAccount)
                  .WithMany(a => a.OutgoingTransactions)
                  .HasForeignKey(t => t.FromAccountId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.ToAccount)
                  .WithMany(a => a.IncomingTransactions)
                  .HasForeignKey(t => t.ToAccountId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.InitiatedByUser)
                  .WithMany(u => u.InitiatedTransactions)
                  .HasForeignKey(t => t.InitiatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // LedgerEntry configurations
        builder.Entity<LedgerEntry>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Amount).HasColumnType("decimal(18,2)");
            entity.Property(l => l.CurrencyCode).IsRequired().HasMaxLength(5);

            entity.HasOne(l => l.Transaction)
                  .WithMany(t => t.LedgerEntries)
                  .HasForeignKey(l => l.TransactionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Account)
                  .WithMany(a => a.LedgerEntries)
                  .HasForeignKey(l => l.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // IdempotencyRecord
        builder.Entity<IdempotencyRecord>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => new { i.UserId, i.OperationType, i.IdempotencyKey }).IsUnique();
            entity.Property(i => i.IdempotencyKey).IsRequired().HasMaxLength(100);
            entity.Property(i => i.OperationType).IsRequired().HasMaxLength(50);
        });

        // AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Action).IsRequired().HasMaxLength(50);
            entity.Property(a => a.EntityType).IsRequired().HasMaxLength(50);
        });

        // Loan
        builder.Entity<Loan>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Amount).HasColumnType("decimal(18,2)");
            entity.Property(l => l.InterestRate).HasColumnType("decimal(5,2)");
            entity.HasOne(l => l.User)
                  .WithMany(u => u.Loans)
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(l => l.Account)
                  .WithMany()
                  .HasForeignKey(l => l.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // FixedDeposit
        builder.Entity<FixedDeposit>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Amount).HasColumnType("decimal(18,2)");
            entity.Property(f => f.InterestRatePerAnnum).HasColumnType("decimal(5,2)");
            entity.Property(f => f.PenaltyRate).HasColumnType("decimal(5,2)");
            entity.Property(f => f.ProjectedInterest).HasColumnType("decimal(18,2)");
            entity.Property(f => f.ActualPayout).HasColumnType("decimal(18,2)");
            entity.HasOne(f => f.User)
                  .WithMany(u => u.FixedDeposits)
                  .HasForeignKey(f => f.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(f => f.Account)
                  .WithMany()
                  .HasForeignKey(f => f.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
