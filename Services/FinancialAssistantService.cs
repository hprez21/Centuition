using System.ClientModel;
using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace CentuitionApp.Services;

/// <summary>
/// AI-powered financial assistant service using tool calling for on-demand data retrieval.
/// </summary>
public class FinancialAssistantService : IFinancialAssistantService
{
    private readonly IChatClient _chatClient;
    private readonly FinancialTools _financialTools;
    private readonly ILogger<FinancialAssistantService> _logger;

    public FinancialAssistantService(
        IChatClient chatClient, 
        FinancialTools financialTools,
        ILogger<FinancialAssistantService> logger)
    {
        _chatClient = chatClient;
        _financialTools = financialTools;
        _logger = logger;
    }

    public async Task<string> GetResponseAsync(string userId, string question)
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("[AI:{RequestId}] START | User: {UserId} | Query: \"{Question}\"", 
            requestId, userId, question);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("[AI:{RequestId}] REJECTED | User not authenticated", requestId);
            return "Please log in to access your financial data.";
        }

        _financialTools.SetUserId(userId);

        var tools = GetAvailableTools();
        var chatOptions = new ChatOptions
        {
            Instructions = BuildSystemPrompt(),
            Tools = tools
        };

        _logger.LogInformation("[AI:{RequestId}] TOOLS | Available: {ToolCount}", 
            requestId, tools.Count);

        try
        {
            var response = await _chatClient.GetResponseAsync(question, chatOptions);
            stopwatch.Stop();

            LogToolCalls(requestId, response);

            _logger.LogInformation("[AI:{RequestId}] COMPLETE | Duration: {ElapsedMs}ms | Tokens: In={InputTokens}, Out={OutputTokens}", 
                requestId, 
                stopwatch.ElapsedMilliseconds,
                response.Usage?.InputTokenCount ?? 0,
                response.Usage?.OutputTokenCount ?? 0);

            _logger.LogDebug("[AI:{RequestId}] RESPONSE | \"{Response}\"", 
                requestId, TruncateForLog(response.Text, 200));

            return response.Text;
        }
        catch (ClientResultException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[AI:{RequestId}] ERROR | Duration: {ElapsedMs}ms | Status: {Status}", 
                requestId, stopwatch.ElapsedMilliseconds, ex.Status);
            return GetErrorMessage(ex.Status);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[AI:{RequestId}] ERROR | Duration: {ElapsedMs}ms | {ErrorType}: {Message}", 
                requestId, stopwatch.ElapsedMilliseconds, ex.GetType().Name, ex.Message);
            throw;
        }
    }

    private void LogToolCalls(string requestId, ChatResponse response)
    {
        var toolCalls = response.Messages
            .SelectMany(m => m.Contents)
            .OfType<FunctionCallContent>()
            .ToList();

        var toolResults = response.Messages
            .SelectMany(m => m.Contents)
            .OfType<FunctionResultContent>()
            .ToList();

        if (toolCalls.Any())
        {
            _logger.LogInformation("[AI:{RequestId}] TOOL_CALLS | Count: {ToolCount}", requestId, toolCalls.Count);
            foreach (var call in toolCalls)
            {
                var args = call.Arguments != null 
                    ? string.Join(", ", call.Arguments.Select(kvp => $"{kvp.Key}={kvp.Value}"))
                    : "none";
                _logger.LogInformation("[AI:{RequestId}]   --> {ToolName}({Args})", 
                    requestId, call.Name, args);
            }
        }
        else
        {
            _logger.LogInformation("[AI:{RequestId}] TOOL_CALLS | None", requestId);
        }

        if (toolResults.Any())
        {
            foreach (var result in toolResults)
            {
                var resultText = result.Result?.ToString() ?? "null";
                _logger.LogDebug("[AI:{RequestId}]   <-- Result: {Result}", 
                    requestId, TruncateForLog(resultText, 100));
            }
        }
    }

    private static string TruncateForLog(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }

    private string BuildSystemPrompt()
    {
        return $@"You are a helpful financial assistant for Centuition, a personal finance tracking application.
You have access to tools that can retrieve the user's financial data. Use these tools to answer questions accurately.

IMPORTANT RULES:
- Use the available tools to get data before answering financial questions
- Always format currency values with $ and two decimal places (e.g., $1,234.56)
- Be concise but informative in your responses
- If asked about something not available through the tools, politely explain what information you can provide
- Provide specific numbers and actionable insights when relevant
- Today's date is {DateTime.Today:MMMM d, yyyy}

Available tools allow you to:
- Get account information and balances
- View recent transactions
- Check spending by category
- Review budget status and progress
- Analyze monthly income/expense trends
- Calculate totals for income, expenses, and savings";
    }

    private IList<AITool> GetAvailableTools() =>
    [
        AIFunctionFactory.Create(_financialTools.GetAccounts),
        AIFunctionFactory.Create(_financialTools.GetTotalBalance),
        AIFunctionFactory.Create(_financialTools.GetBalancesByAccountType),
        AIFunctionFactory.Create(_financialTools.GetRecentTransactions),
        AIFunctionFactory.Create(_financialTools.GetCategorySpending),
        AIFunctionFactory.Create(_financialTools.GetBudgetStatus),
        AIFunctionFactory.Create(_financialTools.GetMonthlyTrends),
        AIFunctionFactory.Create(_financialTools.GetTotalIncome),
        AIFunctionFactory.Create(_financialTools.GetTotalExpenses),
        AIFunctionFactory.Create(_financialTools.GetNetSavings),
        AIFunctionFactory.Create(_financialTools.GetTopExpenseCategories)
    ];

    private static string GetErrorMessage(int statusCode) => statusCode switch
    {
        401 => "Authentication error with the AI service. Please check the API key configuration.",
        429 => "The AI service is currently busy. Please try again in a moment.",
        500 or 502 or 503 => "The AI service is temporarily unavailable. Please try again later.",
        _ => $"An error occurred with the AI service (Status: {statusCode}). Please try again."
    };
}
