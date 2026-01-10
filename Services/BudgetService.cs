using CentuitionApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CentuitionApp.Services;

/// <summary>
/// Service implementation for managing budgets
/// </summary>
public class BudgetService : IBudgetService
{
    private readonly ApplicationDbContext _context;

    public BudgetService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Budget>> GetBudgetsAsync(string userId, int? year = null, int? month = null)
    {
        var query = _context.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == userId);

        if (year.HasValue)
        {
            query = query.Where(b => b.Year == year.Value);
        }

        if (month.HasValue)
        {
            query = query.Where(b => b.Month == month.Value);
        }

        var budgets = await query
            .OrderBy(b => b.Category!.Name)
            .ToListAsync();

        foreach (var budget in budgets)
        {
            budget.SpentAmount = await CalculateSpentAmountAsync(userId, budget.CategoryId, budget.Year, budget.Month);
        }

        return budgets;
    }

    public async Task<Budget?> GetBudgetByIdAsync(int id, string userId)
    {
        return await _context.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    }

    public async Task<Budget?> GetBudgetByCategoryAsync(string userId, int categoryId, int year, int month)
    {
        return await _context.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.UserId == userId 
                && b.CategoryId == categoryId 
                && b.Year == year 
                && b.Month == month);
    }

    public async Task<Budget> CreateBudgetAsync(Budget budget)
    {
        budget.CreatedAt = DateTime.UtcNow;

        var spentAmount = await CalculateSpentAmountAsync(budget.UserId, budget.CategoryId, budget.Year, budget.Month);
        budget.SpentAmount = spentAmount;

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        return budget;
    }

    public async Task<Budget> UpdateBudgetAsync(Budget budget)
    {
        var existing = await _context.Budgets
            .FirstOrDefaultAsync(b => b.Id == budget.Id && b.UserId == budget.UserId);

        if (existing == null)
        {
            throw new InvalidOperationException("Budget not found");
        }

        existing.Name = budget.Name;
        existing.Amount = budget.Amount;
        existing.IsActive = budget.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteBudgetAsync(int id, string userId)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (budget == null)
        {
            return false;
        }

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<BudgetProgress>> GetBudgetProgressAsync(string userId, int year, int month)
    {
        var budgets = await GetBudgetsAsync(userId, year, month);

        return budgets
            .Where(b => b.Category != null)
            .Select(b => new BudgetProgress
            {
                BudgetId = b.Id,
                CategoryName = b.Category!.Name,
                CategoryColor = b.Category.Color,
                BudgetAmount = b.Amount,
                SpentAmount = b.SpentAmount
            })
            .ToList();
    }

    public async Task UpdateBudgetSpendingAsync(string userId, int categoryId, int year, int month)
    {
        var budget = await GetBudgetByCategoryAsync(userId, categoryId, year, month);

        if (budget != null)
        {
            budget.SpentAmount = await CalculateSpentAmountAsync(userId, categoryId, year, month);
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task CopyBudgetsToNextMonthAsync(string userId, int fromYear, int fromMonth)
    {
        var sourceBudgets = await GetBudgetsAsync(userId, fromYear, fromMonth);

        var nextMonth = fromMonth == 12 ? 1 : fromMonth + 1;
        var nextYear = fromMonth == 12 ? fromYear + 1 : fromYear;

        foreach (var sourceBudget in sourceBudgets)
        {
            var existingBudget = await GetBudgetByCategoryAsync(userId, sourceBudget.CategoryId, nextYear, nextMonth);

            if (existingBudget == null)
            {
                var newBudget = new Budget
                {
                    Name = sourceBudget.Name,
                    Amount = sourceBudget.Amount,
                    CategoryId = sourceBudget.CategoryId,
                    Year = nextYear,
                    Month = nextMonth,
                    UserId = userId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                newBudget.SpentAmount = await CalculateSpentAmountAsync(userId, sourceBudget.CategoryId, nextYear, nextMonth);

                _context.Budgets.Add(newBudget);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task<decimal> CalculateSpentAmountAsync(string userId, int categoryId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        return await _context.Transactions
            .Where(t => t.UserId == userId
                && t.CategoryId == categoryId
                && t.Type == TransactionType.Expense
                && t.Date >= startDate
                && t.Date <= endDate)
            .SumAsync(t => t.Amount);
    }
}
