using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using CentuitionApp.Data;

namespace CentuitionApp.Components.Pages.Finance.ReportsComponents;

public partial class TransactionDetailsGrid
{
    private TelerikGrid<Transaction>? gridRef;

    [Parameter, EditorRequired]
    public List<Transaction> Transactions { get; set; } = new();

    [Parameter]
    public EventCallback OnExport { get; set; }

    private async Task HandleExport()
    {
        if (gridRef != null)
        {
            await gridRef.SaveAsExcelFileAsync();
        }
        
        if (OnExport.HasDelegate)
        {
            await OnExport.InvokeAsync();
        }
    }
}
