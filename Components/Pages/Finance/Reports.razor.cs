using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using CentuitionApp.Data;
using CentuitionApp.Services;
using CentuitionApp.Components.Pages.Finance.ReportsComponents;
using Telerik.Blazor;
using Telerik.Blazor.Components;

namespace CentuitionApp.Components.Pages.Finance;

public partial class Reports
{
    [Inject] private ITransactionService TransactionService { get; set; } = default!;
    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    private string? userId;
    private bool isLoading = true;

    private DateTime? reportStartDate;
    private DateTime? reportEndDate;

    private List<Transaction> allTransactions = new();
    private List<Transaction> incomeTransactions = new();
    private List<Transaction> expenseTransactions = new();
    private List<IncomeVsExpensesTrendChart.TrendDataPoint> trendData = new();
    private List<CategorySpending> categorySpending = new();
    private List<Data.Account> accounts = new();

    private decimal totalIncome;
    private decimal totalExpenses;
    private decimal netSavings;
    private decimal savingsRate;
    private decimal avgDailyExpense;
    private int dayCount;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            SetDateRange(QuickDateFilters.DateRangeType.ThisMonth);
            await LoadReportData();
        }

        isLoading = false;
    }

    private void SetDateRange(QuickDateFilters.DateRangeType rangeType)
    {
        var today = DateTime.Today;

        switch (rangeType)
        {
            case QuickDateFilters.DateRangeType.ThisMonth:
                reportStartDate = new DateTime(today.Year, today.Month, 1);
                reportEndDate = reportStartDate.Value.AddMonths(1).AddDays(-1);
                break;
            case QuickDateFilters.DateRangeType.LastMonth:
                reportStartDate = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                reportEndDate = reportStartDate.Value.AddMonths(1).AddDays(-1);
                break;
            case QuickDateFilters.DateRangeType.Last3Months:
                reportStartDate = new DateTime(today.Year, today.Month, 1).AddMonths(-2);
                reportEndDate = today;
                break;
            case QuickDateFilters.DateRangeType.Last6Months:
                reportStartDate = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
                reportEndDate = today;
                break;
            case QuickDateFilters.DateRangeType.ThisYear:
                reportStartDate = new DateTime(today.Year, 1, 1);
                reportEndDate = new DateTime(today.Year, 12, 31);
                break;
            case QuickDateFilters.DateRangeType.LastYear:
                reportStartDate = new DateTime(today.Year - 1, 1, 1);
                reportEndDate = new DateTime(today.Year - 1, 12, 31);
                break;
        }
    }

    private async Task LoadReportData()
    {
        if (string.IsNullOrEmpty(userId) || !reportStartDate.HasValue || !reportEndDate.HasValue) return;

        isLoading = true;
        StateHasChanged();

        allTransactions = await TransactionService.GetTransactionsAsync(userId, reportStartDate, reportEndDate);
        accounts = await AccountService.GetAllAccountsAsync(userId);
        categorySpending = await CategoryService.GetCategorySpendingAsync(userId, reportStartDate, reportEndDate);

        trendData = GenerateTrendData(allTransactions, reportStartDate.Value, reportEndDate.Value);

        incomeTransactions = allTransactions.Where(t => t.Type == TransactionType.Income).ToList();
        expenseTransactions = allTransactions.Where(t => t.Type == TransactionType.Expense).ToList();

        totalIncome = incomeTransactions.Sum(t => t.Amount);
        totalExpenses = expenseTransactions.Sum(t => t.Amount);
        netSavings = totalIncome - totalExpenses;
        savingsRate = totalIncome > 0 ? (netSavings / totalIncome) * 100 : 0;

        dayCount = (reportEndDate.Value - reportStartDate.Value).Days + 1;
        avgDailyExpense = dayCount > 0 ? totalExpenses / dayCount : 0;

        isLoading = false;
        StateHasChanged();
    }

    private List<IncomeVsExpensesTrendChart.TrendDataPoint> GenerateTrendData(List<Transaction> transactions, DateTime startDate, DateTime endDate)
    {
        var result = new List<IncomeVsExpensesTrendChart.TrendDataPoint>();
        var nonTransferTransactions = transactions.Where(t => t.Type != TransactionType.Transfer).ToList();

        var startMonth = new DateTime(startDate.Year, startDate.Month, 1);
        var endMonth = new DateTime(endDate.Year, endDate.Month, 1);
        var monthCount = ((endMonth.Year - startMonth.Year) * 12) + endMonth.Month - startMonth.Month + 1;

        if (monthCount <= 1)
        {
            var weekStart = startDate;
            var weekNumber = 1;

            while (weekStart <= endDate)
            {
                var weekEnd = weekStart.AddDays(6);
                if (weekEnd > endDate) weekEnd = endDate;

                var weekTransactions = nonTransferTransactions
                    .Where(t => t.Date >= weekStart && t.Date <= weekEnd)
                    .ToList();

                result.Add(new IncomeVsExpensesTrendChart.TrendDataPoint
                {
                    Label = $"Week {weekNumber}",
                    TotalIncome = weekTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    TotalExpenses = weekTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                });

                weekStart = weekEnd.AddDays(1);
                weekNumber++;
            }
        }
        else if (monthCount <= 3)
        {
            var weekStart = startDate;

            while (weekStart <= endDate)
            {
                var weekEnd = weekStart.AddDays(6);
                if (weekEnd > endDate) weekEnd = endDate;

                var weekTransactions = nonTransferTransactions
                    .Where(t => t.Date >= weekStart && t.Date <= weekEnd)
                    .ToList();

                result.Add(new IncomeVsExpensesTrendChart.TrendDataPoint
                {
                    Label = $"{weekStart:MMM d}",
                    TotalIncome = weekTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    TotalExpenses = weekTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                });

                weekStart = weekEnd.AddDays(1);
            }
        }
        else
        {
            var currentMonth = startMonth;

            while (currentMonth <= endMonth)
            {
                var monthTransactions = nonTransferTransactions
                    .Where(t => t.Date.Year == currentMonth.Year && t.Date.Month == currentMonth.Month)
                    .ToList();

                result.Add(new IncomeVsExpensesTrendChart.TrendDataPoint
                {
                    Label = currentMonth.ToString("MMM yyyy"),
                    TotalIncome = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    TotalExpenses = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                });

                currentMonth = currentMonth.AddMonths(1);
            }
        }

        return result;
    }

    private string GetTypeBadgeColor(TransactionType type) => type switch
    {
        TransactionType.Income => ThemeConstants.Badge.ThemeColor.Success,
        TransactionType.Expense => ThemeConstants.Badge.ThemeColor.Error,
        TransactionType.Transfer => ThemeConstants.Badge.ThemeColor.Info,
        _ => ThemeConstants.Badge.ThemeColor.Light
    };
}
