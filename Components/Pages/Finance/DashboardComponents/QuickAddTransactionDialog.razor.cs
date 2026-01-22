using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;

namespace CentuitionApp.Components.Pages.Finance.DashboardComponents;

public partial class QuickAddTransactionDialog
{
    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter, EditorRequired]
    public Transaction Transaction { get; set; } = new();

    [Parameter]
    public int? DestinationAccountId { get; set; }

    [Parameter]
    public EventCallback<int?> DestinationAccountIdChanged { get; set; }

    [Parameter, EditorRequired]
    public List<Data.Account> Accounts { get; set; } = new();

    [Parameter, EditorRequired]
    public List<Category> Categories { get; set; } = new();

    [Parameter]
    public EventCallback OnSave { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Parameter]
    public EventCallback OnTransactionChanged { get; set; }

    private List<Category> GetCategoriesForType(TransactionType type)
    {
        var categoryType = type == TransactionType.Income ? CategoryType.Income : CategoryType.Expense;
        return Categories.Where(c => c.Type == categoryType).ToList();
    }
}
