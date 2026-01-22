using Microsoft.AspNetCore.Components;

namespace CentuitionApp.Components.Pages.Finance.Shared;

public partial class PageHeaderWithAction
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string Subtitle { get; set; } = string.Empty;

    [Parameter]
    public bool ShowActionButton { get; set; } = true;

    [Parameter]
    public string ActionButtonText { get; set; } = "Add";

    [Parameter]
    public EventCallback OnActionClick { get; set; }
}
