using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;
using static CentuitionApp.Components.Pages.Finance.BudgetPageComponents.BudgetPageHeader;

namespace CentuitionApp.Components.Pages.Finance.BudgetPageComponents;

public partial class BudgetFormDialog
{
    [Parameter, EditorRequired]
    public bool IsVisible { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter, EditorRequired]
    public bool IsEditing { get; set; }

    [Parameter, EditorRequired]
    public Budget Budget { get; set; } = new();

    [Parameter, EditorRequired]
    public List<Category> AvailableCategories { get; set; } = new();

    [Parameter, EditorRequired]
    public List<MonthOption> Months { get; set; } = new();

    [Parameter, EditorRequired]
    public EventCallback<int> OnCategoryChanged { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnSave { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnCancel { get; set; }
}
