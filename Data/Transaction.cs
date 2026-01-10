using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CentuitionApp.Data;

/// <summary>
/// Represents a financial transaction (expense, income, or transfer)
/// </summary>
public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign key to Account
    [Required]
    public int AccountId { get; set; }

    [ForeignKey(nameof(AccountId))]
    [JsonIgnore]
    public virtual Account? Account { get; set; }

    // Foreign key for transfer destination account (only for transfers)
    public int? DestinationAccountId { get; set; }

    [ForeignKey(nameof(DestinationAccountId))]
    [JsonIgnore]
    public virtual Account? DestinationAccount { get; set; }

    // Foreign key to Category
    public int? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    [JsonIgnore]
    public virtual Category? Category { get; set; }

    // Foreign key to ApplicationUser
    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    [JsonIgnore]
    public virtual ApplicationUser? User { get; set; }

    // Link to recurring transaction if this was auto-generated
    public int? RecurringTransactionId { get; set; }

    [ForeignKey(nameof(RecurringTransactionId))]
    [JsonIgnore]
    public virtual RecurringTransaction? RecurringTransaction { get; set; }

    // Tags for additional categorization (stored as comma-separated values)
    [StringLength(500)]
    public string? Tags { get; set; }

    // Flag to mark if transaction is confirmed/reconciled
    public bool IsReconciled { get; set; } = false;

    // Optional link to UserProfile (for future use)
    public int? UserProfileId { get; set; }

    [ForeignKey(nameof(UserProfileId))]
    [JsonIgnore]
    public virtual UserProfile? UserProfile { get; set; }
}
