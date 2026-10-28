using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.DTOs.Savings;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;
using VaultPay.Domain.Exceptions;

namespace VaultPay.Application.Services;

public class SavingsService : ISavingsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SavingsService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<FixedDepositDto>> GetUserFixedDepositsAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var deposits = await _context.FixedDeposits
            .AsNoTracking()
            .Include(f => f.Account)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.StartDate)
            .ToListAsync(cancellationToken);

        return deposits.Select(f => new FixedDepositDto
        {
            Id = f.Id,
            UserId = f.UserId,
            AccountId = f.AccountId,
            AccountNumber = f.Account?.AccountNumber ?? string.Empty,
            Amount = f.Amount,
            InterestRatePerAnnum = f.InterestRatePerAnnum,
            DurationDays = f.DurationDays,
            StartDate = f.StartDate,
            MaturityDate = f.MaturityDate,
            Status = f.Status,
            PenaltyRate = f.PenaltyRate,
            ProjectedInterest = f.ProjectedInterest,
            ActualPayout = f.ActualPayout,
            ClosedAt = f.ClosedAt
        }).ToList();
    }

    public async Task<FixedDepositDto> CreateFixedDepositAsync(CreateFixedDepositRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        if (request.Amount <= 0)
        {
            throw new InvalidAmountException("Fixed deposit amount must be greater than zero.");
        }

        var durationDays = request.DurationDays <= 0 ? 90 : request.DurationDays;

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);
        if (account == null)
        {
            throw new DomainException("Funding account not found.");
        }

        if (account.UserId != userId && !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Account does not belong to the authenticated user.");
        }

        // Debit the principal from the account
        account.Debit(request.Amount);

        const decimal interestRate = 0.25m; // 25% per annum
        var projectedInterest = FixedDeposit.CalculateProjectedInterest(request.Amount, durationDays, interestRate);

        var deposit = new FixedDeposit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = account.Id,
            Amount = request.Amount,
            InterestRatePerAnnum = interestRate,
            DurationDays = durationDays,
            StartDate = DateTime.UtcNow,
            MaturityDate = DateTime.UtcNow.AddDays(durationDays),
            Status = FixedDepositStatus.Active,
            PenaltyRate = 0.10m, // 10% early withdrawal charge
            ProjectedInterest = projectedInterest
        };

        var refCode = $"VP-SAV-FIXED-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Reference = refCode,
            FromAccountId = account.Id,
            Amount = request.Amount,
            CurrencyCode = account.CurrencyCode,
            Type = TransactionType.FixedDepositCreation,
            Status = TransactionStatus.Completed,
            Description = $"Created 25% p.a. Fixed Deposit for {durationDays} days (Projected interest: ₦{projectedInterest:N2})",
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

        _context.FixedDeposits.Add(deposit);
        _context.Transactions.Add(transaction);
        _context.LedgerEntries.Add(ledgerEntry);
        await _context.SaveChangesAsync(cancellationToken);

        return new FixedDepositDto
        {
            Id = deposit.Id,
            UserId = deposit.UserId,
            AccountId = deposit.AccountId,
            AccountNumber = account.AccountNumber,
            Amount = deposit.Amount,
            InterestRatePerAnnum = deposit.InterestRatePerAnnum,
            DurationDays = deposit.DurationDays,
            StartDate = deposit.StartDate,
            MaturityDate = deposit.MaturityDate,
            Status = deposit.Status,
            PenaltyRate = deposit.PenaltyRate,
            ProjectedInterest = deposit.ProjectedInterest
        };
    }

    public async Task<LiquidateDepositResponse> LiquidateFixedDepositAsync(Guid depositId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var deposit = await _context.FixedDeposits
            .Include(f => f.Account)
            .FirstOrDefaultAsync(f => f.Id == depositId, cancellationToken);

        if (deposit == null)
        {
            throw new DomainException("Fixed deposit not found.");
        }

        if (deposit.UserId != userId && !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Fixed deposit does not belong to the authenticated user.");
        }

        if (deposit.Status != FixedDepositStatus.Active)
        {
            throw new DomainException("Fixed deposit is not currently active.");
        }

        var isEarly = DateTime.UtcNow < deposit.MaturityDate;
        decimal penalty = 0m;
        decimal netPayout = 0m;
        string message;

        if (isEarly)
        {
            // Charged 10% early withdrawal charge for cancelling fixed savings
            penalty = Math.Round(deposit.Amount * deposit.PenaltyRate, 2);
            netPayout = deposit.Amount - penalty;
            deposit.Status = FixedDepositStatus.LiquidatedEarly;
            message = $"Fixed deposit cancelled early. A 10% penalty of ₦{penalty:N2} was charged. ₦{netPayout:N2} has been returned to your account.";
        }
        else
        {
            // Matured: full principal + 25% p.a. accrued interest
            netPayout = deposit.Amount + deposit.ProjectedInterest;
            deposit.Status = FixedDepositStatus.Matured;
            message = $"Fixed deposit matured successfully! Total payout of ₦{netPayout:N2} (Principal + Interest) credited to your account.";
        }

        deposit.ActualPayout = netPayout;
        deposit.ClosedAt = DateTime.UtcNow;

        // Credit the payout amount to the destination account
        var account = deposit.Account ?? await _context.Accounts.FirstOrDefaultAsync(a => a.Id == deposit.AccountId, cancellationToken);
        if (account == null)
        {
            throw new DomainException("Destination account not found.");
        }

        account.Credit(netPayout);

        var refCode = $"VP-SAV-LIQ-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Reference = refCode,
            ToAccountId = account.Id,
            Amount = netPayout,
            CurrencyCode = account.CurrencyCode,
            Type = TransactionType.FixedDepositLiquidation,
            Status = TransactionStatus.Completed,
            Description = isEarly 
                ? $"Early liquidation of Fixed Deposit (10% Penalty: ₦{penalty:N2})"
                : "Matured Fixed Deposit Payout (Principal + 25% p.a. Interest)",
            InitiatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        var ledgerEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            AccountId = account.Id,
            EntryType = LedgerEntryType.Credit,
            Amount = netPayout,
            CurrencyCode = account.CurrencyCode,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        _context.LedgerEntries.Add(ledgerEntry);
        await _context.SaveChangesAsync(cancellationToken);

        return new LiquidateDepositResponse
        {
            Id = deposit.Id,
            PrincipalAmount = deposit.Amount,
            PenaltyDeducted = penalty,
            NetPayoutRefunded = netPayout,
            AccountBalanceAfter = account.Balance,
            WasEarlyWithdrawal = isEarly,
            Message = message
        };
    }
}
