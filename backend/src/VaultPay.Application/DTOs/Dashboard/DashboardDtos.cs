using VaultPay.Application.DTOs.Accounts;
using VaultPay.Application.DTOs.Transactions;

namespace VaultPay.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public decimal TotalBalance { get; set; }
    public string PrimaryCurrency { get; set; } = "NGN";
    public int ActiveAccountsCount { get; set; }
    public decimal TotalDepositsAmount { get; set; }
    public decimal TotalTransfersAmount { get; set; }
    public List<AccountDto> Accounts { get; set; } = new();
    public List<TransactionDto> RecentTransactions { get; set; } = new();
}

public class ActivityPointDto
{
    public string Date { get; set; } = string.Empty;
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
}

public class DashboardAnalyticsDto
{
    public decimal MonthlyInflow { get; set; }
    public decimal MonthlyOutflow { get; set; }
    public decimal NetCashFlow => MonthlyInflow - MonthlyOutflow;
    public List<ActivityPointDto> ActivityPoints { get; set; } = new();
}
