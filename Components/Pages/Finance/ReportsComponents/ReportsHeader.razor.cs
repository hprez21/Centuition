using Microsoft.AspNetCore.Components;

namespace CentuitionApp.Components.Pages.Finance.ReportsComponents;

public partial class ReportsHeader
{
    [Parameter]
    public DateTime? StartDate { get; set; }

    [Parameter]
    public EventCallback<DateTime?> OnStartDateChanged { get; set; }

    [Parameter]
    public DateTime? EndDate { get; set; }

    [Parameter]
    public EventCallback<DateTime?> OnEndDateChanged { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnGenerateReport { get; set; }
}
