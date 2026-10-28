using VaultPay.Domain.Enums;

namespace VaultPay.Application.DTOs.Transactions;

public class TransactionDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public Guid? FromAccountId { get; set; }
    public string? FromAccountNumber { get; set; }
    public string? FromAccountName { get; set; }
    public Guid? ToAccountId { get; set; }
    public string? ToAccountNumber { get; set; }
    public string? ToAccountName { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "NGN";
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsDebit { get; set; }
}

public class TransactionReceiptDto
{
    public string Reference { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "NGN";
    public string? SourceAccountMasked { get; set; }
    public string? SourceAccountName { get; set; }
    public string? RecipientAccountMasked { get; set; }
    public string? RecipientAccountName { get; set; }
    public string Description { get; set; } = string.Empty;
    public string SimulatedDisclaimer { get; set; } = "VaultPay Simulated Banking Record - No Real Financial Value";
}

public class TransactionFilterParams
{
    public Guid? AccountId { get; set; }
    public TransactionType? Type { get; set; }
    public TransactionStatus? Status { get; set; }
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}
