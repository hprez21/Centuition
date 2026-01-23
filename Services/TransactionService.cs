using CentuitionApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CentuitionApp.Services;

/// <summary>
/// Service implementation for managing financial transactions
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IAccountService _accountService;

    public TransactionService(IDbContextFactory<ApplicationDbContext> contextFactory, IAccountService accountService)
    {
        _contextFactory = contextFactory;
        _accountService = accountService;
    }

    public async Task<List<Transaction>> GetTransactionsAsync(
        string userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? categoryId = null,
        int? accountId = null,
        TransactionType? type = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.DestinationAccount)
            .Where(t => t.UserId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(t => t.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.Date <= endDate.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (accountId.HasValue)
        {
            query = query.Where(t => t.AccountId == accountId.Value || t.DestinationAccountId == accountId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        return await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.DestinationAccount)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        transaction.CreatedAt = DateTime.UtcNow;

        await UpdateAccountBalancesForNewTransaction(transaction);

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        return transaction;
    }

    public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Transactions
            .FirstOrDefaultAsync(t => t.Id == transaction.Id && t.UserId == transaction.UserId);

        if (existing == null)
        {
            throw new InvalidOperationException("Transaction not found");
        }

        await ReverseTransactionEffect(existing);

        existing.Amount = transaction.Amount;
        existing.Type = transaction.Type;
        existing.Description = transaction.Description;
        existing.Notes = transaction.Notes;
        existing.Date = transaction.Date;
        existing.AccountId = transaction.AccountId;
        existing.DestinationAccountId = transaction.DestinationAccountId;
        existing.CategoryId = transaction.CategoryId;
        existing.Tags = transaction.Tags;
        existing.IsReconciled = transaction.IsReconciled;
        existing.UpdatedAt = DateTime.UtcNow;

        await UpdateAccountBalancesForNewTransaction(existing);

        await context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteTransactionAsync(int id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var transaction = await context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            return false;
        }

        await ReverseTransactionEffect(transaction);

        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<decimal> GetTotalIncomeAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Income);

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

        return await query.SumAsync(t => t.Amount);
    }

    public async Task<decimal> GetTotalExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense);

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

        return await query.SumAsync(t => t.Amount);
    }

    public async Task<Dictionary<int, decimal>> GetExpensesByCategoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
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

        return await query
            .GroupBy(t => t.CategoryId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(t => t.Amount));
    }

    public async Task<List<TransactionSummary>> GetMonthlyTrendsAsync(string userId, int months = 12)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-months + 1);

        var transactions = await context.Transactions
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Type != TransactionType.Transfer)
            .ToListAsync();

        var summaries = transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new TransactionSummary
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalIncome = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                TotalExpenses = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            })
            .OrderBy(s => s.Year)
            .ThenBy(s => s.Month)
            .ToList();

        return summaries;
    }

    public async Task<List<Transaction>> GetRecentTransactionsAsync(string userId, int count = 10)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    private async Task UpdateAccountBalancesForNewTransaction(Transaction transaction)
    {
        switch (transaction.Type)
        {
            case TransactionType.Income:
                await _accountService.UpdateAccountBalanceAsync(transaction.AccountId, transaction.Amount, true);
                break;

            case TransactionType.Expense:
                await _accountService.UpdateAccountBalanceAsync(transaction.AccountId, transaction.Amount, false);
                break;

            case TransactionType.Transfer:
                await _accountService.UpdateAccountBalanceAsync(transaction.AccountId, transaction.Amount, false);
                if (transaction.DestinationAccountId.HasValue)
                {
                    await _accountService.UpdateAccountBalanceAsync(transaction.DestinationAccountId.Value, transaction.Amount, true);
                }
                break;
        }
    }

    private async Task ReverseTransactionEffect(Transaction transaction)
    {
        switch (transaction.Type)
        {
            case TransactionType.Income:
                await _accountService.UpdateAccountBalanceAsync(transaction.AccountId, transaction.Amount, false);
                break;

            case TransactionType.Expense:
                await _accountService.UpdateAccountBalanceAsync(transaction.AccountId, transaction.Amount, true);
                break;

            case TransactionType.Transfer:
                await _accountService.UpdateAccountBalanceAsync(transaction.AccountId, transaction.Amount, true);
                if (transaction.DestinationAccountId.HasValue)
                {
                    await _accountService.UpdateAccountBalanceAsync(transaction.DestinationAccountId.Value, transaction.Amount, false);
                }
                break;
        }
    }
}
