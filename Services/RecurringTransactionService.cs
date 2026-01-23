using CentuitionApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CentuitionApp.Services;

/// <summary>
/// Service implementation for managing recurring transactions
/// </summary>
public class RecurringTransactionService : IRecurringTransactionService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ITransactionService _transactionService;

    public RecurringTransactionService(IDbContextFactory<ApplicationDbContext> contextFactory, ITransactionService transactionService)
    {
        _contextFactory = contextFactory;
        _transactionService = transactionService;
    }

    public async Task<List<RecurringTransaction>> GetRecurringTransactionsAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.RecurringTransactions
            .Include(r => r.Account)
            .Include(r => r.Category)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.NextDueDate)
            .ToListAsync();
    }

    public async Task<RecurringTransaction?> GetRecurringTransactionByIdAsync(int id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.RecurringTransactions
            .Include(r => r.Account)
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    }

    public async Task<RecurringTransaction> CreateRecurringTransactionAsync(RecurringTransaction recurring)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        recurring.CreatedAt = DateTime.UtcNow;
        recurring.NextDueDate = recurring.StartDate;

        context.RecurringTransactions.Add(recurring);
        await context.SaveChangesAsync();

        return recurring;
    }

    public async Task<RecurringTransaction> UpdateRecurringTransactionAsync(RecurringTransaction recurring)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.RecurringTransactions
            .FirstOrDefaultAsync(r => r.Id == recurring.Id && r.UserId == recurring.UserId);

        if (existing == null)
        {
            throw new InvalidOperationException("Recurring transaction not found");
        }

        existing.Amount = recurring.Amount;
        existing.Type = recurring.Type;
        existing.Description = recurring.Description;
        existing.Notes = recurring.Notes;
        existing.Frequency = recurring.Frequency;
        existing.StartDate = recurring.StartDate;
        existing.EndDate = recurring.EndDate;
        existing.AccountId = recurring.AccountId;
        existing.CategoryId = recurring.CategoryId;
        existing.IsActive = recurring.IsActive;
        existing.AutoCreate = recurring.AutoCreate;
        existing.UpdatedAt = DateTime.UtcNow;

        if (existing.NextDueDate < DateTime.Today)
        {
            existing.NextDueDate = await CalculateNextDueDateAsync(existing);
        }

        await context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteRecurringTransactionAsync(int id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var recurring = await context.RecurringTransactions
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (recurring == null)
        {
            return false;
        }

        context.RecurringTransactions.Remove(recurring);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<List<RecurringTransaction>> GetDueRecurringTransactionsAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var today = DateTime.Today;

        return await context.RecurringTransactions
            .Include(r => r.Account)
            .Include(r => r.Category)
            .Where(r => r.UserId == userId
                && r.IsActive
                && r.NextDueDate.HasValue
                && r.NextDueDate.Value <= today
                && (!r.EndDate.HasValue || r.EndDate.Value >= today))
            .ToListAsync();
    }

    public async Task ProcessDueRecurringTransactionsAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var today = DateTime.Today;
        
        var dueTransactions = await context.RecurringTransactions
            .Include(r => r.Account)
            .Include(r => r.Category)
            .Where(r => r.UserId == userId
                && r.IsActive
                && r.NextDueDate.HasValue
                && r.NextDueDate.Value <= today
                && (!r.EndDate.HasValue || r.EndDate.Value >= today))
            .ToListAsync();

        foreach (var recurring in dueTransactions)
        {
            if (recurring.AutoCreate)
            {
                var transaction = new Transaction
                {
                    Amount = recurring.Amount,
                    Type = recurring.Type,
                    Description = recurring.Description,
                    Notes = recurring.Notes,
                    Date = recurring.NextDueDate ?? DateTime.Today,
                    AccountId = recurring.AccountId,
                    CategoryId = recurring.CategoryId,
                    UserId = recurring.UserId,
                    RecurringTransactionId = recurring.Id
                };

                await _transactionService.CreateTransactionAsync(transaction);
            }

            recurring.LastProcessedDate = recurring.NextDueDate;
            recurring.NextDueDate = await CalculateNextDueDateAsync(recurring);
            recurring.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    public Task<DateTime> CalculateNextDueDateAsync(RecurringTransaction recurring)
    {
        var baseDate = recurring.LastProcessedDate ?? recurring.StartDate;

        var nextDate = recurring.Frequency switch
        {
            RecurrenceFrequency.Daily => baseDate.AddDays(1),
            RecurrenceFrequency.Weekly => baseDate.AddDays(7),
            RecurrenceFrequency.BiWeekly => baseDate.AddDays(14),
            RecurrenceFrequency.Monthly => baseDate.AddMonths(1),
            RecurrenceFrequency.Quarterly => baseDate.AddMonths(3),
            RecurrenceFrequency.Yearly => baseDate.AddYears(1),
            _ => baseDate.AddMonths(1)
        };

        while (nextDate < DateTime.Today)
        {
            nextDate = recurring.Frequency switch
            {
                RecurrenceFrequency.Daily => nextDate.AddDays(1),
                RecurrenceFrequency.Weekly => nextDate.AddDays(7),
                RecurrenceFrequency.BiWeekly => nextDate.AddDays(14),
                RecurrenceFrequency.Monthly => nextDate.AddMonths(1),
                RecurrenceFrequency.Quarterly => nextDate.AddMonths(3),
                RecurrenceFrequency.Yearly => nextDate.AddYears(1),
                _ => nextDate.AddMonths(1)
            };
        }

        return Task.FromResult(nextDate);
    }
}
