using Microsoft.AspNetCore.Components;

namespace CentuitionApp.Components.Pages.Finance.DashboardComponents;

public partial class DashboardHeader
{
    [Parameter] public string SelectedPeriod { get; set; } = "month";
    [Parameter] public DateTime SelectedDate { get; set; } = DateTime.Today;
    [Parameter] public int SelectedYear { get; set; } = DateTime.Today.Year;
    [Parameter] public DateTime? PeriodStartDate { get; set; }
    [Parameter] public DateTime? PeriodEndDate { get; set; }
    
    [Parameter] public EventCallback<string> OnPeriodChanged { get; set; }
    [Parameter] public EventCallback OnDateChanged { get; set; }
    [Parameter] public EventCallback OnYearChanged { get; set; }
    [Parameter] public EventCallback OnCustomRangeChanged { get; set; }
    [Parameter] public EventCallback OnAddTransaction { get; set; }
    [Parameter] public EventCallback OnResetData { get; set; }
}
