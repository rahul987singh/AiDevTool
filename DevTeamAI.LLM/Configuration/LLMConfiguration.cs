namespace DevTeamAI.LLM.Configuration;

public class LLMConfiguration
{
    public string BasePath { get; set; } = "";
    public string ModelId { get; set; } = "";
    public string ServiceId { get; set; } = "";
    public string ExecutionProvider { get; set; } = "";
    public string GpuSubPath { get; set; } = "";
    public string CpuSubPath { get; set; } = "";
    public string CpuFallbackPath { get; set; } = "";
    public int DeviceId { get; set; } = 0;
    public bool EnableLogging { get; set; } = true;
}

public class ExecutionProviderOptions
{
    public const string Auto = "Auto";
    public const string DirectML = "DirectML";
    public const string CPU = "CPU";
}
