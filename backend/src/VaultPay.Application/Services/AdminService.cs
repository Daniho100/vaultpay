using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.Common.Models;
using VaultPay.Application.DTOs.Admin;
using VaultPay.Application.DTOs.Loans;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;
using VaultPay.Domain.Exceptions;

namespace VaultPay.Application.Services;

public class AdminService : IAdminService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AdminService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    private void EnsureAdmin()
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Administrator privileges are required for this action.");
        }
    }

    public async Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var totalCustomers = await _context.Users.CountAsync(cancellationToken);
        var totalAccounts = await _context.Accounts.CountAsync(cancellationToken);
        var totalTransactions = await _context.Transactions.CountAsync(cancellationToken);

        var depositAmounts = await _context.Transactions
            .Where(t => t.Type == TransactionType.Deposit && t.Status == TransactionStatus.Completed)
            .Select(t => t.Amount)
            .ToListAsync(cancellationToken);
        var totalDeposits = depositAmounts.Sum();

        var transferAmounts = await _context.Transactions
            .Where(t => t.Type == TransactionType.Transfer && t.Status == TransactionStatus.Completed)
            .Select(t => t.Amount)
            .ToListAsync(cancellationToken);
        var totalTransfers = transferAmounts.Sum();

        var balances = await _context.Accounts
            .Select(a => a.Balance)
            .ToListAsync(cancellationToken);
        var totalHoldings = balances.Sum();

        return new AdminStatsDto
        {
            TotalCustomers = totalCustomers,
            TotalAccounts = totalAccounts,
            TotalTransactions = totalTransactions,
            TotalDepositsVolume = totalDeposits,
            TotalTransfersVolume = totalTransfers,
            TotalSystemHoldings = totalHoldings,
            SystemStatus = "Operational"
        };
    }

    public async Task<PaginatedResult<AdminUserDto>> GetUsersAsync(int pageNumber, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.Accounts)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u => u.Email!.ToLower().Contains(term) || u.FullName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = users.Select(u => new AdminUserDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            FullName = u.FullName,
            PhoneNumber = u.PhoneNumber,
            IsActive = u.IsActive,
            Tier = u.Tier,
            CreatedAt = u.CreatedAt,
            AccountsCount = u.Accounts.Count,
            TotalBalance = u.Accounts.Sum(a => a.Balance),
            Roles = new List<string>() // Can be enriched from user manager if needed
        }).ToList();

        return new PaginatedResult<AdminUserDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PaginatedResult<AdminAccountDto>> GetAccountsAsync(int pageNumber, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var query = _context.Accounts
            .AsNoTracking()
            .Include(a => a.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a =>
                a.AccountNumber.Contains(term) ||
                a.AccountName.ToLower().Contains(term) ||
                (a.User != null && (a.User.Email!.ToLower().Contains(term) || a.User.FullName.ToLower().Contains(term))));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var accounts = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = accounts.Select(a => new AdminAccountDto
        {
            Id = a.Id,
            AccountNumber = a.AccountNumber,
            AccountName = a.AccountName,
            UserId = a.UserId,
            OwnerEmail = a.User?.Email ?? string.Empty,
            OwnerFullName = a.User?.FullName ?? string.Empty,
            CurrencyCode = a.CurrencyCode,
            Balance = a.Balance,
            Status = a.Status,
            CreatedAt = a.CreatedAt
        }).ToList();

        return new PaginatedResult<AdminAccountDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PaginatedResult<AdminTransactionDto>> GetTransactionsAsync(int pageNumber, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Include(t => t.InitiatedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t =>
                t.Reference.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term) ||
                (t.FromAccount != null && t.FromAccount.AccountNumber.Contains(term)) ||
                (t.ToAccount != null && t.ToAccount.AccountNumber.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var txns = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = txns.Select(t => new AdminTransactionDto
        {
            Id = t.Id,
            Reference = t.Reference,
            Type = t.Type,
            Amount = t.Amount,
            CurrencyCode = t.CurrencyCode,
            Status = t.Status,
            FromAccountNumber = t.FromAccount?.AccountNumber,
            ToAccountNumber = t.ToAccount?.AccountNumber,
            InitiatedByEmail = t.InitiatedByUser?.Email ?? "system",
            Description = t.Description,
            CreatedAt = t.CreatedAt
        }).ToList();

        return new PaginatedResult<AdminTransactionDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<AdminTransactionDto> GetTransactionDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var t = await _context.Transactions
            .AsNoTracking()
            .Include(x => x.FromAccount)
            .Include(x => x.ToAccount)
            .Include(x => x.InitiatedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (t == null)
        {
            throw new DomainException($"Transaction '{id}' was not found.");
        }

        return new AdminTransactionDto
        {
            Id = t.Id,
            Reference = t.Reference,
            Type = t.Type,
            Amount = t.Amount,
            CurrencyCode = t.CurrencyCode,
            Status = t.Status,
            FromAccountNumber = t.FromAccount?.AccountNumber,
            ToAccountNumber = t.ToAccount?.AccountNumber,
            InitiatedByEmail = t.InitiatedByUser?.Email ?? "system",
            Description = t.Description,
            CreatedAt = t.CreatedAt
        };
    }

    public async Task<AdminUserDetailsDto> SearchUserWithTransactionsAsync(string query, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        if (string.IsNullOrWhiteSpace(query))
        {
            throw new DomainException("Search query cannot be empty. Please enter user's email or account number.");
        }

        var term = query.Trim().ToLower();

        // 1. Try finding by email or fullName
        var user = await _context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Email!.ToLower() == term || u.FullName.ToLower().Contains(term), cancellationToken);

        // 2. If not found, try finding via AccountNumber
        if (user == null)
        {
            var acc = await _context.Accounts
                .Include(a => a.User)
                .ThenInclude(u => u.Accounts)
                .FirstOrDefaultAsync(a => a.AccountNumber == query.Trim(), cancellationToken);

            user = acc?.User;
        }

        if (user == null)
        {
            throw new DomainException($"No user found matching email or account number '{query}'.");
        }

        var userAccountIds = user.Accounts.Select(a => a.Id).ToList();

        // Fetch all transactions involving this user (either as sender, recipient, or initiator)
        var transactions = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Include(t => t.InitiatedByUser)
            .Where(t => 
                (t.FromAccountId.HasValue && userAccountIds.Contains(t.FromAccountId.Value)) ||
                (t.ToAccountId.HasValue && userAccountIds.Contains(t.ToAccountId.Value)) ||
                t.InitiatedByUserId == user.Id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return new AdminUserDetailsDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Tier = user.Tier,
            CreatedAt = user.CreatedAt,
            Accounts = user.Accounts.Select(a => new AdminAccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                AccountName = a.AccountName,
                UserId = a.UserId,
                OwnerEmail = user.Email ?? string.Empty,
                OwnerFullName = user.FullName,
                CurrencyCode = a.CurrencyCode,
                Balance = a.Balance,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            }).ToList(),
            Transactions = transactions.Select(t => new AdminTransactionDto
            {
                Id = t.Id,
                Reference = t.Reference,
                Type = t.Type,
                Amount = t.Amount,
                CurrencyCode = t.CurrencyCode,
                Status = t.Status,
                FromAccountNumber = t.FromAccount?.AccountNumber,
                ToAccountNumber = t.ToAccount?.AccountNumber,
                InitiatedByEmail = t.InitiatedByUser?.Email ?? "system",
                Description = t.Description,
                CreatedAt = t.CreatedAt
            }).ToList()
        };
    }

    public async Task<AdminUserDto> BanUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var user = await _context.Users.Include(u => u.Accounts).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) throw new DomainException("User not found.");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        // Suspend user accounts
        foreach (var acc in user.Accounts)
        {
            acc.Status = AccountStatus.Suspended;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Tier = user.Tier,
            CreatedAt = user.CreatedAt,
            AccountsCount = user.Accounts.Count,
            TotalBalance = user.Accounts.Sum(a => a.Balance)
        };
    }

    public async Task<AdminUserDto> UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var user = await _context.Users.Include(u => u.Accounts).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) throw new DomainException("User not found.");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        // Reactivate user accounts
        foreach (var acc in user.Accounts)
        {
            acc.Status = AccountStatus.Active;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Tier = user.Tier,
            CreatedAt = user.CreatedAt,
            AccountsCount = user.Accounts.Count,
            TotalBalance = user.Accounts.Sum(a => a.Balance)
        };
    }

    public async Task<AdminUserDto> EditUserAsync(Guid userId, EditUserRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var user = await _context.Users.Include(u => u.Accounts).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) throw new DomainException("User not found.");

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        if (request.PhoneNumber != null)
        {
            user.PhoneNumber = request.PhoneNumber.Trim();
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Tier = user.Tier,
            CreatedAt = user.CreatedAt,
            AccountsCount = user.Accounts.Count,
            TotalBalance = user.Accounts.Sum(a => a.Balance)
        };
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var user = await _context.Users.Include(u => u.Accounts).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) throw new DomainException("User not found.");

        user.IsActive = false;
        foreach (var acc in user.Accounts)
        {
            acc.Status = AccountStatus.Closed;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AdminUserDto> UpgradeUserTierAsync(Guid userId, UpgradeTierRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var user = await _context.Users.Include(u => u.Accounts).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) throw new DomainException("User not found.");

        user.Tier = request.Tier;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Tier = user.Tier,
            CreatedAt = user.CreatedAt,
            AccountsCount = user.Accounts.Count,
            TotalBalance = user.Accounts.Sum(a => a.Balance)
        };
    }

    public async Task<List<LoanDto>> GetAllLoansAsync(CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var loans = await _context.Loans
            .AsNoTracking()
            .Include(l => l.User)
            .Include(l => l.Account)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        return loans.Select(l => new LoanDto
        {
            Id = l.Id,
            UserId = l.UserId,
            UserFullName = l.User?.FullName ?? string.Empty,
            UserEmail = l.User?.Email ?? string.Empty,
            AccountId = l.AccountId,
            AccountNumber = l.Account?.AccountNumber ?? string.Empty,
            Amount = l.Amount,
            Purpose = l.Purpose,
            DurationMonths = l.DurationMonths,
            InterestRate = l.InterestRate,
            Status = l.Status,
            AdminNotes = l.AdminNotes,
            CreatedAt = l.CreatedAt,
            ApprovedAt = l.ApprovedAt,
            RepaidAt = l.RepaidAt
        }).ToList();
    }

    public async Task<LoanDto> ApproveLoanAsync(Guid loanId, ApproveLoanRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var loan = await _context.Loans
            .Include(l => l.User)
            .Include(l => l.Account)
            .FirstOrDefaultAsync(l => l.Id == loanId, cancellationToken);

        if (loan == null) throw new DomainException("Loan application not found.");
        if (loan.Status != LoanStatus.Pending) throw new DomainException($"Cannot approve loan with status '{loan.Status}'.");

        var account = loan.Account ?? await _context.Accounts.FirstOrDefaultAsync(a => a.Id == loan.AccountId, cancellationToken);
        if (account == null) throw new DomainException("Disbursement account not found.");

        // Disburse loan funds into user account
        account.Credit(loan.Amount);

        loan.Status = LoanStatus.Approved;
        loan.ApprovedAt = DateTime.UtcNow;
        loan.AdminNotes = request?.AdminNotes ?? "Approved by administrator.";

        var refCode = $"VP-LOAN-DISB-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Reference = refCode,
            ToAccountId = account.Id,
            Amount = loan.Amount,
            CurrencyCode = account.CurrencyCode,
            Type = TransactionType.LoanDisbursement,
            Status = TransactionStatus.Completed,
            Description = $"Disbursement for approved loan ({loan.Purpose})",
            InitiatedByUserId = loan.UserId,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        var ledgerEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            AccountId = account.Id,
            EntryType = LedgerEntryType.Credit,
            Amount = loan.Amount,
            CurrencyCode = account.CurrencyCode,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        _context.LedgerEntries.Add(ledgerEntry);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoanDto
        {
            Id = loan.Id,
            UserId = loan.UserId,
            UserFullName = loan.User?.FullName ?? string.Empty,
            UserEmail = loan.User?.Email ?? string.Empty,
            AccountId = loan.AccountId,
            AccountNumber = account.AccountNumber,
            Amount = loan.Amount,
            Purpose = loan.Purpose,
            DurationMonths = loan.DurationMonths,
            InterestRate = loan.InterestRate,
            Status = loan.Status,
            AdminNotes = loan.AdminNotes,
            CreatedAt = loan.CreatedAt,
            ApprovedAt = loan.ApprovedAt
        };
    }

    public async Task<LoanDto> RejectLoanAsync(Guid loanId, ApproveLoanRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        var loan = await _context.Loans
            .Include(l => l.User)
            .Include(l => l.Account)
            .FirstOrDefaultAsync(l => l.Id == loanId, cancellationToken);

        if (loan == null) throw new DomainException("Loan application not found.");
        if (loan.Status != LoanStatus.Pending) throw new DomainException($"Cannot reject loan with status '{loan.Status}'.");

        loan.Status = LoanStatus.Rejected;
        loan.AdminNotes = request?.AdminNotes ?? "Rejected by administrator.";
        await _context.SaveChangesAsync(cancellationToken);

        return new LoanDto
        {
            Id = loan.Id,
            UserId = loan.UserId,
            UserFullName = loan.User?.FullName ?? string.Empty,
            UserEmail = loan.User?.Email ?? string.Empty,
            AccountId = loan.AccountId,
            AccountNumber = loan.Account?.AccountNumber ?? string.Empty,
            Amount = loan.Amount,
            Purpose = loan.Purpose,
            DurationMonths = loan.DurationMonths,
            InterestRate = loan.InterestRate,
            Status = loan.Status,
            AdminNotes = loan.AdminNotes,
            CreatedAt = loan.CreatedAt
        };
    }
}
