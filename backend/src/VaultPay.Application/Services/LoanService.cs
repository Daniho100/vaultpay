using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.DTOs.Loans;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;
using VaultPay.Domain.Exceptions;

namespace VaultPay.Application.Services;

public class LoanService : ILoanService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public LoanService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<LoanDto>> GetUserLoansAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var loans = await _context.Loans
            .AsNoTracking()
            .Include(l => l.Account)
            .Include(l => l.User)
            .Where(l => l.UserId == userId)
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

    public async Task<LoanDto> ApplyLoanAsync(ApplyLoanRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        if (request.Amount <= 0)
        {
            throw new InvalidAmountException("Loan amount must be greater than zero.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new DomainException("User profile not found.");
        }

        // Tier limits
        decimal maxAllowed = user.Tier switch
        {
            UserTier.Tier1 => 100_000m,
            UserTier.Tier2 => 500_000m,
            UserTier.Tier3 => 5_000_000m,
            _ => 100_000m
        };

        if (request.Amount > maxAllowed)
        {
            throw new DomainException($"Loan amount of ₦{request.Amount:N2} exceeds your current {user.Tier} borrowing limit of ₦{maxAllowed:N2}. Please apply for an upgrade to Tier {(int)user.Tier + 1}.");
        }

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);
        if (account == null)
        {
            throw new DomainException("Disbursement account not found.");
        }

        if (account.UserId != userId && !_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Account does not belong to the authenticated user.");
        }

        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = account.Id,
            Amount = request.Amount,
            Purpose = string.IsNullOrWhiteSpace(request.Purpose) ? "Personal / Business Capital" : request.Purpose,
            DurationMonths = request.DurationMonths <= 0 ? 3 : request.DurationMonths,
            InterestRate = 5.0m, // 5% flat rate
            Status = LoanStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoanDto
        {
            Id = loan.Id,
            UserId = loan.UserId,
            UserFullName = user.FullName,
            UserEmail = user.Email ?? string.Empty,
            AccountId = loan.AccountId,
            AccountNumber = account.AccountNumber,
            Amount = loan.Amount,
            Purpose = loan.Purpose,
            DurationMonths = loan.DurationMonths,
            InterestRate = loan.InterestRate,
            Status = loan.Status,
            CreatedAt = loan.CreatedAt
        };
    }
}
