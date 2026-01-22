using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;
using CentuitionApp.Services;

namespace CentuitionApp.Components.Pages.Finance.DashboardComponents;

public partial class CategorySpendingChart
{
    [Parameter, EditorRequired]
    public List<CategorySpending> Data { get; set; } = new();

    [Parameter]
    public string Height { get; set; } = "300px";
}
