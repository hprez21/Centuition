using CentuitionApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CentuitionApp.Services;

/// <summary>
/// Service implementation for managing recurring transactions
/// </summary>
public class RecurringTransactionService : IRecurringTransactionService
{
    private readonly ApplicationDbContext _context;
    private readonly ITransactionService _transactionService;

    public RecurringTransactionService(ApplicationDbContext context, ITransactionService transactionService)
    {
        _context = context;
        _transactionService = transactionService;
    }

    public async Task<List<RecurringTransaction>> GetRecurringTransactionsAsync(string userId)
    {
        return await _context.RecurringTransactions
            .Include(r => r.Account)
            .Include(r => r.Category)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.NextDueDate)
            .ToListAsync();
    }

    public async Task<RecurringTransaction?> GetRecurringTransactionByIdAsync(int id, string userId)
    {
        return await _context.RecurringTransactions
            .Include(r => r.Account)
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    }

    public async Task<RecurringTransaction> CreateRecurringTransactionAsync(RecurringTransaction recurring)
    {
        recurring.CreatedAt = DateTime.UtcNow;
        recurring.NextDueDate = recurring.StartDate;

        _context.RecurringTransactions.Add(recurring);
        await _context.SaveChangesAsync();

        return recurring;
    }

    public async Task<RecurringTransaction> UpdateRecurringTransactionAsync(RecurringTransaction recurring)
    {
        var existing = await _context.RecurringTransactions
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

        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteRecurringTransactionAsync(int id, string userId)
    {
        var recurring = await _context.RecurringTransactions
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (recurring == null)
        {
            return false;
        }

        _context.RecurringTransactions.Remove(recurring);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<RecurringTransaction>> GetDueRecurringTransactionsAsync(string userId)
    {
        var today = DateTime.Today;

        return await _context.RecurringTransactions
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
        var dueTransactions = await GetDueRecurringTransactionsAsync(userId);

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

        await _context.SaveChangesAsync();
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
