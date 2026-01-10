using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CentuitionApp.Data;

/// <summary>
/// Represents a financial account (bank account, credit card, cash, etc.)
/// </summary>
public class Account
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public AccountType AccountType { get; set; } = AccountType.Checking;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal InitialBalance { get; set; } = 0;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; } = 0;

    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = "USD";

    [StringLength(50)]
    public string? Color { get; set; } = "#1b6ec2";

    [StringLength(50)]
    public string? Icon { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IncludeInTotal { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

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

    // Navigation properties
    [JsonIgnore]
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    [JsonIgnore]
    public virtual ICollection<Transaction> TransferDestinations { get; set; } = new List<Transaction>();
}
