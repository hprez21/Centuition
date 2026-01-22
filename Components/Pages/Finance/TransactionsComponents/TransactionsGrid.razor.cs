using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using CentuitionApp.Data;

namespace CentuitionApp.Components.Pages.Finance.TransactionsComponents;

public partial class TransactionsGrid
{
    private TelerikGrid<Transaction>? gridRef;

    [Parameter, EditorRequired]
    public List<Transaction> Transactions { get; set; } = new();

    [Parameter, EditorRequired]
    public IEnumerable<Transaction> SelectedTransactions { get; set; } = Enumerable.Empty<Transaction>();

    [Parameter, EditorRequired]
    public EventCallback<IEnumerable<Transaction>> OnSelectedItemsChanged { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<GridCommandEventArgs> OnEdit { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<GridCommandEventArgs> OnDelete { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnDeleteSelected { get; set; }

    private async Task HandleEdit(GridCommandEventArgs args)
    {
        await OnEdit.InvokeAsync(args);
    }

    private async Task HandleDelete(GridCommandEventArgs args)
    {
        await OnDelete.InvokeAsync(args);
    }

    public async Task SaveAsExcelFileAsync()
    {
        if (gridRef != null)
        {
            await gridRef.SaveAsExcelFileAsync();
        }
    }
}
