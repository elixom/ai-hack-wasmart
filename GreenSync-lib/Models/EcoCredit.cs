using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSync.Lib.Models;

/// <summary>
/// Represents a user's eco-credit account for environmental rewards
/// </summary>
[Table("EcoCredits")]
public class EcoCredit
{
    /// <summary>
    /// Primary key - EcoCredit account ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Associated user ID (one-to-one relationship)
    /// </summary>
    [Required]
    [DisplayName("User ID")]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Current available credit balance
    /// </summary>
    [Required]
    [DisplayName("Current Balance")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal CurrentBalance { get; set; }
    
    /// <summary>
    /// Total credits earned lifetime
    /// </summary>
    [DisplayName("Total Earned")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalEarned { get; set; }
    
    /// <summary>
    /// Total credits redeemed lifetime
    /// </summary>
    [DisplayName("Total Redeemed")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalRedeemed { get; set; }
    
    /// <summary>
    /// Account creation timestamp
    /// </summary>
    [Required]
    [DisplayName("Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last balance update timestamp
    /// </summary>
    [DisplayName("Last Updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Soft delete timestamp
    /// </summary>
    [DisplayName("Deleted At")]
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Associated user
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
    
    /// <summary>
    /// Transaction history for this account
    /// </summary>
    public virtual ICollection<EcoCreditTransaction> TransactionHistory { get; set; } = new List<EcoCreditTransaction>();
}

/// <summary>
/// Represents individual eco-credit transactions
/// </summary>
[Table("EcoCreditTransactions")]
public class EcoCreditTransaction
{
    /// <summary>
    /// Primary key - Transaction ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Associated user ID
    /// </summary>
    [Required]
    [DisplayName("User ID")]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Associated eco-credit account ID
    /// </summary>
    [Required]
    [DisplayName("EcoCredit Account ID")]
    public Guid EcoCreditId { get; set; }
    
    /// <summary>
    /// Transaction amount (positive for earned, negative for redeemed)
    /// </summary>
    [Required]
    [DisplayName("Amount")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Type of transaction
    /// </summary>
    [Required]
    [DisplayName("Transaction Type")]
    public TransactionType Type { get; set; }
    
    /// <summary>
    /// Transaction description
    /// </summary>
    [Required]
    [StringLength(500)]
    [DisplayName("Description")]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// When the transaction occurred
    /// </summary>
    [Required]
    [DisplayName("Transaction Date")]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Related report ID (if earned from a report)
    /// </summary>
    [DisplayName("Related Report ID")]
    public Guid? RelatedReportId { get; set; }
    
    /// <summary>
    /// Unique reference number for tracking
    /// </summary>
    [StringLength(50)]
    [DisplayName("Reference Number")]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Account balance after this transaction
    /// </summary>
    [DisplayName("Balance After Transaction")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal BalanceAfter { get; set; }

    // Navigation properties
    /// <summary>
    /// Associated user
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
    
    /// <summary>
    /// Associated eco-credit account
    /// </summary>
    [ForeignKey(nameof(EcoCreditId))]
    public virtual EcoCredit EcoCredit { get; set; } = null!;
    
    /// <summary>
    /// Related report (if applicable)
    /// </summary>
    [ForeignKey(nameof(RelatedReportId))]
    public virtual Report? RelatedReport { get; set; }
}

public enum TransactionType
{
    Earned,
    Redeemed,
    Bonus,
    Penalty,
    Adjustment
}
