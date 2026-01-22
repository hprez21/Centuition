using Microsoft.AspNetCore.Components;
using CentuitionApp.Data;

namespace CentuitionApp.Components.Pages.Finance.TransactionsComponents;

public partial class TransactionsFilterPanel
{
    [Parameter]
    public DateTime? FilterStartDate { get; set; }

    [Parameter]
    public EventCallback<DateTime?> OnFilterStartDateChanged { get; set; }

    [Parameter]
    public DateTime? FilterEndDate { get; set; }

    [Parameter]
    public EventCallback<DateTime?> OnFilterEndDateChanged { get; set; }

    [Parameter]
    public TransactionType? FilterType { get; set; }

    [Parameter]
    public EventCallback<TransactionType?> OnFilterTypeChanged { get; set; }

    [Parameter]
    public int? FilterAccountId { get; set; }

    [Parameter]
    public EventCallback<int?> OnFilterAccountIdChanged { get; set; }

    [Parameter]
    public int? FilterCategoryId { get; set; }

    [Parameter]
    public EventCallback<int?> OnFilterCategoryIdChanged { get; set; }

    [Parameter, EditorRequired]
    public List<DropDownOption> TransactionTypeOptions { get; set; } = new();

    [Parameter, EditorRequired]
    public List<Data.Account> AccountOptions { get; set; } = new();

    [Parameter, EditorRequired]
    public List<Category> CategoryOptions { get; set; } = new();

    [Parameter, EditorRequired]
    public EventCallback OnApplyFilters { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnClearFilters { get; set; }

    public class DropDownOption
    {
        public TransactionType? Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
