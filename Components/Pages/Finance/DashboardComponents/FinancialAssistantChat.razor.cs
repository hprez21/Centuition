using Microsoft.AspNetCore.Components;
using CentuitionApp.Services;
using Telerik.Blazor.Components;

namespace CentuitionApp.Components.Pages.Finance.DashboardComponents;

public partial class FinancialAssistantChat
{
    [Inject] private IFinancialAssistantService AssistantService { get; set; } = default!;

    [Parameter]
    public string? UserId { get; set; }

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
                Content = "I'm sorry, I encountered an unexpected error. Please try again.",
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
        return await AssistantService.GetResponseAsync(UserId ?? string.Empty, question);
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
