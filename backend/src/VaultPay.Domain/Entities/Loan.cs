using VaultPay.Domain.Enums;

namespace VaultPay.Domain.Entities;

public class Loan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public int DurationMonths { get; set; } = 3;
    public decimal InterestRate { get; set; } = 5.0m;
    public LoanStatus Status { get; set; } = LoanStatus.Pending;
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RepaidAt { get; set; }
}
