using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using CentuitionApp.Data;

namespace CentuitionApp.Components.Pages.Finance.BudgetPageComponents;

public partial class BudgetDetailsGrid
{
    [Parameter, EditorRequired]
    public List<Budget> Budgets { get; set; } = new();

    [Parameter, EditorRequired]
    public EventCallback<GridCommandEventArgs> OnEdit { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<GridCommandEventArgs> OnDelete { get; set; }
}
