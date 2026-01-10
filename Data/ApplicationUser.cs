using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CentuitionApp.Data;

/// <summary>
/// Extended user profile with personal finance preferences
/// </summary>
public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(3)]
    public string Currency { get; set; } = "USD";

    [StringLength(50)]
    public string? Locale { get; set; } = "en-US";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual UserProfile? Profile { get; set; }
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public virtual ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();

    // Computed property for display name
    [NotMapped]
    public string DisplayName => !string.IsNullOrWhiteSpace(FirstName) ? $"{FirstName} {LastName}".Trim() : Email ?? UserName ?? "User";
}
