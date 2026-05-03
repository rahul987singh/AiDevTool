using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;

namespace DevTeamAI.LLM.Services;

public class LLMService
{
    private readonly Kernel _kernel;
    private readonly ILogger<LLMService>? _logger;

    public LLMService(Kernel kernel, ILogger<LLMService>? logger = null)
    {
        _kernel = kernel;
        _logger = logger;
    }

    public Kernel GetKernel()
    {
        return _kernel;
    }

    public bool IsKernelAvailable()
    {
        if (_kernel == null) return false;
        try
        {
            _kernel.GetRequiredService<IChatCompletionService>();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetChatResponseAsync(string userMessage, List<ChatMessage>? messages = null)
    {
        messages ??= new List<ChatMessage>();
        
        var chatHistory = new ChatHistory();
        foreach (var msg in messages)
        {
            if(msg.Role == "User")
            {
                chatHistory.AddUserMessage(msg.Content);
            }
            else if(msg.Role == "Assistant")
            {
                chatHistory.AddAssistantMessage(msg.Content);
            }
        }          
        chatHistory.AddUserMessage(userMessage);  

        IChatCompletionService chatCompletionService;
        try
        {
            chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[LLMService] ChatCompletionService unavailable");
            return "Sorry, the chat completion service is unavailable.";
        }

        var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatHistory: chatHistory,
            kernel: _kernel
        );

        var responseBuilder = new System.Text.StringBuilder();
        await foreach (var content in result)
        {
            if (content.Content != null)
            {
                responseBuilder.Append(content.Content);
            }
        }
        
        return responseBuilder.ToString();
    }

    public async IAsyncEnumerable<string> GetStreamingChatResponseAsync(string userMessage, List<ChatMessage>? messages = null)
    {
        messages ??= new List<ChatMessage>();
        
        var chatHistory = new ChatHistory();
        foreach (var msg in messages)
        {
            if(msg.Role == "User")
            {
                chatHistory.AddUserMessage(msg.Content);
            }
            else if(msg.Role == "Assistant")
            {
                chatHistory.AddAssistantMessage(msg.Content);
            }
        }          
        chatHistory.AddUserMessage(userMessage);  

        IChatCompletionService? chatCompletionService = null;
        try
        {
            chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[LLMService] ChatCompletionService unavailable");
        }

        if (chatCompletionService == null)
        {
            yield return "Sorry, the chat completion service is unavailable.";
            yield break;
        }

        var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatHistory: chatHistory,
            kernel: _kernel
        );

        await foreach (var content in result)
        {
            yield return content.Content ?? string.Empty;
        }
    }
}

public class ChatMessage
{
    public string Role { get; set; } = "User";
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
