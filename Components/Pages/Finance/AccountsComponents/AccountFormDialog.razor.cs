using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;
using CentuitionApp.Helpers;

namespace CentuitionApp.Components.Pages.Finance.AccountsComponents;

public partial class AccountFormDialog
{
    [Parameter, EditorRequired]
    public bool IsVisible { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter, EditorRequired]
    public bool IsEditing { get; set; }

    [Parameter, EditorRequired]
    public Data.Account Account { get; set; } = new();

    [Parameter, EditorRequired]
    public string AccountColor { get; set; } = FinanceUIHelpers.DefaultAccountColor;

    [Parameter, EditorRequired]
    public EventCallback<string> AccountColorChanged { get; set; }

    [Parameter, EditorRequired]
    public List<AccountType> AccountTypes { get; set; } = new();

    [Parameter, EditorRequired]
    public List<string> Currencies { get; set; } = new();

    [Parameter, EditorRequired]
    public EventCallback OnSave { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnCancel { get; set; }

    private async Task OnColorChanged(string value)
    {
        await AccountColorChanged.InvokeAsync(value);
    }
}
