using Microsoft.AspNetCore.Components;

namespace CentuitionApp.Components.Pages.Finance.ReportsComponents;

public partial class IncomeVsExpensesTrendChart
{
    [Parameter, EditorRequired]
    public List<TrendDataPoint> TrendData { get; set; } = new();

    public class TrendDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetAmount => TotalIncome - TotalExpenses;
    }
}
