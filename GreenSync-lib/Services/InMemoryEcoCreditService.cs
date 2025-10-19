using GreenSync.Lib.Models;

namespace GreenSync.Lib.Services;

public class InMemoryEcoCreditService : IEcoCreditService
{
    private readonly List<EcoCredit> _ecoCredits = new();
    private readonly List<EcoCreditTransaction> _transactions = new();

    public InMemoryEcoCreditService()
    {
        // Seed with sample data
        SeedSampleData();
    }

    public Task<EcoCredit?> GetEcoCreditByUserIdAsync(string userId)
    {
        var ecoCredit = _ecoCredits.FirstOrDefault(e => e.UserId == userId);
        return Task.FromResult(ecoCredit);
    }

    public Task<EcoCredit> CreateEcoCreditAccountAsync(string userId)
    {
        var existingAccount = _ecoCredits.FirstOrDefault(e => e.UserId == userId);
        if (existingAccount != null)
            return Task.FromResult(existingAccount);

        var newAccount = new EcoCredit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CurrentBalance = 0,
            TotalEarned = 0,
            TotalRedeemed = 0,
            LastUpdated = DateTime.UtcNow,
            TransactionHistory = new List<EcoCreditTransaction>()
        };

        _ecoCredits.Add(newAccount);
        return Task.FromResult(newAccount);
    }

    public async Task<bool> AddCreditsAsync(string userId, decimal amount, string description, Guid? relatedReportId = null)
    {
        var account = await GetEcoCreditByUserIdAsync(userId);
        if (account == null)
        {
            account = await CreateEcoCreditAccountAsync(userId);
        }

        var transaction = new EcoCreditTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Type = TransactionType.Earned,
            Description = description,
            TransactionDate = DateTime.UtcNow,
            RelatedReportId = relatedReportId,
            ReferenceNumber = GenerateReferenceNumber(),
            BalanceAfter = account.CurrentBalance + amount
        };

        account.CurrentBalance += amount;
        account.TotalEarned += amount;
        account.LastUpdated = DateTime.UtcNow;
        account.TransactionHistory.Add(transaction);

        _transactions.Add(transaction);

        return true;
    }

    public async Task<bool> RedeemCreditsAsync(string userId, decimal amount, string description)
    {
        var account = await GetEcoCreditByUserIdAsync(userId);
        if (account == null || account.CurrentBalance < amount)
            return false;

        var transaction = new EcoCreditTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = -amount,
            Type = TransactionType.Redeemed,
            Description = description,
            TransactionDate = DateTime.UtcNow,
            ReferenceNumber = GenerateReferenceNumber(),
            BalanceAfter = account.CurrentBalance - amount
        };

        account.CurrentBalance -= amount;
        account.TotalRedeemed += amount;
        account.LastUpdated = DateTime.UtcNow;
        account.TransactionHistory.Add(transaction);

        _transactions.Add(transaction);

        return true;
    }

    public async Task<IEnumerable<EcoCreditTransaction>> GetTransactionHistoryAsync(string userId)
    {
        var account = await GetEcoCreditByUserIdAsync(userId);
        if (account == null)
            return Enumerable.Empty<EcoCreditTransaction>();

        return account.TransactionHistory.OrderByDescending(t => t.TransactionDate);
    }

    public async Task<decimal> GetBalanceAsync(string userId)
    {
        var account = await GetEcoCreditByUserIdAsync(userId);
        return account?.CurrentBalance ?? 0;
    }

    public async Task<bool> AdjustCreditsAsync(string userId, decimal amount, string reason)
    {
        var account = await GetEcoCreditByUserIdAsync(userId);
        if (account == null)
        {
            account = await CreateEcoCreditAccountAsync(userId);
        }

        var transaction = new EcoCreditTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Type = TransactionType.Adjustment,
            Description = reason,
            TransactionDate = DateTime.UtcNow,
            ReferenceNumber = GenerateReferenceNumber(),
            BalanceAfter = account.CurrentBalance + amount
        };

        account.CurrentBalance += amount;
        account.LastUpdated = DateTime.UtcNow;
        account.TransactionHistory.Add(transaction);

        _transactions.Add(transaction);

        return true;
    }

    public Task<IEnumerable<EcoCredit>> GetTopUsersAsync(int count = 10)
    {
        var topUsers = _ecoCredits
            .OrderByDescending(e => e.TotalEarned)
            .Take(count);

        return Task.FromResult(topUsers);
    }

    public Task<decimal> GetTotalCreditsInSystemAsync()
    {
        var totalCredits = _ecoCredits.Sum(e => e.CurrentBalance);
        return Task.FromResult(totalCredits);
    }

    private void SeedSampleData()
    {
        var user1Account = new EcoCredit
        {
            Id = Guid.NewGuid(),
            UserId = "user1",
            CurrentBalance = 125.75m,
            TotalEarned = 200.00m,
            TotalRedeemed = 74.25m,
            LastUpdated = DateTime.UtcNow.AddHours(-1),
            TransactionHistory = new List<EcoCreditTransaction>()
        };

        var user2Account = new EcoCredit
        {
            Id = Guid.NewGuid(),
            UserId = "user2",
            CurrentBalance = 87.50m,
            TotalEarned = 87.50m,
            TotalRedeemed = 0m,
            LastUpdated = DateTime.UtcNow.AddMinutes(-30),
            TransactionHistory = new List<EcoCreditTransaction>()
        };

        var user3Account = new EcoCredit
        {
            Id = Guid.NewGuid(),
            UserId = "user3",
            CurrentBalance = 45.25m,
            TotalEarned = 60.00m,
            TotalRedeemed = 14.75m,
            LastUpdated = DateTime.UtcNow.AddHours(-2),
            TransactionHistory = new List<EcoCreditTransaction>()
        };

        // Sample transactions for user1
        var user1Transactions = new List<EcoCreditTransaction>
        {
            new() {
                Id = Guid.NewGuid(),
                UserId = "user1",
                Amount = 50.00m,
                Type = TransactionType.Earned,
                Description = "Waste report submitted and collected",
                TransactionDate = DateTime.UtcNow.AddDays(-5),
                ReferenceNumber = "REF001",
                BalanceAfter = 50.00m
            },
            new() {
                Id = Guid.NewGuid(),
                UserId = "user1",
                Amount = 25.00m,
                Type = TransactionType.Earned,
                Description = "Recycling participation bonus",
                TransactionDate = DateTime.UtcNow.AddDays(-3),
                ReferenceNumber = "REF002",
                BalanceAfter = 75.00m
            },
            new() {
                Id = Guid.NewGuid(),
                UserId = "user1",
                Amount = -20.00m,
                Type = TransactionType.Redeemed,
                Description = "Municipal service discount",
                TransactionDate = DateTime.UtcNow.AddDays(-2),
                ReferenceNumber = "REF003",
                BalanceAfter = 55.00m
            }
        };

        user1Account.TransactionHistory.AddRange(user1Transactions);
        _transactions.AddRange(user1Transactions);

        _ecoCredits.AddRange(new[] { user1Account, user2Account, user3Account });
    }

    private static string GenerateReferenceNumber()
    {
        return $"REF{DateTime.UtcNow.Ticks % 1000000:D6}";
    }
}
