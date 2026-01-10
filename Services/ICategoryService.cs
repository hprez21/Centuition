using CentuitionApp.Data;

namespace CentuitionApp.Services;

/// <summary>
/// Service interface for managing categories
/// </summary>
public interface ICategoryService
{
    Task<List<Category>> GetAllCategoriesAsync(string? userId);
    Task<List<Category>> GetCategoriesByTypeAsync(string? userId, CategoryType type);
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(int id, string userId);
    Task<List<Category>> GetSystemCategoriesAsync();
    Task<List<CategorySpending>> GetCategorySpendingAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<CategorySpending>> GetIncomeByCategory(string userId, DateTime? startDate = null, DateTime? endDate = null);
}

/// <summary>
/// Model for category spending summary
/// </summary>
public class CategorySpending
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#6c757d";
    public double TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public double Percentage { get; set; }
}
