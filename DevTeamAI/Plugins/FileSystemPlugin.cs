using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace DevTeamAI.Plugins;

public class FileSystemPlugin
{
    private readonly string _basePath;

    public FileSystemPlugin(string outputFolderName = "GeneratedCode")
    {
        // We set a base path to ensure the AI doesn't write files all over your PC
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), outputFolderName);
        
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    [KernelFunction, Description("Writes C# code or configuration text to a specific file on disk.")]
    public async Task<string> WriteFile(
        [Description("The name of the file including extension (e.g. Program.cs)")] string fileName,
        [Description("The full content of the file")] string content)
    {
        try
        {
            string filePath = Path.Combine(_basePath, fileName);
            
            // Ensure the sub-directory exists if the AI provides a path like "Models/User.cs"
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(filePath, content);
            
            Console.WriteLine($"[FILE SYSTEM] Success: Saved {fileName} to disk.");
            return $"Successfully wrote file: {fileName}";
        }
        catch (Exception ex)
        {
            return $"Error writing file: {ex.Message}";
        }
    }
}