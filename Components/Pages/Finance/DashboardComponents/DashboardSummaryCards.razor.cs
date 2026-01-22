using Microsoft.AspNetCore.Components;

namespace CentuitionApp.Components.Pages.Finance.DashboardComponents;

public partial class DashboardSummaryCards
{
    [Parameter, EditorRequired]
    public decimal TotalBalance { get; set; }

    [Parameter, EditorRequired]
    public decimal PeriodIncome { get; set; }

    [Parameter, EditorRequired]
    public decimal PeriodExpenses { get; set; }

    [Parameter, EditorRequired]
    public decimal PeriodNetSavings { get; set; }

    [Parameter, EditorRequired]
    public int AccountsCount { get; set; }

    [Parameter, EditorRequired]
    public string PeriodLabel { get; set; } = string.Empty;
}
