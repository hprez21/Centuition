using CentuitionApp.Data;

namespace CentuitionApp.Services;

/// <summary>
/// Service interface for managing recurring transactions
/// </summary>
public interface IRecurringTransactionService
{
    Task<List<RecurringTransaction>> GetRecurringTransactionsAsync(string userId);
    Task<RecurringTransaction?> GetRecurringTransactionByIdAsync(int id, string userId);
    Task<RecurringTransaction> CreateRecurringTransactionAsync(RecurringTransaction recurring);
    Task<RecurringTransaction> UpdateRecurringTransactionAsync(RecurringTransaction recurring);
    Task<bool> DeleteRecurringTransactionAsync(int id, string userId);
    Task<List<RecurringTransaction>> GetDueRecurringTransactionsAsync(string userId);
    Task ProcessDueRecurringTransactionsAsync(string userId);
    Task<DateTime> CalculateNextDueDateAsync(RecurringTransaction recurring);
}
