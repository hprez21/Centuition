using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CentuitionApp.Data;

/// <summary>
/// Represents a budget for a specific category and time period
/// </summary>
public class Budget
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SpentAmount { get; set; } = 0;

    [Required]
    public int Month { get; set; }

    [Required]
    public int Year { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign key to Category
    [Required]
    public int CategoryId { get; set; }

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

    // Calculated property
    [NotMapped]
    public decimal RemainingAmount => Amount - SpentAmount;

    [NotMapped]
    public decimal PercentageUsed => Amount > 0 ? (SpentAmount / Amount) * 100 : 0;

    [NotMapped]
    public bool IsOverBudget => SpentAmount > Amount;
}
