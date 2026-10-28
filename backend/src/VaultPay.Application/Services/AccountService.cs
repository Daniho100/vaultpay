using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.DTOs.Accounts;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;
using VaultPay.Domain.Exceptions;

namespace VaultPay.Application.Services;

public class AccountService : IAccountService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public AccountService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<List<AccountDto>> GetUserAccountsAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                AccountName = a.AccountName,
                CurrencyCode = a.CurrencyCode,
                Balance = a.Balance,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AccountDto> GetAccountByIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new AccountNotFoundException(accountId.ToString());
        }

        if (account.UserId != userId && !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Unauthorized to access this account.");
        }

        return new AccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            CurrencyCode = account.CurrencyCode,
            Balance = account.Balance,
            Status = account.Status,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<AccountBalanceDto> GetAccountBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new AccountNotFoundException(accountId.ToString());
        }

        if (account.UserId != userId && !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Unauthorized to access this account balance.");
        }

        return new AccountBalanceDto
        {
            AccountId = account.Id,
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            CurrencyCode = account.CurrencyCode,
            Status = account.Status
        };
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User record not found.");
        }

        var accountNumber = await GenerateUniqueAccountNumberAsync(cancellationToken);
        var accountName = string.IsNullOrWhiteSpace(request.AccountName) ? $"{user.FullName}'s Wallet" : request.AccountName.Trim();
        var currency = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "NGN" : request.CurrencyCode.ToUpperInvariant().Trim();

        var account = new Account
        {
            UserId = userId,
            AccountNumber = accountNumber,
            AccountName = accountName,
            CurrencyCode = currency,
            Balance = 0.00m,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId,
            "ACCOUNT_CREATED",
            "Account",
            account.Id.ToString(),
            $"Created account {account.AccountNumber} ({account.AccountName})",
            cancellationToken: cancellationToken);

        return new AccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            CurrencyCode = account.CurrencyCode,
            Balance = account.Balance,
            Status = account.Status,
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<RecipientLookupDto> LookupRecipientAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new DomainException("Account number must be specified.");
        }

        var account = await _context.Accounts
            .Include(a => a.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber.Trim(), cancellationToken);

        if (account == null)
        {
            throw new AccountNotFoundException($"Account number '{accountNumber}' does not exist.");
        }

        return new RecipientLookupDto
        {
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            OwnerName = account.User?.FullName ?? account.AccountName,
            CurrencyCode = account.CurrencyCode,
            IsActive = account.Status == AccountStatus.Active
        };
    }

    private async Task<string> GenerateUniqueAccountNumberAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i < 10; i++)
        {
            // Standard Nigerian NUBAN style 10 digits starting with 10...
            var randomDigits = RandomNumberGenerator.GetInt32(10000000, 99999999).ToString();
            var candidate = $"10{randomDigits}";

            var exists = await _context.Accounts.AnyAsync(a => a.AccountNumber == candidate, cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        return $"10{DateTime.UtcNow.Ticks.ToString()[^8..]}";
    }
}
