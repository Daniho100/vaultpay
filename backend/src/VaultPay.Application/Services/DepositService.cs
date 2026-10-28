using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.DTOs.Deposits;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;
using VaultPay.Domain.Exceptions;

namespace VaultPay.Application.Services;

public class DepositService : IDepositService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdempotencyService _idempotencyService;
    private readonly IAuditService _auditService;

    public const decimal MaxSingleDepositAmount = 1_000_000m;

    public DepositService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IIdempotencyService idempotencyService,
        IAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _idempotencyService = idempotencyService;
        _auditService = auditService;
    }

    public async Task<DepositResponseDto> ProcessDepositAsync(DepositRequest request, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new DomainException("Idempotency-Key header is mandatory for deposit operations.");
        }

        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // 1. Check idempotency record
        var existingRecord = await _idempotencyService.GetRecordAsync(userId, "DEPOSIT", idempotencyKey, cancellationToken);
        if (existingRecord != null)
        {
            if (existingRecord.Status == IdempotencyStatus.Completed && !string.IsNullOrEmpty(existingRecord.ResponseData))
            {
                var cachedResponse = JsonSerializer.Deserialize<DepositResponseDto>(existingRecord.ResponseData);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
            }

            if (existingRecord.Status == IdempotencyStatus.Processing)
            {
                throw new IdempotencyConflictException("A deposit request with this Idempotency-Key is currently being processed.");
            }
        }

        // 2. Validate deposit amount limits
        if (request.Amount <= 0)
        {
            throw new InvalidAmountException("Deposit amount must be strictly positive.");
        }

        if (request.Amount > MaxSingleDepositAmount)
        {
            throw new InvalidAmountException($"Simulated deposit amount cannot exceed maximum limit of {MaxSingleDepositAmount:N2} NGN.");
        }

        var fingerprint = $"{request.AccountId}:{request.Amount:F2}:{request.Description}";
        var idempotencyRecord = await _idempotencyService.CreateRecordAsync(userId, "DEPOSIT", idempotencyKey, fingerprint, cancellationToken);

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            // 3. Acquire lock and check account
            var account = await _context.GetAccountForUpdateAsync(request.AccountId, cancellationToken);
            if (account == null)
            {
                throw new AccountNotFoundException($"Account '{request.AccountId}' does not exist.");
            }

            if (account.UserId != userId && !_currentUserService.IsAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to deposit into this account.");
            }

            account.EnsureActive();

            // 4. Update balance
            account.Credit(request.Amount);

            // 5. Create Transaction record
            var reference = $"VP-DEP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
            var txn = new Transaction
            {
                Reference = reference,
                ToAccountId = account.Id,
                Amount = request.Amount,
                CurrencyCode = account.CurrencyCode,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Completed,
                Description = string.IsNullOrWhiteSpace(request.Description) ? "Simulated Demo Deposit" : request.Description.Trim(),
                InitiatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(txn);

            // 6. Record Credit Ledger Entry
            var creditEntry = new LedgerEntry
            {
                Transaction = txn,
                AccountId = account.Id,
                EntryType = LedgerEntryType.Credit,
                Amount = request.Amount,
                CurrencyCode = account.CurrencyCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.LedgerEntries.Add(creditEntry);

            await _context.SaveChangesAsync(cancellationToken);

            var responseDto = new DepositResponseDto
            {
                TransactionId = txn.Id,
                Reference = txn.Reference,
                Amount = txn.Amount,
                CurrencyCode = txn.CurrencyCode,
                Status = txn.Status,
                AccountNumber = account.AccountNumber,
                AccountName = account.AccountName,
                BalanceAfterDeposit = account.Balance,
                Description = txn.Description,
                CreatedAt = txn.CreatedAt,
                CompletedAt = txn.CompletedAt,
                IsDemoFunding = true
            };

            var serializedResponse = JsonSerializer.Serialize(responseDto);
            await _idempotencyService.CompleteRecordAsync(idempotencyRecord.Id, serializedResponse, 200, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            await _auditService.LogAsync(
                userId,
                "DEMO_DEPOSIT_COMPLETED",
                "Transaction",
                txn.Id.ToString(),
                $"Deposited simulated demo funds of {request.Amount:N2} {account.CurrencyCode} to {account.AccountNumber}",
                cancellationToken: cancellationToken);

            return responseDto;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            await _idempotencyService.FailRecordAsync(idempotencyRecord.Id, ex.Message, 400, cancellationToken);
            throw;
        }
    }
}
