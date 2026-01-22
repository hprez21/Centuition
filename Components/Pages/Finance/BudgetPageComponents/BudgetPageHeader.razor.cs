using Microsoft.AspNetCore.Components;

namespace CentuitionApp.Components.Pages.Finance.BudgetPageComponents;

public partial class BudgetPageHeader
{
    [Parameter, EditorRequired]
    public List<MonthOption> Months { get; set; } = new();

    [Parameter, EditorRequired]
    public int SelectedMonth { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<int> OnMonthChanged { get; set; }

    [Parameter, EditorRequired]
    public int SelectedYear { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<int> OnYearChanged { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnRefresh { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnAddBudget { get; set; }

    public class MonthOption
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
