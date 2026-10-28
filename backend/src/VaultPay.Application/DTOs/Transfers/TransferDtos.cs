using VaultPay.Domain.Enums;

namespace VaultPay.Application.DTOs.Transfers;

public class TransferRequest
{
    public Guid FromAccountId { get; set; }
    public string RecipientAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TransferResponseDto
{
    public Guid TransactionId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "NGN";
    public TransactionStatus Status { get; set; }
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string SourceAccountName { get; set; } = string.Empty;
    public string RecipientAccountNumber { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public decimal RemainingBalance { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
