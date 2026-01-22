using Microsoft.AspNetCore.Components;
using CentuitionApp.Services;

namespace CentuitionApp.Components.Pages.Finance.BudgetPageComponents;

public partial class BudgetProgressList
{
    [Parameter, EditorRequired]
    public List<BudgetProgress> BudgetProgress { get; set; } = new();

    [Parameter, EditorRequired]
    public EventCallback OnCopyToNextMonth { get; set; }
}
