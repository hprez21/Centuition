using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using CentuitionApp.Data;
using CentuitionApp.Services;
using Telerik.Blazor.Components;
using System.Globalization;
using System.Text;

namespace CentuitionApp.Components.Pages.Finance.DashboardComponents;

public partial class FinancialAssistantChat
{
    [Inject] private IChatClient ChatClient { get; set; } = default!;

    [Parameter]
    public string? UserId { get; set; }

    [Parameter, EditorRequired]
    public List<Data.Account> Accounts { get; set; } = new();

    [Parameter, EditorRequired]
    public List<Transaction> RecentTransactions { get; set; } = new();

    [Parameter, EditorRequired]
    public List<CategorySpending> CategorySpending { get; set; } = new();

    [Parameter, EditorRequired]
    public List<BudgetProgress> BudgetProgress { get; set; } = new();

    [Parameter, EditorRequired]
    public List<TransactionSummary> MonthlyTrends { get; set; } = new();

    [Parameter, EditorRequired]
    public decimal TotalBalance { get; set; }

    [Parameter, EditorRequired]
    public decimal PeriodIncome { get; set; }

    [Parameter, EditorRequired]
    public decimal PeriodExpenses { get; set; }

    [Parameter, EditorRequired]
    public decimal PeriodNetSavings { get; set; }

    [Parameter, EditorRequired]
    public string PeriodLabel { get; set; } = string.Empty;

    private bool IsOpen;
    private TelerikChat<FinanceChatMessage>? chatRef;
    private List<FinanceChatMessage> chatMessages = new();
    private const string ChatCurrentUserId = "user";
    private const string ChatAssistantId = "assistant";
    private List<string> chatSuggestions = new()
    {
        "What's my total balance?",
        "Show my spending by category",
        "How much did I spend this month?",
        "What are my top expenses?",
        "Am I over budget?"
    };

    private void ToggleChat()
    {
        IsOpen = !IsOpen;
        if (IsOpen && !chatMessages.Any())
        {
            chatMessages.Add(new FinanceChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                AuthorId = ChatAssistantId,
                AuthorName = "Financial Assistant",
                Content = "Hello! I'm your AI financial assistant. I can help you understand your finances. Ask me about your balance, spending, budgets, or transactions!",
                Timestamp = DateTime.Now
            });
        }
    }

    private void ClearChat()
    {
        chatMessages.Clear();
        chatMessages.Add(new FinanceChatMessage
        {
            Id = Guid.NewGuid().ToString(),
            AuthorId = ChatAssistantId,
            AuthorName = "Financial Assistant",
            Content = "Chat cleared. How can I help you with your finances today?",
            Timestamp = DateTime.Now
        });
        chatRef?.Refresh();
    }

    private async Task OnChatSendMessage(ChatSendMessageEventArgs args)
    {
        var userMessage = new FinanceChatMessage
        {
            Id = Guid.NewGuid().ToString(),
            AuthorId = ChatCurrentUserId,
            AuthorName = "You",
            Content = args.Message,
            Timestamp = DateTime.Now
        };
        chatMessages.Add(userMessage);

        var typingMessage = new FinanceChatMessage
        {
            Id = "typing",
            AuthorId = ChatAssistantId,
            AuthorName = "Financial Assistant",
            Content = "",
            IsTyping = true,
            Timestamp = DateTime.Now
        };
        chatMessages.Add(typingMessage);
        chatRef?.Refresh();

        try
        {
            var response = await GetFinanceAssistantResponse(args.Message);

            chatMessages.RemoveAll(m => m.Id == "typing");
            chatMessages.Add(new FinanceChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                AuthorId = ChatAssistantId,
                AuthorName = "Financial Assistant",
                Content = response,
                Timestamp = DateTime.Now
            });
        }
        catch (Exception)
        {
            chatMessages.RemoveAll(m => m.Id == "typing");
            chatMessages.Add(new FinanceChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                AuthorId = ChatAssistantId,
                AuthorName = "Financial Assistant",
                Content = "I'm sorry, I encountered an error processing your request. Please try again.",
                Timestamp = DateTime.Now
            });
        }

        chatRef?.Refresh();
    }

    private async Task OnChatSuggestionClick(ChatSuggestionClickEventArgs args)
    {
        await OnChatSendMessage(new ChatSendMessageEventArgs { Message = args.Suggestion });
    }

    private async Task<string> GetFinanceAssistantResponse(string question)
    {
        if (string.IsNullOrEmpty(UserId)) return "Please log in to access your financial data.";

        var financialContext = BuildFinancialContext();

        var systemPrompt = $@"You are a helpful financial assistant for a personal finance tracking application called Centuition. 
You have access to the user's financial data and should provide helpful, accurate, and actionable insights.

IMPORTANT RULES:
- Always format currency values with $ and two decimal places (e.g., $1,234.56)
- Be concise but informative
- If asked about something not in the context, politely explain you can only answer questions about their financial data
- Provide specific numbers and details from the context when relevant
- Suggest actionable insights when appropriate
- Never make up data that isn't in the context

USER'S FINANCIAL DATA CONTEXT:
{financialContext}

Current Date: {DateTime.Today:MMMM d, yyyy}";

        var options = new ChatOptions
        {
            Instructions = systemPrompt
        };

        var response = await ChatClient.GetResponseAsync(question, options);
        return response.Text;
    }

    private string BuildFinancialContext()
    {
        if (string.IsNullOrEmpty(UserId)) return "No user data available.";

        var usCulture = CultureInfo.GetCultureInfo("en-US");
        var sb = new StringBuilder();

        sb.AppendLine("=== ACCOUNTS ===");
        foreach (var account in Accounts)
        {
            sb.AppendLine($"- {account.Name} ({account.AccountType}): {account.CurrentBalance.ToString("C2", usCulture)}");
        }
        sb.AppendLine($"Total Balance: {TotalBalance.ToString("C2", usCulture)}");
        sb.AppendLine();

        sb.AppendLine($"=== CURRENT PERIOD ({PeriodLabel}) ===");
        sb.AppendLine($"Income: {PeriodIncome.ToString("C2", usCulture)}");
        sb.AppendLine($"Expenses: {PeriodExpenses.ToString("C2", usCulture)}");
        sb.AppendLine($"Net Savings: {PeriodNetSavings.ToString("C2", usCulture)}");
        sb.AppendLine();

        if (CategorySpending.Any())
        {
            sb.AppendLine("=== SPENDING BY CATEGORY ===");
            foreach (var cat in CategorySpending.OrderByDescending(c => c.TotalAmount))
            {
                sb.AppendLine($"- {cat.CategoryName}: ${cat.TotalAmount:F2} ({cat.TransactionCount} transactions, {cat.Percentage:F1}%)");
            }
            sb.AppendLine();
        }

        if (BudgetProgress.Any())
        {
            sb.AppendLine($"=== BUDGET STATUS ({DateTime.Today:MMMM yyyy}) ===");
            foreach (var budget in BudgetProgress)
            {
                var status = budget.IsOverBudget ? "OVER BUDGET" : budget.PercentageUsed >= 80 ? "WARNING" : "On Track";
                sb.AppendLine($"- {budget.CategoryName}: {budget.SpentAmount.ToString("C2", usCulture)} of {budget.BudgetAmount.ToString("C2", usCulture)} ({budget.PercentageUsed:F0}% used) [{status}]");
            }
            sb.AppendLine();
        }

        if (RecentTransactions.Any())
        {
            sb.AppendLine("=== RECENT TRANSACTIONS (Last 10) ===");
            foreach (var tx in RecentTransactions)
            {
                var sign = tx.Type == TransactionType.Income ? "+" : tx.Type == TransactionType.Expense ? "-" : "↔";
                sb.AppendLine($"- {tx.Date:MMM dd}: {sign}{tx.Amount.ToString("C2", usCulture)} - {tx.Description} ({tx.Category?.Name ?? "No category"})");
            }
            sb.AppendLine();
        }

        if (MonthlyTrends.Any())
        {
            sb.AppendLine("=== MONTHLY TRENDS (Last 6 months) ===");
            foreach (var trend in MonthlyTrends)
            {
                sb.AppendLine($"- {trend.MonthName}: Income {trend.TotalIncome.ToString("C2", usCulture)}, Expenses {trend.TotalExpenses.ToString("C2", usCulture)}, Net {trend.NetAmount.ToString("C2", usCulture)}");
            }
        }

        return sb.ToString();
    }

    public class FinanceChatMessage
    {
        public string Id { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsTyping { get; set; }
    }
}
