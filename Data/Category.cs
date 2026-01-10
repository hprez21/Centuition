using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CentuitionApp.Data;

/// <summary>
/// Represents a category for transactions (e.g., Food, Transport, Salary)
/// </summary>
public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public CategoryType Type { get; set; } = CategoryType.Expense;

    [StringLength(50)]
    public string Color { get; set; } = "#6c757d";

    [StringLength(50)]
    public string? Icon { get; set; }

    public int? ParentCategoryId { get; set; }

    [ForeignKey(nameof(ParentCategoryId))]
    [JsonIgnore]
    public virtual Category? ParentCategory { get; set; }

    public bool IsSystem { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key to ApplicationUser (null for system categories)
    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    [JsonIgnore]
    public virtual ApplicationUser? User { get; set; }

    // Optional link to UserProfile (for future use)
    public int? UserProfileId { get; set; }

    [ForeignKey(nameof(UserProfileId))]
    [JsonIgnore]
    public virtual UserProfile? UserProfile { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    [JsonIgnore]
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    [JsonIgnore]
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
