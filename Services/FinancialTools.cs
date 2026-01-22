using System.ComponentModel;
using System.Globalization;
using CentuitionApp.Data;

namespace CentuitionApp.Services;

/// <summary>
/// Provides AI-callable tools for financial data retrieval.
/// These tools are designed to be used with Microsoft.Extensions.AI's function calling capabilities.
/// </summary>
public class FinancialTools
{
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;
    private readonly IBudgetService _budgetService;
    private readonly ICategoryService _categoryService;
    private string? _userId;
    private static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");

    public FinancialTools(
        IAccountService accountService,
        ITransactionService transactionService,
        IBudgetService budgetService,
        ICategoryService categoryService)
    {
        _accountService = accountService;
        _transactionService = transactionService;
        _budgetService = budgetService;
        _categoryService = categoryService;
    }

    /// <summary>
    /// Sets the current user ID for all tool operations.
    /// Must be called before using any tools.
    /// </summary>
    public void SetUserId(string userId) => _userId = userId;

    [Description("Gets all financial accounts for the user with their names, types, and current balances. Use this to answer questions about the user's accounts or where their money is held.")]
    public async Task<string> GetAccounts()
    {
        if (string.IsNullOrEmpty(_userId)) return "Error: User not authenticated.";
        
        var accounts = await _accountService.GetAllAccountsAsync(_userId);
        if (!accounts.Any()) return "No accounts found.";
        
        var result = "User's accounts:\n" + string.Join("\n", accounts.Select(a =>
            $"- {a.Name} ({a.AccountType}): {a.CurrentBalance.ToString("C2", UsCulture)}"));
        
        return result;
    }

    [Description("Gets the total balance across all the user's accounts. Use this to answer questions about total net worth or overall financial position.")]
    public async Task<string> GetTotalBalance()
    {
        if (string.IsNullOrEmpty(_userId)) return "Error: User not authenticated.";
        
        var balance = await _accountService.GetTotalBalanceAsync(_userId);
        return $"Total balance across all accounts: {balance.ToString("C2", UsCulture)}";
    }

    [Description("Gets balances grouped by account type (Checking, Savings, Credit Card, Investment, etc.). Use this to understand the distribution of funds across different account types.")]
    public async Task<string> GetBalancesByAccountType()
    {
        if (string.IsNullOrEmpty(_userId)) return "Error: User not authenticated.";
        
        var balances = await _accountService.GetBalancesByTypeAsync(_userId);
        if (!balances.Any()) return "No account balances found.";
        
        var result = "Balances by account type:\n" + string.Join("\n", balances.Select(kvp =>
            $"- {kvp.Key}: {kvp.Value.ToString("C2", UsCulture)}"));
        
        return result;
    }

    [Description("Gets recent transactions. Use this to show the user their latest financial activity. Parameter 'count' specifies how many transactions to retrieve (default: 10, max: 50).")]
    public async Task<string> GetRecentTransactions(int count = 10)
    {
        if (string.IsNullOrEmpty(_userId)) return "Error: User not authenticated.";
        
        count = Math.Min(Math.Max(count, 1), 50); // Clamp between 1 and 50
        var transactions = await _transactionService.GetRecentTransactionsAsync(_userId, count);
        
        if (!transactions.Any()) return "No recent transactions found.";
        
        var result = $"Last {transactions.Count} transactions:\n" + string.Join("\n", transactions.Select(t =>
        {
            var sign = t.Type == TransactionType.Income ? "+" : t.Type == TransactionType.Expense ? "-" : "↔";
            var category = t.Category?.Name ?? "Uncategorized";
            return $"- {t.Date:MMM dd, yyyy}: {sign}{t.Amount.ToString("C2", UsCulture)} - {t.Description} [{category}]";
        }));
        
        return result;
    }

    [Description("Gets spending breakdown by category for the current month. Use this to answer questions about where money is being spent, top expense categories, or spending patterns.")]
    public async Task<string> GetCategorySpending()
    {
        if (string.IsNullOrEmpty(_userId)) return "Error: User not authenticated.";
        
        var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var endDate = DateTime.Today;
        
        var spending = await _categoryService.GetCategorySpendingAsync(_userId, startDate, endDate);
        
        if (!spending.Any()) return "No spending data found for this month.";
        
        var totalSpent = spending.Sum(s => s.TotalAmount);
        var result = $"Spending by category for {DateTime.Today:MMMM yyyy}:\n";
        result += string.Join("\n", spending.OrderByDescending(s => s.TotalAmount).Select(s =>
            $"- {s.CategoryName}: {s.TotalAmount.ToString("C2", UsCulture)} ({s.TransactionCount} transactions, {s.Percentage:F1}%)"));
        result += $"\n\nTotal spent: {totalSpent.ToString("C2", UsCulture)}";
        
        return result;
    }

    [Description("Gets budget status and progress for the current month. Shows each budget category, amount budgeted, amount spent, and whether the user is on track, over budget, or close to the limit.")]
    public async Task<string> GetBudgetStatus()
    {
        if (string.IsNullOrEmpty(_userId)) return "Error: User not authenticated.";
        
        var progress = await _budgetService.GetBudgetProgressAsync(_userId,
            DateTime.Today.Year, DateTime.Today.Month);
        
        if (!progress.Any()) return "No budgets set up for this month.";
        
        var result = $"Budget status for {DateTime.Today:MMMM yyyy}:\n";
        result += string.Join("\n", progress.Select(b =>
        {
            var statusEmoji = b.IsOverBudget ? "🔴" : b.PercentageUsed >= 80 ? "🟡" : "🟢";
            return $"{statusEmoji} {b.CategoryName}: {b.SpentAmount.ToString("C2", UsCulture)} of {b.BudgetAmount.ToString("C2", UsCulture)} ({b.PercentageUsed:F0}% used) - {b.Status}";
        }));
        
        var overBudgetCount = progress.Count(b => b.IsOverBudget);
        var warningCount = progress.Count(b => !b.IsOverBudget && b.PercentageUsed >= 80);
        
        result += $"\n\nSummary: {overBudgetCount} over budget, {warningCount} approaching limit, {progress.Count - overBudgetCount - warningCount} on track.";
        
        return result;
    }

    [Description("Gets monthly income and expense trends over the past several months. Use this to show financial trends, compare months, or analyze spending/income patterns over time. Parameter 'months' specifies how many months of history to include (default: 6).")]
    public async Task<string> GetMonthlyTrends(int months = 6)
    {
        if (string.IsNullOrEmpty(_userId)) return "Error: User not authenticated.";
        
        months = Math.Min(Math.Max(months, 1), 12); // Clamp between 1 and 12
        var trends = await _transactionService.GetMonthlyTrendsAsync(_userId, months);
        
        if (!trends.Any()) return "No transaction history found.";
        
        var result = $"Monthly trends (last {trends.Count} months):\n";
        result += string.Join("\n", trends.Select(t =>
            $"- {t.MonthName}: Income {t.TotalIncome.ToString("C2", UsCulture)}, Expenses {t.TotalExpenses.ToString("C2", UsCulture)}, Net {t.NetAmount.ToString("C2", UsCulture)}"));
        
        var avgIncome = trends.Average(t => t.TotalIncome);
        var avgExpenses = trends.Average(t => t.TotalExpenses);
        result += $"\n\nAverages: Income {avgIncome.ToString("C2", UsCulture)}/month, Expenses {avgExpenses.ToString("C2", UsCulture)}/month";
        
        return result;
    }

    [Description("Gets total income for a specified date range. Use this to answer questions about how much money the user has earned. If no dates are provided, returns income for the current month.")]
    public async Task<string> GetTotalIncome(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (string.IsNullOrEmpty(_userId)) return "Error: User not authenticated.";
        
        startDate ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        endDate ??= DateTime.Today;
        
        var income = await _transactionService.GetTotalIncomeAsync(_userId, startDate, endDate);
        return $"Total income from {startDate:MMM dd, yyyy} to {endDate:MMM dd, yyyy}: {income.ToString("C2", UsCulture)}";
    }

    [Description("Gets total expenses for a specified date range. Use this to answer questions about how much money the user has spent. If no dates are provided, returns expenses for the current month.")]
    public async Task<string> GetTotalExpenses(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (string.IsNullOrEmpty(_userId)) return "Error: User not authenticated.";
        
        startDate ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        endDate ??= DateTime.Today;
        
        var expenses = await _transactionService.GetTotalExpensesAsync(_userId, startDate, endDate);
        return $"Total expenses from {startDate:MMM dd, yyyy} to {endDate:MMM dd, yyyy}: {expenses.ToString("C2", UsCulture)}";
    }

    [Description("Gets net savings (income minus expenses) for the current month. Use this to answer questions about how much money the user has saved or their monthly cash flow.")]
    public async Task<string> GetNetSavings()
    {
        if (string.IsNullOrEmpty(_userId)) return "Error: User not authenticated.";
        
        var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var endDate = DateTime.Today;
        
        var income = await _transactionService.GetTotalIncomeAsync(_userId, startDate, endDate);
        var expenses = await _transactionService.GetTotalExpensesAsync(_userId, startDate, endDate);
        var netSavings = income - expenses;
        
        var status = netSavings >= 0 ? "saved" : "overspent";
        return $"For {DateTime.Today:MMMM yyyy}:\n" +
               $"- Income: {income.ToString("C2", UsCulture)}\n" +
               $"- Expenses: {expenses.ToString("C2", UsCulture)}\n" +
               $"- Net: {netSavings.ToString("C2", UsCulture)} ({status})";
    }

    [Description("Gets the top expense categories ranked by spending amount. Use this to identify where most money is going. Parameter 'count' specifies how many top categories to return (default: 5).")]
    public async Task<string> GetTopExpenseCategories(int count = 5)
    {
        if (string.IsNullOrEmpty(_userId)) return "Error: User not authenticated.";
        
        count = Math.Min(Math.Max(count, 1), 20);
        var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var endDate = DateTime.Today;
        
        var spending = await _categoryService.GetCategorySpendingAsync(_userId, startDate, endDate);
        var topCategories = spending.OrderByDescending(s => s.TotalAmount).Take(count).ToList();
        
        if (!topCategories.Any()) return "No spending data found.";
        
        var result = $"Top {topCategories.Count} expense categories for {DateTime.Today:MMMM yyyy}:\n";
        for (int i = 0; i < topCategories.Count; i++)
        {
            var cat = topCategories[i];
            result += $"{i + 1}. {cat.CategoryName}: {cat.TotalAmount.ToString("C2", UsCulture)} ({cat.Percentage:F1}%)\n";
        }
        
        return result.TrimEnd();
    }
}
