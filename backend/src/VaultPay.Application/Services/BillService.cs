using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.DTOs.Bills;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;
using VaultPay.Domain.Exceptions;

namespace VaultPay.Application.Services;

public class BillService : IBillService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public BillService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PayBillResponse> PayBillAsync(PayBillRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        if (request.Amount <= 0)
        {
            throw new InvalidAmountException("Bill payment amount must be greater than zero.");
        }

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);
        if (account == null)
        {
            throw new DomainException("Source account not found.");
        }

        if (account.UserId != userId && !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Account does not belong to the authenticated user.");
        }

        // Debit the account
        account.Debit(request.Amount);

        var refCode = $"VP-BILL-{request.Type.ToString().ToUpper()}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        var tokenOrReceipt = request.Type switch
        {
            BillType.Electricity => $"{Random.Shared.Next(1000, 9999)}-{Random.Shared.Next(1000, 9999)}-{Random.Shared.Next(1000, 9999)}-{Random.Shared.Next(1000, 9999)}-{Random.Shared.Next(1000, 9999)}",
            BillType.Airtime => $"AIR-{Random.Shared.Next(10000000, 99999999)}",
            BillType.Data => $"DAT-{Random.Shared.Next(10000000, 99999999)}",
            BillType.Betting => $"BET-{request.Provider.ToUpper()}-{Random.Shared.Next(100000, 999999)}",
            _ => $"VP-{Random.Shared.Next(100000, 999999)}"
        };

        var description = $"{request.Type} recharge via {request.Provider} for {request.CustomerIdentifier}" + 
                          (!string.IsNullOrWhiteSpace(request.PackageName) ? $" ({request.PackageName})" : "");

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Reference = refCode,
            FromAccountId = account.Id,
            Amount = request.Amount,
            CurrencyCode = account.CurrencyCode,
            Type = TransactionType.BillPayment,
            Status = TransactionStatus.Completed,
            Description = description,
            InitiatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        var ledgerEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            AccountId = account.Id,
            EntryType = LedgerEntryType.Debit,
            Amount = request.Amount,
            CurrencyCode = account.CurrencyCode,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        _context.LedgerEntries.Add(ledgerEntry);
        await _context.SaveChangesAsync(cancellationToken);

        return new PayBillResponse
        {
            TransactionId = transaction.Id,
            Reference = refCode,
            Amount = request.Amount,
            Provider = request.Provider,
            CustomerIdentifier = request.CustomerIdentifier,
            Status = "Completed",
            TokenOrReceiptCode = tokenOrReceipt,
            BalanceAfter = account.Balance,
            CreatedAt = transaction.CreatedAt
        };
    }
}
