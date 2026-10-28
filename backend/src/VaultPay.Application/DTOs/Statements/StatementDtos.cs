using VaultPay.Application.DTOs.Accounts;

namespace VaultPay.Application.DTOs.Statements;

public class StatementRequest
{
    public Guid AccountId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class StatementEntryDto
{
    public DateTime Date { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EntryType { get; set; } = string.Empty; // Debit or Credit
    public decimal Amount { get; set; }
    public decimal RunningBalance { get; set; }
}

public class StatementDto
{
    public AccountDto Account { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public List<StatementEntryDto> Entries { get; set; } = new();
}
