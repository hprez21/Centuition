using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using CentuitionApp.Data;

namespace CentuitionApp.Components.Pages.Finance.AccountsComponents;

public partial class AccountsGrid
{
    [Parameter, EditorRequired]
    public List<Data.Account> Accounts { get; set; } = new();

    [Parameter, EditorRequired]
    public EventCallback<GridCommandEventArgs> OnEdit { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<GridCommandEventArgs> OnDelete { get; set; }
}
