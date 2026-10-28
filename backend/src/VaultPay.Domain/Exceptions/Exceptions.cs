namespace VaultPay.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class InsufficientFundsException : DomainException
{
    public decimal AvailableBalance { get; }
    public decimal AttemptedAmount { get; }

    public InsufficientFundsException(decimal availableBalance, decimal attemptedAmount)
        : base($"Insufficient funds. Available balance: {availableBalance:N2}, attempted: {attemptedAmount:N2}.")
    {
        AvailableBalance = availableBalance;
        AttemptedAmount = attemptedAmount;
    }
}

public class AccountInactiveException : DomainException
{
    public string AccountNumber { get; }

    public AccountInactiveException(string accountNumber, string status)
        : base($"Account {accountNumber} cannot transact because its status is {status}.")
    {
        AccountNumber = accountNumber;
    }
}

public class InvalidAmountException : DomainException
{
    public InvalidAmountException(string message) : base(message) { }
}

public class CurrencyMismatchException : DomainException
{
    public CurrencyMismatchException(string sourceCurrency, string destCurrency)
        : base($"Cross-currency transactions are currently not supported ({sourceCurrency} to {destCurrency}).") { }
}

public class SelfTransferException : DomainException
{
    public SelfTransferException() : base("Transfers to the same source account are not permitted.") { }
}

public class AccountNotFoundException : DomainException
{
    public AccountNotFoundException(string identifier) : base($"Account '{identifier}' was not found.") { }
}

public class IdempotencyConflictException : DomainException
{
    public IdempotencyConflictException(string message) : base(message) { }
}
