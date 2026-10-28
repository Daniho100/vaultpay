using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.Common.Models;
using VaultPay.Application.DTOs.Transactions;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;
using VaultPay.Domain.Exceptions;

namespace VaultPay.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public TransactionService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(TransactionFilterParams filter, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var userAccountIds = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .AsQueryable();

        if (!_currentUserService.IsAdmin)
        {
            query = query.Where(t =>
                (t.FromAccountId.HasValue && userAccountIds.Contains(t.FromAccountId.Value)) ||
                (t.ToAccountId.HasValue && userAccountIds.Contains(t.ToAccountId.Value)));
        }

        if (filter.AccountId.HasValue)
        {
            query = query.Where(t => t.FromAccountId == filter.AccountId || t.ToAccountId == filter.AccountId);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(t => t.Type == filter.Type.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.FromDate.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Utc);
            query = query.Where(t => t.CreatedAt >= fromUtc);
        }

        if (filter.ToDate.HasValue)
        {
            var toUtc = DateTime.SpecifyKind(filter.ToDate.Value, DateTimeKind.Utc);
            query = query.Where(t => t.CreatedAt <= toUtc);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(t =>
                t.Reference.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term) ||
                (t.FromAccount != null && t.FromAccount.AccountNumber.Contains(term)) ||
                (t.ToAccount != null && t.ToAccount.AccountNumber.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var rawItems = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rawItems.Select(t => new TransactionDto
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
            IsDebit = t.FromAccountId.HasValue && userAccountIds.Contains(t.FromAccountId.Value)
        }).ToList();

        return new PaginatedResult<TransactionDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<TransactionDto> GetTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var t = await _context.Transactions
            .AsNoTracking()
            .Include(x => x.FromAccount)
            .Include(x => x.ToAccount)
            .FirstOrDefaultAsync(x => x.Id == transactionId, cancellationToken);

        if (t == null)
        {
            throw new DomainException($"Transaction '{transactionId}' was not found.");
        }

        var isSender = t.FromAccount?.UserId == userId;
        var isReceiver = t.ToAccount?.UserId == userId;

        if (!isSender && !isReceiver && !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Unauthorized to access this transaction.");
        }

        return new TransactionDto
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
            IsDebit = isSender
        };
    }

    public async Task<TransactionReceiptDto> GetTransactionReceiptAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var t = await _context.Transactions
            .AsNoTracking()
            .Include(x => x.FromAccount).ThenInclude(a => a!.User)
            .Include(x => x.ToAccount).ThenInclude(a => a!.User)
            .FirstOrDefaultAsync(x => x.Id == transactionId, cancellationToken);

        if (t == null)
        {
            throw new DomainException($"Transaction '{transactionId}' was not found.");
        }

        var isSender = t.FromAccount?.UserId == userId;
        var isReceiver = t.ToAccount?.UserId == userId;

        if (!isSender && !isReceiver && !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Unauthorized to access this receipt.");
        }

        return new TransactionReceiptDto
        {
            Reference = t.Reference,
            Date = t.CreatedAt,
            Type = t.Type,
            Status = t.Status,
            Amount = t.Amount,
            CurrencyCode = t.CurrencyCode,
            SourceAccountMasked = MaskAccountNumber(t.FromAccount?.AccountNumber),
            SourceAccountName = t.FromAccount?.AccountName,
            RecipientAccountMasked = MaskAccountNumber(t.ToAccount?.AccountNumber),
            RecipientAccountName = t.ToAccount?.AccountName,
            Description = t.Description
        };
    }

    private static string? MaskAccountNumber(string? accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber)) return null;
        if (accountNumber.Length <= 4) return accountNumber;
        return $"{new string('*', accountNumber.Length - 4)}{accountNumber[^4..]}";
    }
}
