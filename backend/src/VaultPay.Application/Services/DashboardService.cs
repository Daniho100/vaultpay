using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.DTOs.Accounts;
using VaultPay.Application.DTOs.Dashboard;
using VaultPay.Application.DTOs.Transactions;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;

namespace VaultPay.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DashboardService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var accounts = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        var accountIds = accounts.Select(a => a.Id).ToList();

        var totalBalance = accounts.Sum(a => a.Balance);

        // Recent transactions
        var recentTxns = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Where(t =>
                (t.FromAccountId.HasValue && accountIds.Contains(t.FromAccountId.Value)) ||
                (t.ToAccountId.HasValue && accountIds.Contains(t.ToAccountId.Value)))
            .OrderByDescending(t => t.CreatedAt)
            .Take(6)
            .ToListAsync(cancellationToken);

        var depositAmounts = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.ToAccountId.HasValue && accountIds.Contains(t.ToAccountId.Value) && t.Type == TransactionType.Deposit && t.Status == TransactionStatus.Completed)
            .Select(t => t.Amount)
            .ToListAsync(cancellationToken);
        var depositsTotal = depositAmounts.Sum();

        var transferAmounts = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.FromAccountId.HasValue && accountIds.Contains(t.FromAccountId.Value) && t.Type == TransactionType.Transfer && t.Status == TransactionStatus.Completed)
            .Select(t => t.Amount)
            .ToListAsync(cancellationToken);
        var transfersTotal = transferAmounts.Sum();

        return new DashboardSummaryDto
        {
            TotalBalance = totalBalance,
            PrimaryCurrency = accounts.FirstOrDefault()?.CurrencyCode ?? "NGN",
            ActiveAccountsCount = accounts.Count(a => a.Status == AccountStatus.Active),
            TotalDepositsAmount = depositsTotal,
            TotalTransfersAmount = transfersTotal,
            Accounts = accounts.Select(a => new AccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                AccountName = a.AccountName,
                CurrencyCode = a.CurrencyCode,
                Balance = a.Balance,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            }).ToList(),
            RecentTransactions = recentTxns.Select(t => new TransactionDto
            {
                Id = t.Id,
                Reference = t.Reference,
                FromAccountId = t.FromAccountId,
                FromAccountNumber = t.FromAccount?.AccountNumber,
                FromAccountName = t.FromAccount?.AccountName,
                ToAccountId = t.ToAccountId,
                ToAccountNumber = t.ToAccount?.AccountNumber,
                ToAccountName = t.ToAccount?.AccountName,
                Amount = t.Amount,
                CurrencyCode = t.CurrencyCode,
                Type = t.Type,
                Status = t.Status,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt,
                IsDebit = t.FromAccountId.HasValue && accountIds.Contains(t.FromAccountId.Value)
            }).ToList()
        };
    }

    public async Task<DashboardAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var accountIds = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var ledgerEntries = await _context.LedgerEntries
            .AsNoTracking()
            .Where(le => accountIds.Contains(le.AccountId) && le.CreatedAt >= thirtyDaysAgo)
            .ToListAsync(cancellationToken);

        decimal monthlyInflow = 0m;
        decimal monthlyOutflow = 0m;

        // Group by day for charts
        var dailyMap = new Dictionary<string, (decimal Inflow, decimal Outflow)>();
        for (int i = 29; i >= 0; i--)
        {
            var dayKey = DateTime.UtcNow.AddDays(-i).ToString("MMM dd");
            dailyMap[dayKey] = (0m, 0m);
        }

        foreach (var entry in ledgerEntries)
        {
            var dayKey = entry.CreatedAt.ToString("MMM dd");
            var current = dailyMap.ContainsKey(dayKey) ? dailyMap[dayKey] : (0m, 0m);

            if (entry.EntryType == LedgerEntryType.Credit)
            {
                monthlyInflow += entry.Amount;
                dailyMap[dayKey] = (current.Item1 + entry.Amount, current.Item2);
            }
            else
            {
                monthlyOutflow += entry.Amount;
                dailyMap[dayKey] = (current.Item1, current.Item2 + entry.Amount);
            }
        }

        var activityPoints = dailyMap.Select(kv => new ActivityPointDto
        {
            Date = kv.Key,
            Inflow = kv.Value.Item1,
            Outflow = kv.Value.Item2
        }).ToList();

        return new DashboardAnalyticsDto
        {
            MonthlyInflow = monthlyInflow,
            MonthlyOutflow = monthlyOutflow,
            ActivityPoints = activityPoints
        };
    }
}
