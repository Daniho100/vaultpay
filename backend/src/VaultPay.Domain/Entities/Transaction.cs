using VaultPay.Domain.Enums;

namespace VaultPay.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Reference { get; set; } = string.Empty;
    public Guid? FromAccountId { get; set; }
    public Account? FromAccount { get; set; }
    public Guid? ToAccountId { get; set; }
    public Account? ToAccount { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "NGN";
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public string Description { get; set; } = string.Empty;
    public Guid InitiatedByUserId { get; set; }
    public User InitiatedByUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }

    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();

    public void MarkCompleted()
    {
        Status = TransactionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = TransactionStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        FailureReason = reason;
    }
}
