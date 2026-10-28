namespace VaultPay.Domain.Enums;

public enum AccountStatus
{
    Active = 1,
    Suspended = 2,
    Closed = 3
}

public enum TransactionType
{
    Transfer = 1,
    Deposit = 2,
    BillPayment = 3,
    LoanDisbursement = 4,
    FixedDepositCreation = 5,
    FixedDepositLiquidation = 6
}

public enum TransactionStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3
}

public enum LedgerEntryType
{
    Debit = 1,
    Credit = 2
}

public enum IdempotencyStatus
{
    Processing = 1,
    Completed = 2,
    Failed = 3
}

public enum UserTier
{
    Tier1 = 1,
    Tier2 = 2,
    Tier3 = 3
}

public enum LoanStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Repaid = 4
}

public enum FixedDepositStatus
{
    Active = 1,
    LiquidatedEarly = 2,
    Matured = 3
}

public enum BillType
{
    Airtime = 1,
    Data = 2,
    Electricity = 3,
    Betting = 4
}

