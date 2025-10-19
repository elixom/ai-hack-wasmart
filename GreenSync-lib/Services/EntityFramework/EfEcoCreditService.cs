using GreenSync.Lib.Data;
using GreenSync.Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSync.Lib.Services.EntityFramework;

/// <summary>
/// Entity Framework implementation of the EcoCredit service
/// </summary>
public class EfEcoCreditService : IEcoCreditService
{
    private readonly GreenSyncDbContext _context;
    private readonly ILogger<EfEcoCreditService> _logger;

    public EfEcoCreditService(GreenSyncDbContext context, ILogger<EfEcoCreditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EcoCredit?> GetEcoCreditByUserIdAsync(Guid userId)
    {
        try
        {
            return await _context.EcoCredits
                .Include(ec => ec.User)
                .Include(ec => ec.TransactionHistory.OrderByDescending(t => t.TransactionDate))
                .FirstOrDefaultAsync(ec => ec.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving EcoCredit for user {UserId}", userId);
            throw;
        }
    }

    public async Task<EcoCredit> CreateEcoCreditAccountAsync(Guid userId)
    {
        try
        {
            // Check if account already exists
            var existing = await _context.EcoCredits.FirstOrDefaultAsync(ec => ec.UserId == userId);
            if (existing != null)
            {
                return existing;
            }

            var ecoCredit = new EcoCredit
            {
                UserId = userId,
                CurrentBalance = 0m,
                TotalEarned = 0m,
                TotalRedeemed = 0m,
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            _context.EcoCredits.Add(ecoCredit);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created EcoCredit account for user {UserId}", userId);
            return ecoCredit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating EcoCredit account for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> AddCreditsAsync(Guid userId, decimal amount, string description, Guid? relatedReportId = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Get or create eco-credit account
            var ecoCredit = await GetEcoCreditByUserIdAsync(userId);
            if (ecoCredit == null)
            {
                ecoCredit = await CreateEcoCreditAccountAsync(userId);
            }

            // Update balance
            ecoCredit.CurrentBalance += amount;
            ecoCredit.TotalEarned += amount;
            ecoCredit.LastUpdated = DateTime.UtcNow;

            // Create transaction record
            var ecoCreditTransaction = new EcoCreditTransaction
            {
                UserId = userId,
                EcoCreditId = ecoCredit.Id,
                Amount = amount,
                Type = TransactionType.Earned,
                Description = description,
                TransactionDate = DateTime.UtcNow,
                RelatedReportId = relatedReportId,
                ReferenceNumber = GenerateReferenceNumber(),
                BalanceAfter = ecoCredit.CurrentBalance
            };

            _context.EcoCreditTransactions.Add(ecoCreditTransaction);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Added {Amount} credits to user {UserId}. New balance: {Balance}", 
                amount, userId, ecoCredit.CurrentBalance);
            
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error adding credits to user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> RedeemCreditsAsync(Guid userId, decimal amount, string description)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var ecoCredit = await GetEcoCreditByUserIdAsync(userId);
            if (ecoCredit == null || ecoCredit.CurrentBalance < amount)
            {
                return false; // Insufficient balance
            }

            // Update balance
            ecoCredit.CurrentBalance -= amount;
            ecoCredit.TotalRedeemed += amount;
            ecoCredit.LastUpdated = DateTime.UtcNow;

            // Create transaction record
            var ecoCreditTransaction = new EcoCreditTransaction
            {
                UserId = userId,
                EcoCreditId = ecoCredit.Id,
                Amount = -amount, // Negative for redemption
                Type = TransactionType.Redeemed,
                Description = description,
                TransactionDate = DateTime.UtcNow,
                ReferenceNumber = GenerateReferenceNumber(),
                BalanceAfter = ecoCredit.CurrentBalance
            };

            _context.EcoCreditTransactions.Add(ecoCreditTransaction);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Redeemed {Amount} credits from user {UserId}. New balance: {Balance}", 
                amount, userId, ecoCredit.CurrentBalance);
            
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error redeeming credits for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<EcoCreditTransaction>> GetTransactionHistoryAsync(Guid userId)
    {
        try
        {
            return await _context.EcoCreditTransactions
                .Where(t => t.UserId == userId)
                .Include(t => t.RelatedReport)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction history for user {UserId}", userId);
            throw;
        }
    }

    public async Task<decimal> GetBalanceAsync(Guid userId)
    {
        try
        {
            var ecoCredit = await _context.EcoCredits
                .FirstOrDefaultAsync(ec => ec.UserId == userId);
            
            return ecoCredit?.CurrentBalance ?? 0m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> AdjustCreditsAsync(Guid userId, decimal amount, string reason)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get or create eco-credit account
            var ecoCredit = await GetEcoCreditByUserIdAsync(userId);
            if (ecoCredit == null)
            {
                ecoCredit = await CreateEcoCreditAccountAsync(userId);
            }

            // Update balance
            ecoCredit.CurrentBalance += amount;
            if (amount > 0)
            {
                ecoCredit.TotalEarned += amount;
            }
            else
            {
                ecoCredit.TotalRedeemed += Math.Abs(amount);
            }
            ecoCredit.LastUpdated = DateTime.UtcNow;

            // Create transaction record
            var ecoCreditTransaction = new EcoCreditTransaction
            {
                UserId = userId,
                EcoCreditId = ecoCredit.Id,
                Amount = amount,
                Type = TransactionType.Adjustment,
                Description = $"Admin adjustment: {reason}",
                TransactionDate = DateTime.UtcNow,
                ReferenceNumber = GenerateReferenceNumber(),
                BalanceAfter = ecoCredit.CurrentBalance
            };

            _context.EcoCreditTransactions.Add(ecoCreditTransaction);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Adjusted credits for user {UserId} by {Amount}. Reason: {Reason}", 
                userId, amount, reason);
            
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error adjusting credits for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<EcoCredit>> GetTopUsersAsync(int count = 10)
    {
        try
        {
            return await _context.EcoCredits
                .Include(ec => ec.User)
                .Where(ec => ec.User.IsActive && !ec.User.IsDeleted)
                .OrderByDescending(ec => ec.TotalEarned)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top users");
            throw;
        }
    }

    public async Task<decimal> GetTotalCreditsInSystemAsync()
    {
        try
        {
            return await _context.EcoCredits
                .SumAsync(ec => ec.CurrentBalance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total credits in system");
            throw;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Generate a unique reference number for transactions
    /// </summary>
    private static string GenerateReferenceNumber()
    {
        return $"ECT{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }

    #endregion
}
