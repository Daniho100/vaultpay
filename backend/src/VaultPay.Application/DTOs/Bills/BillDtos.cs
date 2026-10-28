using VaultPay.Domain.Enums;

namespace VaultPay.Application.DTOs.Bills;

public class PayBillRequest
{
    public Guid AccountId { get; set; }
    public BillType Type { get; set; }
    public string Provider { get; set; } = string.Empty; // e.g. MTN, Airtel, IKEDC, Bet9ja
    public string CustomerIdentifier { get; set; } = string.Empty; // Phone number, Meter number, or Betting ID
    public decimal Amount { get; set; }
    public string? PackageName { get; set; } // e.g. 5GB Monthly, Prepaid Token
}

public class PayBillResponse
{
    public Guid TransactionId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string CustomerIdentifier { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
    public string? TokenOrReceiptCode { get; set; } // e.g. 20-digit electricity token or bet funding ref
    public decimal BalanceAfter { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
