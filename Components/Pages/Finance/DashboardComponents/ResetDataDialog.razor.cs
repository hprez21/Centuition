using Microsoft.AspNetCore.Components;

namespace CentuitionApp.Components.Pages.Finance.DashboardComponents;

public partial class ResetDataDialog
{
    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter]
    public bool IsResetting { get; set; }

    [Parameter]
    public EventCallback OnConfirm { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }
}
