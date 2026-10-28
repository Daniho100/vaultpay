using VaultPay.Domain.Enums;

namespace VaultPay.Application.DTOs.Admin;

public class AdminStatsDto
{
    public int TotalCustomers { get; set; }
    public int TotalAccounts { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalDepositsVolume { get; set; }
    public decimal TotalTransfersVolume { get; set; }
    public decimal TotalSystemHoldings { get; set; }
    public string SystemStatus { get; set; } = "Operational";
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public UserTier Tier { get; set; } = UserTier.Tier1;
    public DateTime CreatedAt { get; set; }
    public int AccountsCount { get; set; }
    public decimal TotalBalance { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

public class AdminUserDetailsDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public UserTier Tier { get; set; } = UserTier.Tier1;
    public DateTime CreatedAt { get; set; }
    public IList<AdminAccountDto> Accounts { get; set; } = new List<AdminAccountDto>();
    public IList<AdminTransactionDto> Transactions { get; set; } = new List<AdminTransactionDto>();
}

public class EditUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class UpgradeTierRequest
{
    public UserTier Tier { get; set; }
}

public class AdminAccountDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "NGN";
    public decimal Balance { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminTransactionDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "NGN";
    public TransactionStatus Status { get; set; }
    public string? FromAccountNumber { get; set; }
    public string? ToAccountNumber { get; set; }
    public string InitiatedByEmail { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

