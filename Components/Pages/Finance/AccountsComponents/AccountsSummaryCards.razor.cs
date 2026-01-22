using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;

namespace CentuitionApp.Components.Pages.Finance.AccountsComponents;

public partial class AccountsSummaryCards
{
    [Parameter, EditorRequired]
    public Dictionary<AccountType, decimal> AccountsByType { get; set; } = new();

    [Parameter, EditorRequired]
    public List<Data.Account> Accounts { get; set; } = new();

    private int AccountCount(AccountType type) => Accounts.Count(a => a.AccountType == type);
}
