using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using CentuitionApp.Data;

namespace CentuitionApp.Components.Pages.Finance.CategoriesComponents;

public partial class CategoriesTabsGrid
{
    [Parameter, EditorRequired]
    public int ActiveTabIndex { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<int> ActiveTabIndexChanged { get; set; }

    [Parameter, EditorRequired]
    public List<Category> ExpenseCategories { get; set; } = new();

    [Parameter, EditorRequired]
    public List<Category> IncomeCategories { get; set; } = new();

    [Parameter, EditorRequired]
    public EventCallback<GridCommandEventArgs> OnEdit { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<GridCommandEventArgs> OnDelete { get; set; }

    private async Task OnActiveTabIndexChanged(int index)
    {
        await ActiveTabIndexChanged.InvokeAsync(index);
    }
}
