using VaultPay.Domain.Enums;

namespace VaultPay.Application.DTOs.Accounts;

public class AccountDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "NGN";
    public decimal Balance { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAccountRequest
{
    public string AccountName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "NGN";
}

public class AccountBalanceDto
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string CurrencyCode { get; set; } = "NGN";
    public AccountStatus Status { get; set; }
}

public class RecipientLookupDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "NGN";
    public bool IsActive { get; set; }
}
