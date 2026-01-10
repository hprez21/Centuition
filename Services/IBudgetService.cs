using CentuitionApp.Data;

namespace CentuitionApp.Services;

/// <summary>
/// Service interface for managing budgets
/// </summary>
public interface IBudgetService
{
    Task<List<Budget>> GetBudgetsAsync(string userId, int? year = null, int? month = null);
    Task<Budget?> GetBudgetByIdAsync(int id, string userId);
    Task<Budget?> GetBudgetByCategoryAsync(string userId, int categoryId, int year, int month);
    Task<Budget> CreateBudgetAsync(Budget budget);
    Task<Budget> UpdateBudgetAsync(Budget budget);
    Task<bool> DeleteBudgetAsync(int id, string userId);
    Task<List<BudgetProgress>> GetBudgetProgressAsync(string userId, int year, int month);
    Task UpdateBudgetSpendingAsync(string userId, int categoryId, int year, int month);
    Task CopyBudgetsToNextMonthAsync(string userId, int fromYear, int fromMonth);
}

/// <summary>
/// Model for budget progress tracking
/// </summary>
public class BudgetProgress
{
    public int BudgetId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#6c757d";
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount => BudgetAmount - SpentAmount;
    public decimal PercentageUsed => BudgetAmount > 0 ? Math.Round((SpentAmount / BudgetAmount) * 100, 1) : 0;
    public bool IsOverBudget => SpentAmount > BudgetAmount;
    public string Status => IsOverBudget ? "Over Budget" : PercentageUsed >= 80 ? "Warning" : "On Track";
}
