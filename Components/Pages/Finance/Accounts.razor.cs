using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using CentuitionApp.Data;
using CentuitionApp.Services;
using CentuitionApp.Helpers;

namespace CentuitionApp.Components.Pages.Finance;

public partial class Accounts : ComponentBase
{
    [Inject] private IAccountService AccountService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private string? userId;
    private List<Data.Account> accounts = new();
    private Dictionary<AccountType, decimal> accountsByType = new();

    private bool showDialog;
    private bool isEditing;
    private Data.Account editAccount = new();
    private string accountColor = FinanceUIHelpers.DefaultAccountColor;

    private List<AccountType> accountTypes = Enum.GetValues<AccountType>().ToList();
    private List<string> currencies = new() { "USD", "EUR", "GBP", "CAD", "AUD", "MXN", "JPY", "CNY" };

    private TelerikNotification? notificationRef;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await LoadAccounts();
        }
    }

    private async Task LoadAccounts()
    {
        if (string.IsNullOrEmpty(userId)) return;

        accounts = await AccountService.GetAllAccountsAsync(userId);
        accountsByType = await AccountService.GetBalancesByTypeAsync(userId);
    }

    private void OpenCreateDialog()
    {
        isEditing = false;
        editAccount = new Data.Account
        {
            UserId = userId ?? string.Empty,
            IsActive = true,
            IncludeInTotal = true,
            Currency = "USD",
            InitialBalance = 0
        };
        accountColor = FinanceUIHelpers.DefaultAccountColor;
        showDialog = true;
    }

    private async Task EditAccountHandler(GridCommandEventArgs args)
    {
        var account = (Data.Account)args.Item;

        isEditing = true;
        editAccount = new Data.Account
        {
            Id = account.Id,
            Name = account.Name,
            Description = account.Description,
            AccountType = account.AccountType,
            InitialBalance = account.InitialBalance,
            CurrentBalance = account.CurrentBalance,
            Currency = account.Currency,
            Color = account.Color,
            Icon = account.Icon,
            IsActive = account.IsActive,
            IncludeInTotal = account.IncludeInTotal,
            UserId = account.UserId
        };
        accountColor = account.Color ?? FinanceUIHelpers.DefaultAccountColor;
        showDialog = true;

        await Task.CompletedTask;
    }

    private async Task DeleteAccount(GridCommandEventArgs args)
    {
        var account = (Data.Account)args.Item;

        if (string.IsNullOrEmpty(userId)) return;

        var result = await AccountService.DeleteAccountAsync(account.Id, userId);

        if (result)
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = "Account deleted successfully!",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                CloseAfter = 3000
            });
            await LoadAccounts();
        }
        else
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = "Failed to delete account.",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Error,
                CloseAfter = 3000
            });
        }
    }

    private void CloseDialog()
    {
        showDialog = false;
    }

    private async Task SaveAccount()
    {
        if (string.IsNullOrEmpty(userId)) return;

        editAccount.Color = accountColor;
        editAccount.UserId = userId;

        try
        {
            if (isEditing)
            {
                await AccountService.UpdateAccountAsync(editAccount);
                notificationRef?.Show(new NotificationModel
                {
                    Text = "Account updated successfully!",
                    ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                    CloseAfter = 3000
                });
            }
            else
            {
                await AccountService.CreateAccountAsync(editAccount);
                notificationRef?.Show(new NotificationModel
                {
                    Text = "Account created successfully!",
                    ThemeColor = ThemeConstants.Notification.ThemeColor.Success,
                    CloseAfter = 3000
                });
            }

            showDialog = false;
            await LoadAccounts();
        }
        catch (InvalidOperationException ex)
        {
            notificationRef?.Show(new NotificationModel
            {
                Text = ex.Message,
                ThemeColor = ThemeConstants.Notification.ThemeColor.Warning,
                CloseAfter = 5000
            });
        }
        catch (Exception ex)
        {
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            notificationRef?.Show(new NotificationModel
            {
                Text = $"Unexpected error: {errorMessage}",
                ThemeColor = ThemeConstants.Notification.ThemeColor.Error,
                CloseAfter = 7000
            });
        }
    }
}
