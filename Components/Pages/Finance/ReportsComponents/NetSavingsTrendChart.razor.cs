using Microsoft.AspNetCore.Components;

namespace CentuitionApp.Components.Pages.Finance.ReportsComponents;

public partial class NetSavingsTrendChart
{
    [Parameter, EditorRequired]
    public List<IncomeVsExpensesTrendChart.TrendDataPoint> TrendData { get; set; } = new();
}
