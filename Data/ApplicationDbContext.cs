using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CentuitionApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Budget> Budgets { get; set; } = null!;
    public DbSet<RecurringTransaction> RecurringTransactions { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Account configuration
        builder.Entity<Account>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Category configuration
        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.Name, e.Type });
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ParentCategory)
                .WithMany(e => e.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Transaction configuration
        builder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.UserId, e.Date });
            entity.HasIndex(e => new { e.UserId, e.AccountId, e.Date });

            entity.HasOne(e => e.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DestinationAccount)
                .WithMany(a => a.TransferDestinations)
                .HasForeignKey(e => e.DestinationAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.RecurringTransaction)
                .WithMany(r => r.GeneratedTransactions)
                .HasForeignKey(e => e.RecurringTransactionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Budget configuration
        builder.Entity<Budget>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.CategoryId, e.Year, e.Month }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Budgets)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RecurringTransaction configuration
        builder.Entity<RecurringTransaction>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.NextDueDate });

            entity.HasOne(e => e.User)
                .WithMany(u => u.RecurringTransactions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // UserProfile configuration
        builder.Entity<UserProfile>(entity =>
        {
            entity.HasIndex(e => e.IdentityUserId).IsUnique();

            entity.HasOne(e => e.IdentityUser)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(e => e.IdentityUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed default system categories
        SeedCategories(builder);
    }

    private static void SeedCategories(ModelBuilder builder)
    {
        // Static date for seed data to avoid migration issues
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Expense categories
        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Food & Dining", Type = CategoryType.Expense, Color = "#FF6B6B", Icon = "restaurant", IsSystem = true, SortOrder = 1, CreatedAt = seedDate },
            new Category { Id = 2, Name = "Transportation", Type = CategoryType.Expense, Color = "#4ECDC4", Icon = "car", IsSystem = true, SortOrder = 2, CreatedAt = seedDate },
            new Category { Id = 3, Name = "Housing", Type = CategoryType.Expense, Color = "#45B7D1", Icon = "home", IsSystem = true, SortOrder = 3, CreatedAt = seedDate },
            new Category { Id = 4, Name = "Utilities", Type = CategoryType.Expense, Color = "#96CEB4", Icon = "bolt", IsSystem = true, SortOrder = 4, CreatedAt = seedDate },
            new Category { Id = 5, Name = "Healthcare", Type = CategoryType.Expense, Color = "#FFEAA7", Icon = "medical", IsSystem = true, SortOrder = 5, CreatedAt = seedDate },
            new Category { Id = 6, Name = "Entertainment", Type = CategoryType.Expense, Color = "#DDA0DD", Icon = "movie", IsSystem = true, SortOrder = 6, CreatedAt = seedDate },
            new Category { Id = 7, Name = "Shopping", Type = CategoryType.Expense, Color = "#98D8C8", Icon = "shopping-cart", IsSystem = true, SortOrder = 7, CreatedAt = seedDate },
            new Category { Id = 8, Name = "Education", Type = CategoryType.Expense, Color = "#F7DC6F", Icon = "school", IsSystem = true, SortOrder = 8, CreatedAt = seedDate },
            new Category { Id = 9, Name = "Personal Care", Type = CategoryType.Expense, Color = "#BB8FCE", Icon = "spa", IsSystem = true, SortOrder = 9, CreatedAt = seedDate },
            new Category { Id = 10, Name = "Insurance", Type = CategoryType.Expense, Color = "#85C1E9", Icon = "shield", IsSystem = true, SortOrder = 10, CreatedAt = seedDate },
            new Category { Id = 11, Name = "Subscriptions", Type = CategoryType.Expense, Color = "#F1948A", Icon = "repeat", IsSystem = true, SortOrder = 11, CreatedAt = seedDate },
            new Category { Id = 12, Name = "Travel", Type = CategoryType.Expense, Color = "#82E0AA", Icon = "airplane", IsSystem = true, SortOrder = 12, CreatedAt = seedDate },
            new Category { Id = 13, Name = "Gifts & Donations", Type = CategoryType.Expense, Color = "#F5B7B1", Icon = "gift", IsSystem = true, SortOrder = 13, CreatedAt = seedDate },
            new Category { Id = 14, Name = "Other Expense", Type = CategoryType.Expense, Color = "#AEB6BF", Icon = "more-horizontal", IsSystem = true, SortOrder = 99, CreatedAt = seedDate },

            // Income categories
            new Category { Id = 15, Name = "Salary", Type = CategoryType.Income, Color = "#27AE60", Icon = "briefcase", IsSystem = true, SortOrder = 1, CreatedAt = seedDate },
            new Category { Id = 16, Name = "Freelance", Type = CategoryType.Income, Color = "#2ECC71", Icon = "laptop", IsSystem = true, SortOrder = 2, CreatedAt = seedDate },
            new Category { Id = 17, Name = "Investments", Type = CategoryType.Income, Color = "#1ABC9C", Icon = "trending-up", IsSystem = true, SortOrder = 3, CreatedAt = seedDate },
            new Category { Id = 18, Name = "Rental Income", Type = CategoryType.Income, Color = "#3498DB", Icon = "home", IsSystem = true, SortOrder = 4, CreatedAt = seedDate },
            new Category { Id = 19, Name = "Business", Type = CategoryType.Income, Color = "#9B59B6", Icon = "building", IsSystem = true, SortOrder = 5, CreatedAt = seedDate },
            new Category { Id = 20, Name = "Refunds", Type = CategoryType.Income, Color = "#E74C3C", Icon = "refresh", IsSystem = true, SortOrder = 6, CreatedAt = seedDate },
            new Category { Id = 21, Name = "Other Income", Type = CategoryType.Income, Color = "#95A5A6", Icon = "plus-circle", IsSystem = true, SortOrder = 99, CreatedAt = seedDate }
        );
    }
}
