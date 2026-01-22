using Microsoft.AspNetCore.Components;
using CentuitionApp.Services;

namespace CentuitionApp.Components.Pages.Finance.ReportsComponents;

public partial class CategoryBreakdownTable
{
    [Parameter, EditorRequired]
    public List<CategorySpending> CategorySpending { get; set; } = new();
}
