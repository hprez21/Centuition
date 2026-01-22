using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;
using System.Globalization;

namespace CentuitionApp.Components.Pages.Finance.DashboardComponents;

public partial class RecentTransactionsGrid
{
    [Parameter, EditorRequired]
    public List<Transaction> Transactions { get; set; } = new();

    [Parameter]
    public EventCallback OnAddTransaction { get; set; }
}
