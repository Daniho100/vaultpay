using VaultPay.Domain.Enums;

namespace VaultPay.Application.DTOs.Savings;

public class CreateFixedDepositRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public int DurationDays { get; set; } = 90; // e.g. 30, 60, 90, 180, 365
}

public class FixedDepositDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal InterestRatePerAnnum { get; set; } = 0.25m; // 25% per annum
    public int DurationDays { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime MaturityDate { get; set; }
    public FixedDepositStatus Status { get; set; }
    public decimal PenaltyRate { get; set; } = 0.10m; // 10% early withdrawal penalty
    public decimal ProjectedInterest { get; set; }
    public decimal? ActualPayout { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool IsEligibleForFullMaturity => DateTime.UtcNow >= MaturityDate;
    public decimal EarlyWithdrawalPenaltyAmount => Math.Round(Amount * PenaltyRate, 2);
    public decimal EarlyWithdrawalRefundAmount => Math.Round(Amount - (Amount * PenaltyRate), 2);
}

public class LiquidateDepositResponse
{
    public Guid Id { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal PenaltyDeducted { get; set; }
    public decimal NetPayoutRefunded { get; set; }
    public decimal AccountBalanceAfter { get; set; }
    public bool WasEarlyWithdrawal { get; set; }
    public string Message { get; set; } = string.Empty;
}
