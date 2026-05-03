using System;
using System.ComponentModel;

namespace DevTeamAI.ToolsTest;

public class TimePlugin
{
    [Description("Gets the current system time")]
    public string GetSystemTime()
    {
        Console.WriteLine("[TimePlugin] GetSystemTime invoked (code-behind executing)");
        return DateTime.Now.ToString("O");
    }
}
