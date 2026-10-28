using VaultPay.Application.Common.Models;
using VaultPay.Application.DTOs.Accounts;
using VaultPay.Application.DTOs.Admin;
using VaultPay.Application.DTOs.Auth;
using VaultPay.Application.DTOs.Dashboard;
using VaultPay.Application.DTOs.Deposits;
using VaultPay.Application.DTOs.Statements;
using VaultPay.Application.DTOs.Transactions;
using VaultPay.Application.DTOs.Transfers;

using VaultPay.Application.DTOs.Bills;
using VaultPay.Application.DTOs.Loans;
using VaultPay.Application.DTOs.Savings;

namespace VaultPay.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileDto> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
}

public interface IAccountService
{
    Task<List<AccountDto>> GetUserAccountsAsync(CancellationToken cancellationToken = default);
    Task<AccountDto> GetAccountByIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<AccountBalanceDto> GetAccountBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<AccountDto> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);
    Task<RecipientLookupDto> LookupRecipientAsync(string accountNumber, CancellationToken cancellationToken = default);
}

public interface ITransferService
{
    Task<TransferResponseDto> ProcessTransferAsync(TransferRequest request, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<TransferResponseDto> GetTransferByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
}

public interface IDepositService
{
    Task<DepositResponseDto> ProcessDepositAsync(DepositRequest request, string idempotencyKey, CancellationToken cancellationToken = default);
}

public interface ITransactionService
{
    Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(TransactionFilterParams filter, CancellationToken cancellationToken = default);
    Task<TransactionDto> GetTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<TransactionReceiptDto> GetTransactionReceiptAsync(Guid transactionId, CancellationToken cancellationToken = default);
}

public interface IStatementService
{
    Task<StatementDto> GenerateStatementAsync(StatementRequest request, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateCsvStatementAsync(StatementRequest request, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<DashboardAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken = default);
}

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<PaginatedResult<AdminUserDto>> GetUsersAsync(int pageNumber, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<PaginatedResult<AdminAccountDto>> GetAccountsAsync(int pageNumber, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<PaginatedResult<AdminTransactionDto>> GetTransactionsAsync(int pageNumber, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<AdminTransactionDto> GetTransactionDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdminUserDetailsDto> SearchUserWithTransactionsAsync(string query, CancellationToken cancellationToken = default);
    Task<AdminUserDto> BanUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AdminUserDto> UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AdminUserDto> EditUserAsync(Guid userId, EditUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AdminUserDto> UpgradeUserTierAsync(Guid userId, UpgradeTierRequest request, CancellationToken cancellationToken = default);
    Task<List<LoanDto>> GetAllLoansAsync(CancellationToken cancellationToken = default);
    Task<LoanDto> ApproveLoanAsync(Guid loanId, ApproveLoanRequest request, CancellationToken cancellationToken = default);
    Task<LoanDto> RejectLoanAsync(Guid loanId, ApproveLoanRequest request, CancellationToken cancellationToken = default);
}

public interface IBillService
{
    Task<PayBillResponse> PayBillAsync(PayBillRequest request, CancellationToken cancellationToken = default);
}

public interface ISavingsService
{
    Task<List<FixedDepositDto>> GetUserFixedDepositsAsync(CancellationToken cancellationToken = default);
    Task<FixedDepositDto> CreateFixedDepositAsync(CreateFixedDepositRequest request, CancellationToken cancellationToken = default);
    Task<LiquidateDepositResponse> LiquidateFixedDepositAsync(Guid depositId, CancellationToken cancellationToken = default);
}

public interface ILoanService
{
    Task<List<LoanDto>> GetUserLoansAsync(CancellationToken cancellationToken = default);
    Task<LoanDto> ApplyLoanAsync(ApplyLoanRequest request, CancellationToken cancellationToken = default);
}

