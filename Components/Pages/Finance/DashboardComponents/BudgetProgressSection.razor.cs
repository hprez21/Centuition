using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;
using CentuitionApp.Services;
using System.Globalization;

namespace CentuitionApp.Components.Pages.Finance.DashboardComponents;

public partial class BudgetProgressSection
{
    [Parameter, EditorRequired]
    public List<BudgetProgress> BudgetProgress { get; set; } = new();

    [Parameter]
    public int MaxItems { get; set; } = 6;
}
