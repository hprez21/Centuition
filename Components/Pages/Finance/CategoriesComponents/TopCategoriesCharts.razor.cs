using Microsoft.AspNetCore.Components;
using CentuitionApp.Services;

namespace CentuitionApp.Components.Pages.Finance.CategoriesComponents;

public partial class TopCategoriesCharts
{
    [Parameter, EditorRequired]
    public List<CategorySpending> ExpenseCategorySpending { get; set; } = new();

    [Parameter, EditorRequired]
    public List<CategorySpending> IncomeCategorySpending { get; set; } = new();
}
