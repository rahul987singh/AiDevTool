using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace DevTeamAI.ToolsTest;

// Minimal shims to demonstrate function-invocation wiring without external packages
public record AIFunction(string Name, Delegate Delegate, Type DeclaringType, string Description);

public static class AIFunctionFactory
{
    public static AIFunction Create(string name, Delegate del, Type declaringType, string description)
    {
        return new AIFunction(name, del, declaringType, description);
    }
}

public class ChatOptions
{
    public List<AIFunction> Tools { get; set; } = new();
}

public interface IChatClient
{
    Task<string> SendMessageAsync(string prompt, ChatOptions? options = null);
}

// A simple mock chat client that simulates a model not using tools by itself
public class MockChatClient : IChatClient
{
    public Task<string> SendMessageAsync(string prompt, ChatOptions? options = null)
    {
        // naive model text generation simulation
        return Task.FromResult($"[MockModel] Received prompt: {prompt}\n(assistant would normally generate a reply)");
    }
}

public class ChatClientBuilder
{
    private readonly IChatClient _inner;

    public ChatClientBuilder(IChatClient inner)
    {
        _inner = inner;
    }

    // Return a wrapper that supports function invocation using provided ChatOptions.Tools
    public IChatClient UseFunctionInvocation()
    {
        return new FunctionInvokingChatClient(_inner);
    }
}

public class FunctionInvokingChatClient : IChatClient
{
    private readonly IChatClient _inner;

    public FunctionInvokingChatClient(IChatClient inner)
    {
        _inner = inner;
    }

    public async Task<string> SendMessageAsync(string prompt, ChatOptions? options = null)
    {
        // If the model is instructed to use tools and we have tools available, invoke them.
        if (options?.Tools != null && options.Tools.Count > 0 && prompt?.ToLower().Contains("use your tools") == true)
        {
            // For this minimal test, just call the first tool synchronously and include its result in the response
            var tool = options.Tools[0];
            try
            {
                var resultObj = tool.Delegate.DynamicInvoke(Array.Empty<object>());
                var result = resultObj?.ToString() ?? string.Empty;
                // Simulate the model incorporating the tool result into its answer
                return $"[MockModel] I used tool '{tool.Name}' and got: {result}\nFinal answer: The current time is {result}";
            }
            catch (Exception ex)
            {
                return $"[MockModel] Tool invocation failed: {ex.Message}";
            }
        }

        // Default: delegate to underlying client
        return await _inner.SendMessageAsync(prompt, options);
    }
}
