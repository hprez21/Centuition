using CentuitionApp.Data;

namespace CentuitionApp.Services;

/// <summary>
/// Service interface for managing financial transactions
/// </summary>
public interface ITransactionService
{
    Task<List<Transaction>> GetTransactionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, int? categoryId = null, int? accountId = null, TransactionType? type = null);
    Task<Transaction?> GetTransactionByIdAsync(int id, string userId);
    Task<Transaction> CreateTransactionAsync(Transaction transaction);
    Task<Transaction> UpdateTransactionAsync(Transaction transaction);
    Task<bool> DeleteTransactionAsync(int id, string userId);
    Task<decimal> GetTotalIncomeAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetTotalExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<int, decimal>> GetExpensesByCategoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TransactionSummary>> GetMonthlyTrendsAsync(string userId, int months = 12);
    Task<List<Transaction>> GetRecentTransactionsAsync(string userId, int count = 10);
}

/// <summary>
/// Summary model for transaction aggregations
/// </summary>
public class TransactionSummary
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetAmount => TotalIncome - TotalExpenses;
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
}
