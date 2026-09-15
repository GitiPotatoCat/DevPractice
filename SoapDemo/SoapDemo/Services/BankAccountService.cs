using SoapDemo.Contracts;
using SoapDemo.Models;

namespace SoapDemo.Services;

public sealed class BankAccountService : IBankAccountService
{
    private readonly ILogger<BankAccountService> _logger;

    // ── In-memory store (replaces a real DB for this demo) ────
    private static readonly Dictionary<string, (string Holder, decimal Balance, string Type)> _accounts = new()
    {
        ["ACC001"] = ("Alice Johnson", 5_000.00m, "Savings"),
        ["ACC002"] = ("Bob Martinez", 12_500.75m, "Current"),
        ["ACC003"] = ("Carol White", 1_250.50m, "Savings"),
    };
    
    public BankAccountService(ILogger<BankAccountService> logger)
    {
        _logger = logger;
    }


    public AccountInfo GetAccountInfo(string accountNumber)
    {
        BankAccountServiceLogs.GetAccountInfo(_logger, accountNumber);

        if (!_accounts.TryGetValue(accountNumber, out var accountInfo)) 
        {
            return new AccountInfo
            {
                AccountNumber = accountNumber,
                HolderName = "Not Found",
                Balance = 0m
            };
        }

        return new AccountInfo
        {
            AccountNumber = accountNumber,
            HolderName = accountInfo.Holder,
            Balance = accountInfo.Balance,
            Currency = "USD",
            AccountType = accountInfo.Type
        };
    }

    public TransactionResult Deposit(string accountNumber, decimal amount)
    {
        BankAccountServiceLogs.Deposit(_logger, accountNumber, amount);

        if (!_accounts.TryGetValue(accountNumber, out var accountInfo))
            return Fail("Account Not Found");

        if (amount <= 0)
            return Fail("Deposit amount must be greater than zero.");


        var updated = accountInfo.Balance + amount;
        _accounts[accountNumber] = (accountInfo.Holder, updated, accountInfo.Type);

        return Success(updated, $"Deposited {amount:C} successfully.");
    }

    public TransactionResult Withdraw(string accountNumber, decimal amount)
    {
        BankAccountServiceLogs.Withdraw(_logger, accountNumber, amount);

        if (!_accounts.TryGetValue(accountNumber, out var accountInfo))
            return Fail("No account found!");

        if (amount <= 0)
            return Fail("Withdraw amount needs to be greater than zero.");

        if (accountInfo.Balance < amount)
            return Fail("Insufficient Balance");


        var updated = accountInfo.Balance - amount;
        _accounts[accountNumber] = (accountInfo.Holder, updated, accountInfo.Type);

        return Success(updated, $"Withdraw {amount:C} successfully.");
    }
#region Private helpers 
    private static TransactionResult Success(decimal balance, string message) => new()
    {
        IsSuccess = true,
        TransactionId = $"TXN-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
        UpdatedBalance = balance,
        Message = message,
        ProcessedAt = DateTime.UtcNow
    };

    private static TransactionResult Fail(string reason) => new()
    {
        IsSuccess = false,
        TransactionId = string.Empty,
        Message = reason,
        ProcessedAt = DateTime.UtcNow
    };
#endregion
}



#region service Logs custom class
public static partial class BankAccountServiceLogs
{
    [LoggerMessage(EventId = 1, EventName = "GetAccountInfo", Level = LogLevel.Information, Message = "GetAccountInfo: {AccountNumber}")]
    public static partial void GetAccountInfo(
        ILogger logger,
        string accountNumber);


    [LoggerMessage(EventId = 2, EventName = "Deposit", Level = LogLevel.Information, Message = "Deposit: {AccountNumber} | Amount: {Amount}")]
    public static partial void Deposit(
        ILogger logger,
        string accountNumber,
        decimal amount);


    [LoggerMessage(EventId = 3, EventName = "Withdraw", Level = LogLevel.Information, Message = "Withdraw: {AccountNumber} | Amount: {Amount}")]
    public static partial void Withdraw(
        ILogger logger,
        string accountNumber,
        decimal amount);
}
#endregion
