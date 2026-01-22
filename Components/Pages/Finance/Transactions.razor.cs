using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using CentuitionApp.Data;
using CentuitionApp.Services;
using CentuitionApp.Components.Pages.Finance.TransactionsComponents;
using Telerik.Blazor;
using Telerik.Blazor.Components;

namespace CentuitionApp.Components.Pages.Finance;

public partial class Transactions
{
    [Inject] private ITransactionService TransactionService { get; set; } = default!;
    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private string? userId;
    private List<Transaction> transactions = new();
    private List<Data.Account> accounts = new();
    private List<Category> categories = new();
    private IEnumerable<Transaction> selectedTransactions = Enumerable.Empty<Transaction>();
    private TransactionsGrid? gridRef;

    private DateTime? filterStartDate;
    private DateTime? filterEndDate;
    private TransactionType? filterType;
    private int? filterAccountId;
    private int? filterCategoryId;

    private List<TransactionsFilterPanel.DropDownOption> transactionTypeOptions = new()
    {
        new TransactionsFilterPanel.DropDownOption { Value = null, Text = "All Types" },
        new TransactionsFilterPanel.DropDownOption { Value = TransactionType.Expense, Text = "Expense" },
        new TransactionsFilterPanel.DropDownOption { Value = TransactionType.Income, Text = "Income" },
        new TransactionsFilterPanel.DropDownOption { Value = TransactionType.Transfer, Text = "Transfer" }
    };
    private List<Data.Account> accountOptions = new();
    private List<Category> categoryOptions = new();

    private decimal filteredIncome;
    private decimal filteredExpenses;
    private decimal filteredNet;

    private bool showDialog;
    private bool isEditing;
    private Transaction editTransaction = new();
    private int? destinationAccountId;

    private TelerikNotification? notificationRef;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            filterStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            filterEndDate = filterStartDate.Value.AddMonths(1).AddDays(-1);

            await LoadReferenceData();
            await LoadTransactions();
        }
    }

    private async Task LoadReferenceData()
    {
        if (string.IsNullOrEmpty(userId)) return;

        accounts = await AccountService.GetAllAccountsAsync(userId);
        categories = await CategoryService.GetAllCategoriesAsync(userId);

        accountOptions = accounts;
        categoryOptions = categories;
    }

    private async Task LoadTransactions()
    {
        if (string.IsNullOrEmpty(userId)) return;

        transactions = await TransactionService.GetTransactionsAsync(
            userId,
            filterStartDate,
            filterEndDate,
            filterCategoryId,
            filterAccountId,
            filterType
        );

        filteredIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        filteredExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        filteredNet = filteredIncome - filteredExpenses;
    }

    private async Task ApplyFilters()
    {
        await LoadTransactions();
    }

    private async Task ClearFilters()
    {
        filterStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        filterEndDate = filterStartDate.Value.AddMonths(1).AddDays(-1);
        filterType = null;
        filterAccountId = null;
        filterCategoryId = null;
        await LoadTransactions();
    }

    private void OpenCreateDialog()
    {
        isEditing = false;
        editTransaction = new Transaction
        {
            Date = DateTime.Today,
            Type = TransactionType.Expense,
            UserId = userId ?? string.Empty,
            AccountId = accounts.FirstOrDefault()?.Id ?? 0
        };
        destinationAccountId = null;
        showDialog = true;
    }

    private async Task EditTransactionHandler(GridCommandEventArgs args)
    {
        var transaction = (Transaction)args.Item;

        isEditing = true;
        editTransaction = new Transaction
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Type = transaction.Type,
            Description = transaction.Description,
            Notes = transaction.Notes,
            Date = transaction.Date,
            AccountId = transaction.AccountId,
            DestinationAccountId = transaction.DestinationAccountId,
            CategoryId = transaction.CategoryId,
            Tags = transaction.Tags,
            IsReconciled = transaction.IsReconciled,
            UserId = transaction.UserId
        };
        destinationAccountId = transaction.DestinationAccountId;
        showDialog = true;

        await Task.CompletedTask;
    }

    private async Task DeleteTransaction(GridCommandEventArgs args)
    {
        var transaction = (Transaction)args.Item;

        if (string.IsNullOrEmpty(userId)) return;

        var result = await TransactionService.DeleteTransactionAsync(transaction.Id, userId);

        if (result)
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = "Transaction deleted successfully!",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                CloseAfter = 3000
            });
            await LoadTransactions();
        }
        else
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = "Failed to delete transaction.",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Error,
                CloseAfter = 3000
            });
        }
    }

    private async Task DeleteSelectedTransactions()
    {
        if (string.IsNullOrEmpty(userId)) return;

        foreach (var transaction in selectedTransactions)
        {
            await TransactionService.DeleteTransactionAsync(transaction.Id, userId);
        }

        notificationRef?.Show(new NotificationModel
        {
            Text = $"{selectedTransactions.Count()} transactions deleted!",
            ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
            CloseAfter = 3000
        });

        selectedTransactions = Enumerable.Empty<Transaction>();
        await LoadTransactions();
    }

    private void OnTypeChanged(TransactionType type)
    {
        editTransaction.Type = type;
        if (type == TransactionType.Transfer)
        {
            editTransaction.CategoryId = null;
        }
        else
        {
            destinationAccountId = null;
        }
    }

    private async Task SaveTransaction()
    {
        if (string.IsNullOrEmpty(userId)) return;

        editTransaction.UserId = userId;
        if (editTransaction.Type == TransactionType.Transfer)
        {
            editTransaction.DestinationAccountId = destinationAccountId;
            editTransaction.CategoryId = null;
        }
        else
        {
            editTransaction.DestinationAccountId = null;
        }

        try
        {
            if (isEditing)
            {
                await TransactionService.UpdateTransactionAsync(editTransaction);
                notificationRef?.Show(new NotificationModel
                {
                    Text = "Transaction updated successfully!",
                    ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                    CloseAfter = 3000
                });
            }
            else
            {
                await TransactionService.CreateTransactionAsync(editTransaction);
                notificationRef?.Show(new NotificationModel
                {
                    Text = "Transaction created successfully!",
                    ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                    CloseAfter = 3000
                });
            }

            showDialog = false;
            await LoadTransactions();
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

    private async Task ExportToExcel()
    {
        if (gridRef != null)
        {
            await gridRef.SaveAsExcelFileAsync();
        }
    }
}
