using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CentuitionApp.Data;

/// <summary>
/// Represents a recurring/scheduled transaction template
/// </summary>
public class RecurringTransaction
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
    public RecurrenceFrequency Frequency { get; set; } = RecurrenceFrequency.Monthly;

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? NextDueDate { get; set; }

    public DateTime? LastProcessedDate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool AutoCreate { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign key to Account
    [Required]
    public int AccountId { get; set; }

    [ForeignKey(nameof(AccountId))]
    [JsonIgnore]
    public virtual Account? Account { get; set; }

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

    // Optional link to UserProfile (for future use)
    public int? UserProfileId { get; set; }

    [ForeignKey(nameof(UserProfileId))]
    [JsonIgnore]
    public virtual UserProfile? UserProfile { get; set; }

    // Navigation property for generated transactions
    [JsonIgnore]
    public virtual ICollection<Transaction> GeneratedTransactions { get; set; } = new List<Transaction>();
}
