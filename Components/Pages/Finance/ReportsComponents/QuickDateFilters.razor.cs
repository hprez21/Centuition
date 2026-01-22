using Microsoft.AspNetCore.Components;

namespace CentuitionApp.Components.Pages.Finance.ReportsComponents;

public partial class QuickDateFilters
{
    [Parameter, EditorRequired]
    public EventCallback<DateRangeType> OnDateRangeSelected { get; set; }

    public enum DateRangeType
    {
        ThisMonth,
        LastMonth,
        Last3Months,
        Last6Months,
        ThisYear,
        LastYear
    }
}
