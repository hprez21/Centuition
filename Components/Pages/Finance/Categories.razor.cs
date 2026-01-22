using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using CentuitionApp.Data;
using CentuitionApp.Services;

namespace CentuitionApp.Components.Pages.Finance;

public partial class Categories : ComponentBase
{
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    private string? userId;
    private List<Category> categories = new();
    private List<Category> expenseCategories = new();
    private List<Category> incomeCategories = new();
    private List<CategorySpending> expenseCategorySpending = new();
    private List<CategorySpending> incomeCategorySpending = new();
    private int activeTabIndex = 0;

    private bool showDialog;
    private bool isEditing;
    private Category editCategory = new();
    private string categoryColor = "#6c757d";

    private List<CategoryType> categoryTypes = Enum.GetValues<CategoryType>().ToList();

    private TelerikNotification? notificationRef;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await LoadCategories();
        }
    }

    private async Task LoadCategories()
    {
        if (string.IsNullOrEmpty(userId)) return;

        categories = await CategoryService.GetAllCategoriesAsync(userId);
        expenseCategories = categories.Where(c => c.Type == CategoryType.Expense).ToList();
        incomeCategories = categories.Where(c => c.Type == CategoryType.Income).ToList();

        var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        expenseCategorySpending = await CategoryService.GetCategorySpendingAsync(userId, startOfMonth, endOfMonth);
        incomeCategorySpending = await CategoryService.GetIncomeByCategory(userId, startOfMonth, endOfMonth);
    }

    private void OpenCreateDialog()
    {
        isEditing = false;
        editCategory = new Category
        {
            UserId = userId,
            Type = activeTabIndex == 0 ? CategoryType.Expense : CategoryType.Income,
            SortOrder = 0
        };
        categoryColor = "#6c757d";
        showDialog = true;
    }

    private async Task EditCategoryHandler(GridCommandEventArgs args)
    {
        var category = (Category)args.Item;

        isEditing = true;
        editCategory = new Category
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Type = category.Type,
            Color = category.Color,
            Icon = category.Icon,
            SortOrder = category.SortOrder,
            IsSystem = category.IsSystem,
            UserId = category.UserId
        };
        categoryColor = category.Color;
        showDialog = true;

        await Task.CompletedTask;
    }

    private async Task DeleteCategory(GridCommandEventArgs args)
    {
        var category = (Category)args.Item;

        if (category.IsSystem)
        {
            args.IsCancelled = true;
            notificationRef?.Show(new NotificationModel
            {
                Text = "System categories cannot be deleted.",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Warning,
                CloseAfter = 3000
            });
            return;
        }

        if (string.IsNullOrEmpty(userId)) return;

        var result = await CategoryService.DeleteCategoryAsync(category.Id, userId);

        if (result)
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = "Category deleted successfully!",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                CloseAfter = 3000
            });
            await LoadCategories();
        }
        else
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = "Failed to delete category.",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Error,
                CloseAfter = 3000
            });
        }
    }

    private void CloseDialog()
    {
        showDialog = false;
    }

    private async Task SaveCategory()
    {
        editCategory.Color = categoryColor;

        try
        {
            if (isEditing)
            {
                await CategoryService.UpdateCategoryAsync(editCategory);
                notificationRef?.Show(new NotificationModel
                {
                    Text = "Category updated successfully!",
                    ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                    CloseAfter = 3000
                });
            }
            else
            {
                editCategory.UserId = userId;
                await CategoryService.CreateCategoryAsync(editCategory);
                notificationRef?.Show(new NotificationModel
                {
                    Text = "Category created successfully!",
                    ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                    CloseAfter = 3000
                });
            }

            showDialog = false;
            await LoadCategories();
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
}
