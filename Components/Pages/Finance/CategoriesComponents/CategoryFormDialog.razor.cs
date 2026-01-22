using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;

namespace CentuitionApp.Components.Pages.Finance.CategoriesComponents;

public partial class CategoryFormDialog
{
    [Parameter, EditorRequired]
    public bool IsVisible { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter, EditorRequired]
    public bool IsEditing { get; set; }

    [Parameter, EditorRequired]
    public Category Category { get; set; } = new();

    [Parameter, EditorRequired]
    public string CategoryColor { get; set; } = "#6c757d";

    [Parameter, EditorRequired]
    public EventCallback<string> CategoryColorChanged { get; set; }

    [Parameter, EditorRequired]
    public List<CategoryType> CategoryTypes { get; set; } = new();

    [Parameter, EditorRequired]
    public EventCallback OnSave { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnCancel { get; set; }

    private async Task OnColorChanged(string value)
    {
        await CategoryColorChanged.InvokeAsync(value);
    }
}
