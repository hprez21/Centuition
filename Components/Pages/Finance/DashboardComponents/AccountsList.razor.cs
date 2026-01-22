using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;
using CentuitionApp.Helpers;
using System.Globalization;

namespace CentuitionApp.Components.Pages.Finance.DashboardComponents;

public partial class AccountsList
{
    [Parameter, EditorRequired]
    public List<Data.Account> Accounts { get; set; } = new();

    [Parameter]
    public int MaxItems { get; set; } = 5;
}
