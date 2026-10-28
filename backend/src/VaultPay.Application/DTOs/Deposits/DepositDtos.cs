using VaultPay.Domain.Enums;

namespace VaultPay.Application.DTOs.Deposits;

public class DepositRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "Simulated Demo Deposit";
}

public class DepositResponseDto
{
    public Guid TransactionId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "NGN";
    public TransactionStatus Status { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal BalanceAfterDeposit { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsDemoFunding { get; set; } = true;
}
