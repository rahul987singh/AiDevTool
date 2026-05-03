using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace DevTeamAI.Plugins;

public class MermaidPlugin
{
    private readonly string _diagramPath;

    public MermaidPlugin(string outputFolderName = "GeneratedCode")
    {
        _diagramPath = Path.Combine(Directory.GetCurrentDirectory(), outputFolderName, "architecture.md");
    }

    [KernelFunction, Description("Saves a Mermaid.js Entity Relationship (ER) diagram for the database schema.")]
    public async Task<string> SaveDatabaseDiagram(
        [Description("The Mermaid syntax string (e.g., erDiagram BOOK ||--o{ AUTHOR : writes)")] string mermaidCode)
    {
        try
        {
            string content = $"# System Architecture\n\n```mermaid\n{mermaidCode}\n```";
            await File.WriteAllTextAsync(_diagramPath, content);
            
            Console.WriteLine("[ARCHITECT] Saved Mermaid diagram to architecture.md");
            return "Diagram saved successfully. Please review the architecture.md file.";
        }
        catch (Exception ex)
        {
            return $"Failed to save diagram: {ex.Message}";
        }
    }
}