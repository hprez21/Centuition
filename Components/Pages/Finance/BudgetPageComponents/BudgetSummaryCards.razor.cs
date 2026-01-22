using Microsoft.AspNetCore.Components;

namespace CentuitionApp.Components.Pages.Finance.BudgetPageComponents;

public partial class BudgetSummaryCards
{
    [Parameter, EditorRequired]
    public decimal TotalBudget { get; set; }

    [Parameter, EditorRequired]
    public decimal TotalSpent { get; set; }

    [Parameter, EditorRequired]
    public decimal TotalRemaining { get; set; }

    [Parameter, EditorRequired]
    public decimal OverallPercentage { get; set; }

    [Parameter, EditorRequired]
    public int BudgetCount { get; set; }
}
