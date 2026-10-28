using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.DTOs.Transfers;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;
using VaultPay.Domain.Exceptions;

namespace VaultPay.Application.Services;

public class TransferService : ITransferService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdempotencyService _idempotencyService;
    private readonly IAuditService _auditService;

    public const decimal MaxSingleTransferAmount = 5_000_000m;

    public TransferService(
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

    public async Task<TransferResponseDto> ProcessTransferAsync(TransferRequest request, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new DomainException("Idempotency-Key header is mandatory for transfer operations.");
        }

        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // 1. Check Idempotency Record
        var existingRecord = await _idempotencyService.GetRecordAsync(userId, "TRANSFER", idempotencyKey, cancellationToken);
        if (existingRecord != null)
        {
            if (existingRecord.Status == IdempotencyStatus.Completed && !string.IsNullOrEmpty(existingRecord.ResponseData))
            {
                var cachedResponse = JsonSerializer.Deserialize<TransferResponseDto>(existingRecord.ResponseData);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
            }

            if (existingRecord.Status == IdempotencyStatus.Processing)
            {
                throw new IdempotencyConflictException("A transfer request with this Idempotency-Key is currently being processed.");
            }
        }

        // Validate basic request fields
        if (request.Amount <= 0)
        {
            throw new InvalidAmountException("Transfer amount must be strictly positive.");
        }

        if (request.Amount > MaxSingleTransferAmount)
        {
            throw new InvalidAmountException($"Transfer amount cannot exceed maximum limit of {MaxSingleTransferAmount:N2} NGN.");
        }

        var fingerprint = $"{request.FromAccountId}:{request.RecipientAccountNumber}:{request.Amount:F2}:{request.Description}";
        var idempotencyRecord = await _idempotencyService.CreateRecordAsync(userId, "TRANSFER", idempotencyKey, fingerprint, cancellationToken);

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            // 2. Fetch and verify source account
            var sourceAccount = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == request.FromAccountId, cancellationToken);

            if (sourceAccount == null)
            {
                throw new AccountNotFoundException($"Source account '{request.FromAccountId}' does not exist.");
            }

            if (sourceAccount.UserId != userId && !_currentUserService.IsAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to debit this account.");
            }

            sourceAccount.EnsureActive();

            // 3. Fetch and verify recipient account
            var recipientAccount = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountNumber == request.RecipientAccountNumber.Trim(), cancellationToken);

            if (recipientAccount == null)
            {
                throw new AccountNotFoundException($"Recipient account number '{request.RecipientAccountNumber}' does not exist.");
            }

            if (sourceAccount.Id == recipientAccount.Id)
            {
                throw new SelfTransferException();
            }

            recipientAccount.EnsureActive();

            if (!string.Equals(sourceAccount.CurrencyCode, recipientAccount.CurrencyCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new CurrencyMismatchException(sourceAccount.CurrencyCode, recipientAccount.CurrencyCode);
            }

            // 4. Deterministic Account Locking to Prevent Deadlocks: Lock smaller GUID first, then larger
            var firstAccountId = sourceAccount.Id.CompareTo(recipientAccount.Id) < 0 ? sourceAccount.Id : recipientAccount.Id;
            var secondAccountId = firstAccountId == sourceAccount.Id ? recipientAccount.Id : sourceAccount.Id;

            var lockedFirst = await _context.GetAccountForUpdateAsync(firstAccountId, cancellationToken);
            var lockedSecond = await _context.GetAccountForUpdateAsync(secondAccountId, cancellationToken);

            var activeSource = sourceAccount.Id == firstAccountId ? lockedFirst : lockedSecond;
            var activeRecipient = recipientAccount.Id == firstAccountId ? lockedFirst : lockedSecond;

            if (activeSource == null || activeRecipient == null)
            {
                throw new DomainException("Failed to acquire transaction locks on participating accounts.");
            }

            // 5. Re-check funds and state inside locked transaction
            activeSource.EnsureActive();
            activeRecipient.EnsureActive();

            // 6. Execute atomic balance updates
            activeSource.Debit(request.Amount);
            activeRecipient.Credit(request.Amount);

            // 7. Generate Reference
            var reference = $"VP-TRF-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            var txn = new Transaction
            {
                Reference = reference,
                FromAccountId = activeSource.Id,
                ToAccountId = activeRecipient.Id,
                Amount = request.Amount,
                CurrencyCode = activeSource.CurrencyCode,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                Description = string.IsNullOrWhiteSpace(request.Description) ? "Transfer" : request.Description.Trim(),
                InitiatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(txn);

            // 8. Add Double-Entry Ledger Records (Immutable)
            var debitEntry = new LedgerEntry
            {
                Transaction = txn,
                AccountId = activeSource.Id,
                EntryType = LedgerEntryType.Debit,
                Amount = request.Amount,
                CurrencyCode = activeSource.CurrencyCode,
                CreatedAt = DateTime.UtcNow
            };

            var creditEntry = new LedgerEntry
            {
                Transaction = txn,
                AccountId = activeRecipient.Id,
                EntryType = LedgerEntryType.Credit,
                Amount = request.Amount,
                CurrencyCode = activeRecipient.CurrencyCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.LedgerEntries.Add(debitEntry);
            _context.LedgerEntries.Add(creditEntry);

            await _context.SaveChangesAsync(cancellationToken);

            // 9. Prepare and persist response
            var responseDto = new TransferResponseDto
            {
                TransactionId = txn.Id,
                Reference = txn.Reference,
                Amount = txn.Amount,
                CurrencyCode = txn.CurrencyCode,
                Status = txn.Status,
                SourceAccountNumber = activeSource.AccountNumber,
                SourceAccountName = activeSource.AccountName,
                RecipientAccountNumber = activeRecipient.AccountNumber,
                RecipientName = recipientAccount.User?.FullName ?? recipientAccount.AccountName,
                RemainingBalance = activeSource.Balance,
                Description = txn.Description,
                CreatedAt = txn.CreatedAt,
                CompletedAt = txn.CompletedAt
            };

            var serializedResponse = JsonSerializer.Serialize(responseDto);
            await _idempotencyService.CompleteRecordAsync(idempotencyRecord.Id, serializedResponse, 200, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            await _auditService.LogAsync(
                userId,
                "TRANSFER_COMPLETED",
                "Transaction",
                txn.Id.ToString(),
                $"Transferred {request.Amount:N2} {txn.CurrencyCode} from {activeSource.AccountNumber} to {activeRecipient.AccountNumber}",
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

    public async Task<TransferResponseDto> GetTransferByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var txn = await _context.Transactions
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
                .ThenInclude(a => a!.User)
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.Type == TransactionType.Transfer, cancellationToken);

        if (txn == null)
        {
            throw new DomainException($"Transfer transaction '{transactionId}' was not found.");
        }

        if (txn.InitiatedByUserId != userId &&
            txn.FromAccount?.UserId != userId &&
            txn.ToAccount?.UserId != userId &&
            !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Unauthorized to access this transfer record.");
        }

        return new TransferResponseDto
        {
            TransactionId = txn.Id,
            Reference = txn.Reference,
            Amount = txn.Amount,
            CurrencyCode = txn.CurrencyCode,
            Status = txn.Status,
            SourceAccountNumber = txn.FromAccount?.AccountNumber ?? "N/A",
            SourceAccountName = txn.FromAccount?.AccountName ?? "N/A",
            RecipientAccountNumber = txn.ToAccount?.AccountNumber ?? "N/A",
            RecipientName = txn.ToAccount?.User?.FullName ?? txn.ToAccount?.AccountName ?? "N/A",
            RemainingBalance = txn.FromAccount?.Balance ?? 0m,
            Description = txn.Description,
            CreatedAt = txn.CreatedAt,
            CompletedAt = txn.CompletedAt
        };
    }
}
