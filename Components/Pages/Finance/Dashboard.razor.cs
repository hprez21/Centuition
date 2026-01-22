using CentuitionApp.Data;
using CentuitionApp.Services;
using Microsoft.Extensions.AI;
using System.Globalization;
using Telerik.Blazor;
using Telerik.Blazor.Components;

namespace CentuitionApp.Components.Pages.Finance
{
    public partial class Dashboard
    {
        private bool isLoading = true;
        private string? userId;

        private bool showResetConfirmation;
        private bool isResetting;

        private string selectedPeriod = "month";
        private DateTime selectedDate = DateTime.Today;
        private int selectedYear = DateTime.Today.Year;
        private DateTime? periodStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        private DateTime? periodEndDate = DateTime.Today;

        private decimal totalBalance;
        private decimal periodIncome;
        private decimal periodExpenses;
        private decimal periodNetSavings;

        private List<Data.Account> accounts = new();
        private List<Transaction> recentTransactions = new();
        private List<CategorySpending> categorySpending = new();
        private List<TransactionSummary> monthlyTrends = new();
        private List<BudgetProgress> budgetProgress = new();
        private List<Category> categories = new();

        private bool showQuickAddDialog;
        private Transaction newTransaction = new();
        private int? destinationAccountId;
        private TelerikNotification? notificationRef;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await LoadDashboardData();
            }

            isLoading = false;
        }

        private (DateTime start, DateTime end) GetPeriodDates()
        {
            return selectedPeriod switch
            {
                "month" => (
                    new DateTime(selectedDate.Year, selectedDate.Month, 1),
                    new DateTime(selectedDate.Year, selectedDate.Month, 1).AddMonths(1).AddDays(-1)
                ),
                "year" => (
                    new DateTime(selectedYear, 1, 1),
                    new DateTime(selectedYear, 12, 31)
                ),
                "custom" => (
                    periodStartDate ?? DateTime.Today.AddMonths(-1),
                    periodEndDate ?? DateTime.Today
                ),
                _ => (DateTime.Today.AddMonths(-1), DateTime.Today)
            };
        }

        private string GetPeriodLabel()
        {
            return selectedPeriod switch
            {
                "month" => selectedDate.ToString("MMMM yyyy"),
                "year" => $"Year {selectedYear}",
                "custom" => "Selected period",
                _ => "This month"
            };
        }

        private async Task ChangePeriod(string period)
        {
            selectedPeriod = period;
            await LoadDashboardData();
        }

        private async Task OnDateChanged(object value)
        {
            await LoadDashboardData();
        }

        private async Task OnYearChanged(object value)
        {
            await LoadDashboardData();
        }

        private async Task OnCustomRangeChanged(object value)
        {
            if (periodStartDate.HasValue && periodEndDate.HasValue)
            {
                await LoadDashboardData();
            }
        }

        private async Task LoadDashboardData()
        {
            if (string.IsNullOrEmpty(userId)) return;

            var (startDate, endDate) = GetPeriodDates();

            accounts = await AccountService.GetAllAccountsAsync(userId);
            totalBalance = await AccountService.GetTotalBalanceAsync(userId);
            periodIncome = await TransactionService.GetTotalIncomeAsync(userId, startDate, endDate);
            periodExpenses = await TransactionService.GetTotalExpensesAsync(userId, startDate, endDate);
            recentTransactions = await TransactionService.GetRecentTransactionsAsync(userId, 10);
            categorySpending = await CategoryService.GetCategorySpendingAsync(userId, startDate, endDate);
            monthlyTrends = await TransactionService.GetMonthlyTrendsAsync(userId, 6);
            budgetProgress = await BudgetService.GetBudgetProgressAsync(userId, DateTime.Today.Year, DateTime.Today.Month);
            categories = await CategoryService.GetAllCategoriesAsync(userId);

            periodNetSavings = periodIncome - periodExpenses;
        }

        private void OpenQuickAddTransaction()
        {
            newTransaction = new Transaction
            {
                Date = DateTime.Today,
                Type = TransactionType.Expense,
                UserId = userId ?? string.Empty,
                AccountId = accounts.FirstOrDefault()?.Id ?? 0
            };
            destinationAccountId = null;
            showQuickAddDialog = true;
        }

        private void CloseQuickAddDialog()
        {
            showQuickAddDialog = false;
        }

        private List<Category> GetCategoriesForType(TransactionType type)
        {
            var categoryType = type == TransactionType.Income ? CategoryType.Income : CategoryType.Expense;
            return categories.Where(c => c.Type == categoryType).ToList();
        }

        private bool IsQuickTransactionValid()
        {
            if (newTransaction.Amount <= 0) return false;
            if (string.IsNullOrWhiteSpace(newTransaction.Description)) return false;
            if (newTransaction.AccountId <= 0) return false;
            if (newTransaction.Type == TransactionType.Transfer && (!destinationAccountId.HasValue || destinationAccountId.Value <= 0)) return false;
            return true;
        }

        private async Task SaveQuickTransaction()
        {
            if (string.IsNullOrEmpty(userId)) return;

            if (!IsQuickTransactionValid())
            {
                notificationRef?.Show(new NotificationModel
                {
                    Text = GetQuickTransactionValidationError() ?? "Please complete all required fields.",
                    ThemeColor = ThemeConstants.Notification.ThemeColor.Warning,
                    CloseAfter = 4000
                });
                return;
            }

            newTransaction.UserId = userId;
            if (newTransaction.Type == TransactionType.Transfer)
            {
                newTransaction.DestinationAccountId = destinationAccountId;
                newTransaction.CategoryId = null;
            }

            await TransactionService.CreateTransactionAsync(newTransaction);

            showQuickAddDialog = false;

            notificationRef?.Show(new NotificationModel
            {
                Text = "Transaction added successfully!",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                CloseAfter = 3000
            });

            await LoadDashboardData();
        }

        private string? GetQuickTransactionValidationError()
        {
            if (newTransaction.Amount <= 0) return "Please enter an amount greater than $0.00.";
            if (string.IsNullOrWhiteSpace(newTransaction.Description)) return "Please enter a description.";
            if (newTransaction.AccountId <= 0) return "Please select an account.";
            if (newTransaction.Type == TransactionType.Transfer && (!destinationAccountId.HasValue || destinationAccountId.Value <= 0))
                return "Please select a destination account for the transfer.";
            return null;
        }

        private void OpenResetDialog()
        {
            showResetConfirmation = true;
        }

        private void CancelReset()
        {
            showResetConfirmation = false;
        }

        private async Task ConfirmReset()
        {
            if (string.IsNullOrEmpty(userId))
            {
                showResetConfirmation = false;
                return;
            }

            isResetting = true;
            StateHasChanged();

            try
            {
                await SeedData.ResetAndSeedTestDataAsync(DbContext, userId);

                await LoadDashboardData();

                notificationRef?.Show(new NotificationModel
                {
                    Text = "Test data has been reset successfully! Budgets and transactions for all 12 months of 2025 have been created.",
                    ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                    CloseAfter = 5000
                });
            }
            catch (Exception ex)
            {
                notificationRef?.Show(new NotificationModel
                {
                    Text = $"Error resetting data: {ex.Message}",
                    ThemeColor = ThemeConstants.Notification.ThemeColor.Error,
                    CloseAfter = 5000
                });
            }
            finally
            {
                isResetting = false;
                showResetConfirmation = false;
            }
        }

        private Task OnQuickAddChanged(object? _)
        {
            return InvokeAsync(StateHasChanged);
        }
    }
}
