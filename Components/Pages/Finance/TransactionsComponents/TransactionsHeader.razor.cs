using Microsoft.AspNetCore.Components;

namespace CentuitionApp.Components.Pages.Finance.TransactionsComponents;

public partial class TransactionsHeader
{
    [Parameter, EditorRequired]
    public EventCallback OnExportClick { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnAddClick { get; set; }
}
