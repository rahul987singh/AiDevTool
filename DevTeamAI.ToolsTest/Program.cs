using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DevTeamAI.ToolsTest;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Minimal function-invocation test (shim)");

        // Create plugin instance
        var plugin = new TimePlugin();

        // Create a tool using the (shim) AIFunctionFactory
        var timeTool = AIFunctionFactory.Create(nameof(TimePlugin.GetSystemTime), (Func<string>)plugin.GetSystemTime, typeof(TimePlugin), "Gets the current system time");

        // Build a simple chat client and wrap it with function-invocation support
        IChatClient baseClient = new MockChatClient();
        var builder = new ChatClientBuilder(baseClient);
        var wrapped = builder.UseFunctionInvocation();

        // Prepare chat options with the Time tool
        var options = new ChatOptions
        {
            Tools = new List<AIFunction> { timeTool }
        };

        // Send a prompt that instructs the model to use tools
        var prompt = "What time is it right now? Use your tools to find out.";

        var response = await wrapped.SendMessageAsync(prompt, options);

        Console.WriteLine("Model response:\n" + response);
    }
}
