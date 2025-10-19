using GreenSync.Lib.Models;

namespace GreenSync.Lib.Services;

public interface IEcoCreditService
{
    Task<EcoCredit?> GetEcoCreditByUserIdAsync(string userId);
    Task<EcoCredit> CreateEcoCreditAccountAsync(string userId);
    Task<bool> AddCreditsAsync(string userId, decimal amount, string description, Guid? relatedReportId = null);
    Task<bool> RedeemCreditsAsync(string userId, decimal amount, string description);
    Task<IEnumerable<EcoCreditTransaction>> GetTransactionHistoryAsync(string userId);
    Task<decimal> GetBalanceAsync(string userId);
    Task<bool> AdjustCreditsAsync(string userId, decimal amount, string reason);
    Task<IEnumerable<EcoCredit>> GetTopUsersAsync(int count = 10);
    Task<decimal> GetTotalCreditsInSystemAsync();
}
