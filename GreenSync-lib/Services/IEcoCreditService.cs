using GreenSync.Lib.Models;

namespace GreenSync.Lib.Services;

public interface IEcoCreditService
{
    Task<EcoCredit?> GetEcoCreditByUserIdAsync(Guid userId);
    Task<EcoCredit> CreateEcoCreditAccountAsync(Guid userId);
    Task<bool> AddCreditsAsync(Guid userId, decimal amount, string description, Guid? relatedReportId = null);
    Task<bool> RedeemCreditsAsync(Guid userId, decimal amount, string description);
    Task<IEnumerable<EcoCreditTransaction>> GetTransactionHistoryAsync(Guid userId);
    Task<decimal> GetBalanceAsync(Guid userId);
    Task<bool> AdjustCreditsAsync(Guid userId, decimal amount, string reason);
    Task<IEnumerable<EcoCredit>> GetTopUsersAsync(int count = 10);
    Task<decimal> GetTotalCreditsInSystemAsync();
}
