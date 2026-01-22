namespace CentuitionApp.Services;

/// <summary>
/// Service interface for AI-powered financial assistant interactions.
/// </summary>
public interface IFinancialAssistantService
{
    /// <summary>
    /// Gets an AI-generated response to a financial question using tool calling.
    /// </summary>
    /// <param name="userId">The user ID for data access.</param>
    /// <param name="question">The user's question.</param>
    /// <returns>The AI assistant's response.</returns>
    Task<string> GetResponseAsync(string userId, string question);
}
