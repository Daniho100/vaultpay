using VaultPay.Domain.Enums;

namespace VaultPay.Domain.Entities;

public class FixedDeposit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal InterestRatePerAnnum { get; set; } = 0.25m; // 25% per annum
    public int DurationDays { get; set; } = 90;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime MaturityDate { get; set; } = DateTime.UtcNow.AddDays(90);
    public FixedDepositStatus Status { get; set; } = FixedDepositStatus.Active;
    public decimal PenaltyRate { get; set; } = 0.10m; // 10% early withdrawal penalty
    public decimal ProjectedInterest { get; set; }
    public decimal? ActualPayout { get; set; }
    public DateTime? ClosedAt { get; set; }

    public static decimal CalculateProjectedInterest(decimal principal, int days, decimal rate = 0.25m)
    {
        return Math.Round(principal * rate * (days / 365.0m), 2);
    }
}
