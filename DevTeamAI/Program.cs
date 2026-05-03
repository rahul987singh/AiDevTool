using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DevTeamAI.LLM.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add LLM services
builder.Services.AddLLMServices(builder.Configuration);

// Add our application service
builder.Services.AddTransient<DevTeamAIApplication>();

var host = builder.Build();

// Run the application
await host.Services.GetRequiredService<DevTeamAIApplication>().RunAsync();

public class DevTeamAIApplication
{
    private readonly LLMService _llmService;
    private readonly ILogger<DevTeamAIApplication> _logger;

    public DevTeamAIApplication(LLMService llmService, ILogger<DevTeamAIApplication> logger)
    {
        _llmService = llmService;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("🤖 DevTeamAI LLM Console Application");
        Console.WriteLine("=====================================");
        
        // Check if LLM service is available
        if (!_llmService.IsKernelAvailable())
        {
            _logger.LogError("❌ LLM service is not available");
            Console.WriteLine("❌ LLM service failed to initialize. Check logs for details.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("✅ LLM service initialized successfully!");
        Console.WriteLine("🚀 GPU/DirectML acceleration enabled with CPU fallback");
        Console.WriteLine();
        Console.WriteLine("Type 'exit' to quit, 'help' for commands");
        Console.WriteLine();

        var chatHistory = new List<ChatMessage>();

        while (true)
        {
            Console.Write("You: ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(input)) continue;
            
            if (input.ToLower() == "exit")
            {
                Console.WriteLine("👋 Goodbye!");
                break;
            }

            if (input.ToLower() == "help")
            {
                ShowHelp();
                continue;
            }

            if (input.ToLower() == "clear")
            {
                chatHistory.Clear();
                Console.WriteLine("🗑️ Chat history cleared.");
                continue;
            }

            if (input.ToLower() == "status")
            {
                ShowStatus();
                continue;
            }

            try
            {
                Console.Write("Assistant: ");
                var userMessage = new ChatMessage { Role = "User", Content = input };
                
                // Get non-streaming response to test tokenizer
                var response = await _llmService.GetChatResponseAsync(input, chatHistory);
                Console.WriteLine(response);

                // Add to chat history
                chatHistory.Add(userMessage);
                chatHistory.Add(new ChatMessage { Role = "Assistant", Content = response });

                // Keep chat history manageable (last 10 exchanges)
                if (chatHistory.Count > 20)
                {
                    chatHistory.RemoveRange(0, 2);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing LLM request");
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
            
            Console.WriteLine();
        }
    }

    private void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  help   - Show this help message");
        Console.WriteLine("  clear  - Clear chat history");
        Console.WriteLine("  status - Show LLM service status");
        Console.WriteLine("  exit   - Exit the application");
        Console.WriteLine();
    }

    private void ShowStatus()
    {
        Console.WriteLine("🔍 LLM Service Status:");
        Console.WriteLine($"  Available: {_llmService.IsKernelAvailable()}");
        Console.WriteLine($"  Kernel: {_llmService.GetKernel()?.GetType().Name ?? "N/A"}");
        Console.WriteLine();
    }
}
