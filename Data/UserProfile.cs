using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CentuitionApp.Data;

/// <summary>
/// Extended user profile with personal finance preferences and settings
/// </summary>
public class UserProfile
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to ASP.NET Identity User
    /// </summary>
    [Required]
    [StringLength(450)]
    public string IdentityUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(IdentityUserId))]
    [JsonIgnore]
    public virtual ApplicationUser? IdentityUser { get; set; }

    // Personal Information
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(255)]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    // Regional Settings
    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = "USD";

    [Required]
    [StringLength(10)]
    public string CurrencySymbol { get; set; } = "$";

    [Required]
    [StringLength(50)]
    public string Locale { get; set; } = "en-US";

    [Required]
    [StringLength(50)]
    public string DateFormat { get; set; } = "MM/dd/yyyy";

    [Required]
    [StringLength(50)]
    public string TimeZone { get; set; } = "UTC";

    // Budget Settings
    public int BudgetStartDay { get; set; } = 1;

    public bool ShowCentsInAmounts { get; set; } = true;

    // Notification Preferences
    public bool EmailNotifications { get; set; } = true;

    public bool BudgetAlerts { get; set; } = true;

    [Column(TypeName = "decimal(18,2)")]
    public decimal BudgetAlertThreshold { get; set; } = 0.80m;

    // UI Preferences
    [Required]
    [StringLength(20)]
    public string Theme { get; set; } = "light";

    public bool CompactView { get; set; } = false;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    [JsonIgnore]
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    [JsonIgnore]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    [JsonIgnore]
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    [JsonIgnore]
    public virtual ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();

    // Computed property
    [NotMapped]
    public string FullName => !string.IsNullOrWhiteSpace(FirstName) 
        ? $"{FirstName} {LastName}".Trim() 
        : DisplayName ?? "User";
}
