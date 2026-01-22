using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;

namespace CentuitionApp.Components.Pages.Finance.TransactionsComponents;

public partial class TransactionFormDialog
{
    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter]
    public bool IsEditing { get; set; }

    [Parameter, EditorRequired]
    public Transaction Transaction { get; set; } = new();

    [Parameter]
    public int? DestinationAccountId { get; set; }

    [Parameter]
    public EventCallback<int?> OnDestinationAccountIdChanged { get; set; }

    [Parameter, EditorRequired]
    public List<Data.Account> Accounts { get; set; } = new();

    [Parameter, EditorRequired]
    public List<Category> Categories { get; set; } = new();

    [Parameter, EditorRequired]
    public EventCallback<TransactionType> OnTypeChange { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnSubmit { get; set; }

    private void OnTypeChanged(TransactionType type)
    {
        Transaction.Type = type;
        OnTypeChange.InvokeAsync(type);
    }

    private List<Data.Account> GetAvailableDestinationAccounts()
    {
        return Accounts.Where(a => a.Id != Transaction.AccountId).ToList();
    }

    private List<Category> GetCategoriesForType()
    {
        var categoryType = Transaction.Type == TransactionType.Income ? CategoryType.Income : CategoryType.Expense;
        return Categories.Where(c => c.Type == categoryType).ToList();
    }

    private async Task HandleSubmit()
    {
        await OnSubmit.InvokeAsync();
    }
}
