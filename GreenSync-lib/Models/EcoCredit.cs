using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GreenSync.Lib.Models;

public class EcoCredit
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [DisplayName("User ID")]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [DisplayName("Current Balance")]
    public decimal CurrentBalance { get; set; }
    
    [DisplayName("Total Earned")]
    public decimal TotalEarned { get; set; }
    
    [DisplayName("Total Redeemed")]
    public decimal TotalRedeemed { get; set; }
    
    [DisplayName("Last Updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    [DisplayName("Transaction History")]
    public List<EcoCreditTransaction> TransactionHistory { get; set; } = new();
}

public class EcoCreditTransaction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [DisplayName("User ID")]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [DisplayName("Amount")]
    public decimal Amount { get; set; }
    
    [Required]
    [DisplayName("Transaction Type")]
    public TransactionType Type { get; set; }
    
    [Required]
    [DisplayName("Description")]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [DisplayName("Transaction Date")]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    [DisplayName("Related Report ID")]
    public Guid? RelatedReportId { get; set; }
    
    [DisplayName("Reference Number")]
    public string? ReferenceNumber { get; set; }
    
    [DisplayName("Balance After Transaction")]
    public decimal BalanceAfter { get; set; }
}

public enum TransactionType
{
    Earned,
    Redeemed,
    Bonus,
    Penalty,
    Adjustment
}
