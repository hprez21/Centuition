using CentuitionApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CentuitionApp.Services;

/// <summary>
/// Service implementation for managing categories
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public CategoryService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Category>> GetAllCategoriesAsync(string? userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Categories
            .Where(c => c.IsSystem || c.UserId == userId)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Category>> GetCategoriesByTypeAsync(string? userId, CategoryType type)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Categories
            .Where(c => (c.IsSystem || c.UserId == userId) && c.Type == type && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Categories.FindAsync(id);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        category.CreatedAt = DateTime.UtcNow;
        category.IsSystem = false;

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Categories.FindAsync(category.Id);

        if (existing == null)
        {
            throw new InvalidOperationException("Category not found");
        }

        if (existing.IsSystem)
        {
            existing.Color = category.Color;
            existing.Icon = category.Icon;
            existing.SortOrder = category.SortOrder;
        }
        else
        {
            existing.Name = category.Name;
            existing.Description = category.Description;
            existing.Type = category.Type;
            existing.Color = category.Color;
            existing.Icon = category.Icon;
            existing.ParentCategoryId = category.ParentCategoryId;
            existing.SortOrder = category.SortOrder;
            existing.IsActive = category.IsActive;
        }

        await context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteCategoryAsync(int id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsSystem);

        if (category == null)
        {
            return false;
        }

        var hasTransactions = await context.Transactions.AnyAsync(t => t.CategoryId == id);

        if (hasTransactions)
        {
            category.IsActive = false;
        }
        else
        {
            context.Categories.Remove(category);
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Category>> GetSystemCategoriesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Categories
            .Where(c => c.IsSystem && c.IsActive)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<List<CategorySpending>> GetCategorySpendingAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.CategoryId.HasValue);

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(t => t.Date >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1);
            query = query.Where(t => t.Date < end);
        }

        var groupedData = await query
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key!.Value,
                TotalAmount = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .ToListAsync();

        var categoryIds = groupedData.Select(g => g.CategoryId).ToList();
        var categories = await context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => new { c.Name, c.Color });

        var spending = groupedData
            .Where(g => categories.ContainsKey(g.CategoryId))
            .Select(g => new CategorySpending
            {
                CategoryId = g.CategoryId,
                CategoryName = categories[g.CategoryId].Name,
                CategoryColor = categories[g.CategoryId].Color,
                TotalAmount = (double)g.TotalAmount,
                TransactionCount = g.TransactionCount
            })
            .OrderByDescending(s => s.TotalAmount)
            .ToList();

        var total = spending.Sum(s => s.TotalAmount);
        foreach (var item in spending)
        {
            item.Percentage = total > 0 ? Math.Round((item.TotalAmount / total) * 100, 1) : 0;
        }

        return spending;
    }

    public async Task<List<CategorySpending>> GetIncomeByCategory(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Income && t.CategoryId.HasValue);

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(t => t.Date >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1);
            query = query.Where(t => t.Date < end);
        }

        var groupedData = await query
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key!.Value,
                TotalAmount = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .ToListAsync();

        var categoryIds = groupedData.Select(g => g.CategoryId).ToList();
        var categories = await context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => new { c.Name, c.Color });

        var income = groupedData
            .Where(g => categories.ContainsKey(g.CategoryId))
            .Select(g => new CategorySpending
            {
                CategoryId = g.CategoryId,
                CategoryName = categories[g.CategoryId].Name,
                CategoryColor = categories[g.CategoryId].Color,
                TotalAmount = (double)g.TotalAmount,
                TransactionCount = g.TransactionCount
            })
            .OrderByDescending(s => s.TotalAmount)
            .ToList();

        var total = income.Sum(s => s.TotalAmount);
        foreach (var item in income)
        {
            item.Percentage = total > 0 ? Math.Round((item.TotalAmount / total) * 100, 1) : 0;
        }

        return income;
    }
}
