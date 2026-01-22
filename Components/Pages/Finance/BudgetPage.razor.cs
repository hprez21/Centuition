using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using CentuitionApp.Data;
using CentuitionApp.Services;
using static CentuitionApp.Components.Pages.Finance.BudgetPageComponents.BudgetPageHeader;

namespace CentuitionApp.Components.Pages.Finance;

public partial class BudgetPage : ComponentBase
{
    [Inject] private IBudgetService BudgetService { get; set; } = default!;
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    private string? userId;
    private List<Budget> budgets = new();
    private List<BudgetProgress> budgetProgress = new();
    private List<Category> expenseCategories = new();
    private List<Category> availableCategories = new();

    private int selectedMonth = DateTime.Today.Month;
    private int selectedYear = DateTime.Today.Year;

    private decimal totalBudget;
    private decimal totalSpent;
    private decimal totalRemaining;
    private decimal overallPercentage;

    private bool showDialog;
    private bool isEditing;
    private Budget editBudget = new();

    private List<MonthOption> months = Enumerable.Range(1, 12)
        .Select(m => new MonthOption { Value = m, Name = new DateTime(2000, m, 1).ToString("MMMM") })
        .ToList();

    private TelerikNotification? notificationRef;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await LoadCategories();
            await LoadBudgetData();
        }
    }

    private async Task LoadCategories()
    {
        if (string.IsNullOrEmpty(userId)) return;

        var categories = await CategoryService.GetAllCategoriesAsync(userId);
        expenseCategories = categories.Where(c => c.Type == CategoryType.Expense).ToList();
    }

    private async Task LoadBudgetData()
    {
        if (string.IsNullOrEmpty(userId)) return;

        budgets = await BudgetService.GetBudgetsAsync(userId, selectedYear, selectedMonth);
        budgetProgress = await BudgetService.GetBudgetProgressAsync(userId, selectedYear, selectedMonth);

        totalBudget = budgets.Sum(b => b.Amount);
        totalSpent = budgets.Sum(b => b.SpentAmount);
        totalRemaining = totalBudget - totalSpent;
        overallPercentage = totalBudget > 0 ? (totalSpent / totalBudget) * 100 : 0;

        var budgetedCategoryIds = budgets.Select(b => b.CategoryId).ToHashSet();
        availableCategories = expenseCategories.Where(c => !budgetedCategoryIds.Contains(c.Id)).ToList();
    }

    private async Task OnMonthChanged(int value)
    {
        selectedMonth = value;
        await LoadBudgetData();
    }

    private async Task OnYearChanged(int value)
    {
        selectedYear = value;
        await LoadBudgetData();
    }

    private void OnCategoryChanged(int categoryId)
    {
        var category = availableCategories.FirstOrDefault(c => c.Id == categoryId)
                       ?? expenseCategories.FirstOrDefault(c => c.Id == categoryId);
        editBudget.Name = category?.Name ?? "Budget";
    }

    private void OpenCreateDialog()
    {
        if (!availableCategories.Any())
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = "All expense categories already have budgets for this month.",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Warning,
                CloseAfter = 3000
            });
            return;
        }

        isEditing = false;
        var firstCategory = availableCategories.First();
        editBudget = new Budget
        {
            UserId = userId ?? string.Empty,
            Month = selectedMonth,
            Year = selectedYear,
            IsActive = true,
            CategoryId = firstCategory.Id,
            Name = firstCategory.Name
        };
        showDialog = true;
    }

    private async Task EditBudgetHandler(GridCommandEventArgs args)
    {
        var budget = (Budget)args.Item;

        isEditing = true;
        editBudget = new Budget
        {
            Id = budget.Id,
            Name = budget.Name,
            Amount = budget.Amount,
            CategoryId = budget.CategoryId,
            Month = budget.Month,
            Year = budget.Year,
            IsActive = budget.IsActive,
            UserId = budget.UserId
        };
        showDialog = true;

        await Task.CompletedTask;
    }

    private async Task DeleteBudget(GridCommandEventArgs args)
    {
        var budget = (Budget)args.Item;

        if (string.IsNullOrEmpty(userId)) return;

        var result = await BudgetService.DeleteBudgetAsync(budget.Id, userId);

        if (result)
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = "Budget deleted successfully!",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                CloseAfter = 3000
            });
            await LoadBudgetData();
        }
        else
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = "Failed to delete budget.",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Error,
                CloseAfter = 3000
            });
        }
    }

    private void CloseDialog()
    {
        showDialog = false;
    }

    private async Task SaveBudget()
    {
        if (string.IsNullOrEmpty(userId)) return;

        editBudget.UserId = userId;

        var category = expenseCategories.FirstOrDefault(c => c.Id == editBudget.CategoryId);
        editBudget.Name = category?.Name ?? "Budget";

        try
        {
            if (isEditing)
            {
                await BudgetService.UpdateBudgetAsync(editBudget);
                notificationRef?.Show(new NotificationModel
                {
                    Text = "Budget updated successfully!",
                    ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                    CloseAfter = 3000
                });
            }
            else
            {
                await BudgetService.CreateBudgetAsync(editBudget);
                notificationRef?.Show(new NotificationModel
                {
                    Text = "Budget created successfully!",
                    ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                    CloseAfter = 3000
                });
            }

            showDialog = false;
            await LoadBudgetData();
        }
        catch (Exception ex)
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = $"Error: {ex.Message}",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Error,
                CloseAfter = 5000
            });
        }
    }

    private async Task CopyToNextMonth()
    {
        if (string.IsNullOrEmpty(userId) || !budgets.Any())
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = "No budgets to copy.",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Warning,
                CloseAfter = 3000
            });
            return;
        }

        await BudgetService.CopyBudgetsToNextMonthAsync(userId, selectedYear, selectedMonth);

        notificationRef?.Show(new NotificationModel
        {
            Text = "Budgets copied to next month!",
            ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
            CloseAfter = 3000
        });
    }
}
