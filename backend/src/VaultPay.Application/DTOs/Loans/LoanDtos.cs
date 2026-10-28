using VaultPay.Domain.Enums;

namespace VaultPay.Application.DTOs.Loans;

public class ApplyLoanRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public int DurationMonths { get; set; } = 3;
}

public class LoanDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public decimal InterestRate { get; set; }
    public decimal TotalRepayment => Math.Round(Amount + (Amount * (InterestRate / 100m)), 2);
    public LoanStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RepaidAt { get; set; }
}

public class ApproveLoanRequest
{
    public string? AdminNotes { get; set; }
}
