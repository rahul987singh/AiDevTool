using DevTeamAI.LLM.Services;
using DevTeamAI.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DevTeamAI;

public class DevTeamAIApplication
{
    private readonly LLMService _llmService;
    private readonly ILogger<DevTeamAIApplication> _logger;
    private readonly Kernel _kernel;

    public DevTeamAIApplication(LLMService llmService, ILogger<DevTeamAIApplication> logger)
    {
        _llmService = llmService;
        _logger = logger;

        // 1. Get the Kernel from your DLL
        _kernel = _llmService.GetKernel();

        // 2. Register the Plugins
        // These provide the 'tools' for the Architect and Developer
        _kernel.Plugins.AddFromObject(new FileSystemPlugin(), "FileSystem");
        _kernel.Plugins.AddFromObject(new MermaidPlugin(), "ArchitectTools");
    }

    public async Task RunAsync()
    {
        Console.WriteLine("🤖 DevTeamAI Multi-Agent System Activated");
        Console.WriteLine("==========================================");

        // 3. Define the Specialized Agents
        var designer = new ChatCompletionAgent()
        {
            Name = "ProductDesigner",
            Instructions = "You are a Product Designer. Convert user ideas into a JSON requirement spec. Define Entities, UserRoles, and PrimaryActions. Output ONLY the JSON.",
            Kernel = _kernel
        };

        var architect = new ChatCompletionAgent()
        {
            Name = "SystemsArchitect",
            Instructions = "You are a Systems Architect. Analyze the JSON requirements and design a technical blueprint. You MUST use the ArchitectTools-SaveDatabaseDiagram tool to save a Mermaid diagram of the schema.",
            Kernel = _kernel
        };

        var developer = new ChatCompletionAgent()
        {
            Name = "DotNetDeveloper",
            Instructions = "You are a Senior .NET Developer. Write Minimal API C# code based on the Architect's design. You MUST use the FileSystem-WriteFile tool to save .cs and .csproj files to disk. Ensure code is complete and has all necessary using statements.",
            Kernel = _kernel
        };

        // 4. Define Orchestration
        AgentGroupChat chat = new(designer, architect, developer)
        {
            ExecutionSettings = new()
            {
                // Force the order: Designer -> Architect -> Developer
                SelectionStrategy = new SequentialSelectionStrategy()
                {
                    InitialAgent = designer
                },
                // Pause after the Architect to allow for Human Approval
                TerminationStrategy = new ApprovalTerminationStrategy()
            }
        };

        Console.Write("\nDescribe your project idea (e.g., 'I want a task manager'): ");
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) return;

        // 5. Start the Agentic Flow
        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

        try
        {
            await foreach (var response in chat.InvokeAsync())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n--- {response.AuthorName?.ToUpper()} ---");
                Console.ResetColor();
                Console.WriteLine(response.Content);

                // HITL GATE: This logic triggers when the TerminationStrategy returns 'true'
                if (response.AuthorName == "SystemsArchitect")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n[GATE] Review the design above. Type 'APPROVE' to allow the Developer to write code, or anything else to exit:");
                    Console.ResetColor();

                    string? gateInput = Console.ReadLine();
                    if (gateInput?.ToUpper() != "APPROVE")
                    {
                        Console.WriteLine("❌ Project halted by user.");
                        return;
                    }

                    Console.WriteLine("🚀 Approval received. Handing off to Developer...");
                }
            }

            Console.WriteLine("\n✅ Project generation complete! Check the 'GeneratedCode' folder.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during agentic workflow execution.");
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Custom strategy to pause the conversation after the Architect finishes.
    /// </summary>
    private class ApprovalTerminationStrategy : TerminationStrategy
    {
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        {
            // Terminate the automatic loop specifically after the Architect speaks.
            // This returns control to the 'await foreach' loop in RunAsync.
            return Task.FromResult(agent.Name == "SystemsArchitect");
        }
    }
}