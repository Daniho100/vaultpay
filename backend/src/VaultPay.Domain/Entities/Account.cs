using VaultPay.Domain.Enums;
using VaultPay.Domain.Exceptions;

namespace VaultPay.Domain.Entities;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "NGN";
    public decimal Balance { get; set; } = 0.00m;
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string ConcurrencyToken { get; set; } = Guid.NewGuid().ToString("N");

    public ICollection<Transaction> OutgoingTransactions { get; set; } = new List<Transaction>();
    public ICollection<Transaction> IncomingTransactions { get; set; } = new List<Transaction>();
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();

    public void EnsureActive()
    {
        if (Status != AccountStatus.Active)
        {
            throw new AccountInactiveException(AccountNumber, Status.ToString());
        }
    }

    public void Debit(decimal amount)
    {
        EnsureActive();

        if (amount <= 0)
        {
            throw new InvalidAmountException("Debit amount must be strictly positive.");
        }

        if (Balance < amount)
        {
            throw new InsufficientFundsException(Balance, amount);
        }

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;
        ConcurrencyToken = Guid.NewGuid().ToString("N");
    }

    public void Credit(decimal amount)
    {
        EnsureActive();

        if (amount <= 0)
        {
            throw new InvalidAmountException("Credit amount must be strictly positive.");
        }

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;
        ConcurrencyToken = Guid.NewGuid().ToString("N");
    }
}
