using CentuitionApp.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Telerik.AI.SmartComponents.Extensions;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Grid;

namespace CentuitionApp.Components.Pages.Finance.TransactionsComponents;

public partial class TransactionsGrid
{
    private TelerikGrid<Transaction>? gridRef;

    private static readonly JsonSerializerOptions AiResponseSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };


    [Inject]
    private IChatClient ChatClient { get; set; } = default!;

    [Inject]
    private ILogger<TransactionsGrid> Logger { get; set; } = default!;

    private static readonly List<string> AIPromptSuggestions =
    [
        "Group transactions by category.",
        "Show expenses from last month.",
        "Sort by amount descending."
    ];

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
    private async Task OnAIPromptRequest(AIPromptPromptRequestEventArgs args)
    {
        try
        {
            if (gridRef is null)
            {
                args.Output = "The grid is not ready. Try again.";
                args.Response = BuildEmptyAiResponse("Grid not ready.");
                return;
            }

            var requestData = gridRef.GetAIRequest(args.Prompt);

            var chatOptions = new ChatOptions();
            chatOptions.AddGridChatTools(requestData.Columns.Select(ToGridAIColumn).ToList());

            var response = await ChatClient.GetResponseAsync(args.Prompt, chatOptions);

            var gridResponse = response.ExtractGridResponse() ?? new GridAIResponse();
            if (gridResponse.Commands.Count == 0)
            {
                gridResponse.Message ??= "No grid commands were generated.";
            }

            var json = JsonSerializer.Serialize(gridResponse, AiResponseSerializerOptions);

            args.Output = string.IsNullOrWhiteSpace(gridResponse.Message)
                ? "The request was processed."
                : gridResponse.Message;
            args.Response = json;

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "AI prompt request failed.");
            args.Output = $"AI error: {ex.GetType().Name} - {ex.Message}";
            args.Response = BuildEmptyAiResponse("AI error.");
        }
    }

    private static GridAIColumn ToGridAIColumn(GridAIColumnDescriptor descriptor)
    {
        return new GridAIColumn
        {
            Id = descriptor.Id,
            Field = descriptor.Field,
            Type = descriptor.Type,
            Values = descriptor.Values,
            Columns = descriptor.Columns?.Select(ToGridAIColumn).ToList()
        };
    }

    private static string BuildEmptyAiResponse(string? message)
    {
        var response = new GridAIResponse
        {
            Message = message
        };

        return JsonSerializer.Serialize(response, AiResponseSerializerOptions);
    }

}
