using System.Text;
using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.DTOs.Accounts;
using VaultPay.Application.DTOs.Statements;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;
using VaultPay.Domain.Exceptions;

namespace VaultPay.Application.Services;

public class StatementService : IStatementService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public StatementService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<StatementDto> GenerateStatementAsync(StatementRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
        {
            throw new AccountNotFoundException(request.AccountId.ToString());
        }

        if (account.UserId != userId && !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Unauthorized to access statements for this account.");
        }

        var startDate = request.StartDate.HasValue
            ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc)
            : DateTime.UtcNow.AddMonths(-1);

        var endDate = request.EndDate.HasValue
            ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc)
            : DateTime.UtcNow;

        if (startDate > endDate)
        {
            (startDate, endDate) = (endDate, startDate);
        }

        // Ledger entries before startDate to determine Opening Balance
        var priorEntries = await _context.LedgerEntries
            .AsNoTracking()
            .Where(le => le.AccountId == account.Id && le.CreatedAt < startDate)
            .ToListAsync(cancellationToken);

        decimal openingBalance = 0m;
        foreach (var pe in priorEntries)
        {
            if (pe.EntryType == LedgerEntryType.Credit) openingBalance += pe.Amount;
            else if (pe.EntryType == LedgerEntryType.Debit) openingBalance -= pe.Amount;
        }

        // Ledger entries within date range
        var periodEntries = await _context.LedgerEntries
            .AsNoTracking()
            .Include(le => le.Transaction)
            .Where(le => le.AccountId == account.Id && le.CreatedAt >= startDate && le.CreatedAt <= endDate)
            .OrderBy(le => le.CreatedAt)
            .ToListAsync(cancellationToken);

        decimal runningBalance = openingBalance;
        decimal totalDebits = 0m;
        decimal totalCredits = 0m;
        var statementEntries = new List<StatementEntryDto>();

        foreach (var entry in periodEntries)
        {
            if (entry.EntryType == LedgerEntryType.Credit)
            {
                runningBalance += entry.Amount;
                totalCredits += entry.Amount;
            }
            else
            {
                runningBalance -= entry.Amount;
                totalDebits += entry.Amount;
            }

            statementEntries.Add(new StatementEntryDto
            {
                Date = entry.CreatedAt,
                Reference = entry.Transaction?.Reference ?? "N/A",
                Description = entry.Transaction?.Description ?? "Transaction",
                EntryType = entry.EntryType.ToString(),
                Amount = entry.Amount,
                RunningBalance = runningBalance
            });
        }

        return new StatementDto
        {
            Account = new AccountDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                AccountName = account.AccountName,
                CurrencyCode = account.CurrencyCode,
                Balance = account.Balance,
                Status = account.Status,
                CreatedAt = account.CreatedAt
            },
            StartDate = startDate,
            EndDate = endDate,
            OpeningBalance = openingBalance,
            ClosingBalance = runningBalance,
            TotalDebits = totalDebits,
            TotalCredits = totalCredits,
            Entries = statementEntries
        };
    }

    public async Task<byte[]> GenerateCsvStatementAsync(StatementRequest request, CancellationToken cancellationToken = default)
    {
        var statement = await GenerateStatementAsync(request, cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("VaultPay Simulated Account Statement");
        sb.AppendLine($"Account Number,{statement.Account.AccountNumber}");
        sb.AppendLine($"Account Name,{statement.Account.AccountName}");
        sb.AppendLine($"Currency,{statement.Account.CurrencyCode}");
        sb.AppendLine($"Period,{statement.StartDate:yyyy-MM-dd} to {statement.EndDate:yyyy-MM-dd}");
        sb.AppendLine($"Opening Balance,{statement.OpeningBalance:F2}");
        sb.AppendLine($"Total Debits,{statement.TotalDebits:F2}");
        sb.AppendLine($"Total Credits,{statement.TotalCredits:F2}");
        sb.AppendLine($"Closing Balance,{statement.ClosingBalance:F2}");
        sb.AppendLine();
        sb.AppendLine("Date (UTC),Reference,Description,Type,Amount,Running Balance");

        foreach (var entry in statement.Entries)
        {
            var desc = entry.Description.Replace("\"", "\"\"");
            sb.AppendLine($"{entry.Date:yyyy-MM-dd HH:mm:ss},{entry.Reference},\"{desc}\",{entry.EntryType},{entry.Amount:F2},{entry.RunningBalance:F2}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
