using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;

namespace VaultPay.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context, 
        UserManager<User> userManager, 
        RoleManager<Role> roleManager)
    {
        await context.Database.EnsureCreatedAsync();

        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""Loans"" (
                    ""Id"" TEXT NOT NULL PRIMARY KEY,
                    ""UserId"" TEXT NOT NULL,
                    ""AccountId"" TEXT NOT NULL,
                    ""Amount"" TEXT NOT NULL,
                    ""Purpose"" TEXT NOT NULL,
                    ""DurationMonths"" INTEGER NOT NULL,
                    ""InterestRate"" TEXT NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""AdminNotes"" TEXT,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ApprovedAt"" TEXT,
                    ""RepaidAt"" TEXT
                );
                CREATE TABLE IF NOT EXISTS ""FixedDeposits"" (
                    ""Id"" TEXT NOT NULL PRIMARY KEY,
                    ""UserId"" TEXT NOT NULL,
                    ""AccountId"" TEXT NOT NULL,
                    ""Amount"" TEXT NOT NULL,
                    ""InterestRatePerAnnum"" TEXT NOT NULL,
                    ""DurationDays"" INTEGER NOT NULL,
                    ""StartDate"" TEXT NOT NULL,
                    ""MaturityDate"" TEXT NOT NULL,
                    ""Status"" INTEGER NOT NULL,
                    ""PenaltyRate"" TEXT NOT NULL,
                    ""ProjectedInterest"" TEXT NOT NULL,
                    ""ActualPayout"" TEXT,
                    ""ClosedAt"" TEXT
                );
            ");
        }
        catch { }

        try
        {
            await context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""AspNetUsers"" ADD COLUMN ""Tier"" INTEGER NOT NULL DEFAULT 1;");
        }
        catch { }

        // 1. Seed Roles
        string[] roles = ["Admin", "Customer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new Role(role));
            }
        }

        // 2. Seed Admin User
        var adminEmail = "admin@vaultpay.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "VaultPay System Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };

            await userManager.CreateAsync(adminUser, "Admin@12345");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // 3. Seed Counterparty Customers
        var chinonsoEmail = "chinonso.okeke@example.com";
        var chinonso = await userManager.FindByEmailAsync(chinonsoEmail);
        if (chinonso == null)
        {
            chinonso = new User
            {
                Id = Guid.NewGuid(),
                UserName = chinonsoEmail,
                Email = chinonsoEmail,
                FullName = "Chinonso Okeke",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            };
            await userManager.CreateAsync(chinonso, "Customer@12345");
            await userManager.AddToRoleAsync(chinonso, "Customer");

            var cAccount = new Account
            {
                Id = Guid.NewGuid(),
                UserId = chinonso.Id,
                AccountNumber = "1038291047",
                AccountName = "Chinonso Okeke's Wallet",
                CurrencyCode = "NGN",
                Balance = 750000.00m,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            };
            context.Accounts.Add(cAccount);
        }

        var adewaleEmail = "adewale.adeleke@example.com";
        var adewale = await userManager.FindByEmailAsync(adewaleEmail);
        if (adewale == null)
        {
            adewale = new User
            {
                Id = Guid.NewGuid(),
                UserName = adewaleEmail,
                Email = adewaleEmail,
                FullName = "Adewale Adeleke",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };
            await userManager.CreateAsync(adewale, "Customer@12345");
            await userManager.AddToRoleAsync(adewale, "Customer");

            var aAccount = new Account
            {
                Id = Guid.NewGuid(),
                UserId = adewale.Id,
                AccountNumber = "1047291844",
                AccountName = "Adewale Adeleke's Wallet",
                CurrencyCode = "NGN",
                Balance = 1200000.00m,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };
            context.Accounts.Add(aAccount);
        }

        // 4. Seed Primary Customer (Alex Morgan)
        var customerEmail = "alex.morgan@vaultpay.com";
        var customerUser = await userManager.FindByEmailAsync(customerEmail);
        if (customerUser == null)
        {
            customerUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = customerEmail,
                Email = customerEmail,
                FullName = "Alex Morgan",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            };

            await userManager.CreateAsync(customerUser, "Customer@12345");
            await userManager.AddToRoleAsync(customerUser, "Customer");

            var acc1 = new Account
            {
                Id = Guid.NewGuid(),
                UserId = customerUser.Id,
                AccountNumber = "1084920194",
                AccountName = "Primary Spending Account",
                CurrencyCode = "NGN",
                Balance = 3450250.00m,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            };

            var acc2 = new Account
            {
                Id = Guid.NewGuid(),
                UserId = customerUser.Id,
                AccountNumber = "1073829104",
                AccountName = "High-Yield Vault Saver (8.5% APY)",
                CurrencyCode = "NGN",
                Balance = 1850000.00m,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            };

            var acc3 = new Account
            {
                Id = Guid.NewGuid(),
                UserId = customerUser.Id,
                AccountNumber = "1092837411",
                AccountName = "Global Travel & SaaS Vault",
                CurrencyCode = "USD",
                Balance = 2450.00m,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };

            context.Accounts.AddRange(acc1, acc2, acc3);
            await context.SaveChangesAsync();

            // Seed initial transactions and ledger entries
            var tx1 = new Transaction
            {
                Id = Guid.NewGuid(),
                Reference = "VP-DEP-20261020-0012",
                ToAccountId = acc1.Id,
                Amount = 500000.00m,
                CurrencyCode = "NGN",
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Completed,
                Description = "Direct Bank Deposit from Providus Bank",
                InitiatedByUserId = customerUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                CompletedAt = DateTime.UtcNow.AddDays(-5)
            };

            var tx2 = new Transaction
            {
                Id = Guid.NewGuid(),
                Reference = "VP-TX-20261022-8172",
                FromAccountId = acc1.Id,
                ToAccountId = context.Accounts.FirstOrDefault(a => a.AccountNumber == "1038291047")?.Id,
                Amount = 75000.00m,
                CurrencyCode = "NGN",
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                Description = "Contract payment for Q3 Web Design",
                InitiatedByUserId = customerUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                CompletedAt = DateTime.UtcNow.AddDays(-3)
            };

            context.Transactions.AddRange(tx1, tx2);

            // Double-entry ledger records
            var le1 = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = tx1.Id,
                AccountId = acc1.Id,
                EntryType = LedgerEntryType.Credit,
                Amount = 500000.00m,
                CurrencyCode = "NGN",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            var le2 = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = tx2.Id,
                AccountId = acc1.Id,
                EntryType = LedgerEntryType.Debit,
                Amount = 75000.00m,
                CurrencyCode = "NGN",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            };

            context.LedgerEntries.AddRange(le1, le2);
            await context.SaveChangesAsync();
        }

        // Ensure sample loans and fixed deposits exist for demo
        var morgan = await userManager.FindByEmailAsync(customerEmail);
        if (morgan != null)
        {
            var primaryAcc = await context.Accounts.FirstOrDefaultAsync(a => a.UserId == morgan.Id);
            if (primaryAcc != null)
            {
                if (!await context.Loans.AnyAsync(l => l.UserId == morgan.Id))
                {
                    context.Loans.Add(new Loan
                    {
                        Id = Guid.NewGuid(),
                        UserId = morgan.Id,
                        AccountId = primaryAcc.Id,
                        Amount = 250000m,
                        Purpose = "Working Capital & Inventory Growth",
                        DurationMonths = 6,
                        InterestRate = 5.0m,
                        Status = LoanStatus.Pending,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    });
                }

                if (!await context.FixedDeposits.AnyAsync(f => f.UserId == morgan.Id))
                {
                    context.FixedDeposits.Add(new FixedDeposit
                    {
                        Id = Guid.NewGuid(),
                        UserId = morgan.Id,
                        AccountId = primaryAcc.Id,
                        Amount = 500000m,
                        InterestRatePerAnnum = 0.25m, // 25% per annum
                        DurationDays = 180,
                        StartDate = DateTime.UtcNow.AddDays(-20),
                        MaturityDate = DateTime.UtcNow.AddDays(160),
                        Status = FixedDepositStatus.Active,
                        PenaltyRate = 0.10m,
                        ProjectedInterest = FixedDeposit.CalculateProjectedInterest(500000m, 180, 0.25m)
                    });
                }
                await context.SaveChangesAsync();
            }
        }

        await context.SaveChangesAsync();
    }
}
