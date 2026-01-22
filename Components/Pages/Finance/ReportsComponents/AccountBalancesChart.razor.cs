using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;

namespace CentuitionApp.Components.Pages.Finance.ReportsComponents;

public partial class AccountBalancesChart
{
    [Parameter, EditorRequired]
    public List<Data.Account> Accounts { get; set; } = new();
}
