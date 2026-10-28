using Microsoft.AspNetCore.Identity;

using VaultPay.Domain.Enums;

namespace VaultPay.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public UserTier Tier { get; set; } = UserTier.Tier1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Transaction> InitiatedTransactions { get; set; } = new List<Transaction>();
    public ICollection<IdempotencyRecord> IdempotencyRecords { get; set; } = new List<IdempotencyRecord>();
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public ICollection<FixedDeposit> FixedDeposits { get; set; } = new List<FixedDeposit>();
}

public class Role : IdentityRole<Guid>
{
    public Role() : base() { }
    public Role(string roleName) : base(roleName) { }
}
