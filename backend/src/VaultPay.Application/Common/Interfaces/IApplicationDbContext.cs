using Microsoft.EntityFrameworkCore;
using VaultPay.Domain.Entities;

namespace VaultPay.Application.Common.Interfaces;

public interface IDbContextTransactionProxy : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<LedgerEntry> LedgerEntries { get; }
    DbSet<IdempotencyRecord> IdempotencyRecords { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Loan> Loans { get; }
    DbSet<FixedDeposit> FixedDeposits { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransactionProxy> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<Account?> GetAccountForUpdateAsync(Guid accountId, CancellationToken cancellationToken = default);
}
