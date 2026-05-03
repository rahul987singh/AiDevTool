# DevTeamAI.LLM

A configurable DLL for LLM model invocation with support for GPU (DirectML) and CPU fallback, designed for easy integration into any .NET project.

## Features

- **Configurable Model Selection**: Easily switch between different models via configuration
- **GPU/CPU Fallback**: Automatic GPU (DirectML) with CPU fallback or forced execution modes
- **Semantic Kernel Integration**: Full Semantic Kernel support with streaming capabilities
- **Dependency Injection Ready**: Simple DI registration for any project
- **Logging Support**: Built-in logging for monitoring and debugging

## Quick Start

### 1. Install the DLL

Reference the `DevTeamAI.LLM.dll` in your project or add it as a project reference.

### 2. Configure Services

In your `Program.cs` or `Startup.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;

// Add LLM services with configuration from appsettings.json
builder.Services.AddLLMServices(builder.Configuration);

// OR configure programmatically
builder.Services.AddLLMServices(config =>
{
    config.BasePath = "D:/Project/Models/Llama-3.1-8B-Instruct-ONNX";
    config.ModelId = "llama-3.1-8b-instruct";
    config.ExecutionProvider = ExecutionProviderOptions.Auto; // Auto, DirectML, or CPU
    config.EnableLogging = true;
});
```

### 3. Configuration

Add to your `appsettings.json`:

```json
{
  "LLM": {
    "BasePath": "D:/Project/Models/Llama-3.1-8B-Instruct-ONNX",
    "ModelId": "llama-3.1-8b-instruct",
    "ServiceId": "onnx-service-llama",
    "ExecutionProvider": "Auto",
    "DeviceId": 0,
    "CpuFallbackPath": "D:/Project/Models/Llama-3.1-8B-Instruct-ONNX",
    "EnableLogging": true
  }
}
```

### 4. Use the Service

```csharp
public class MyService
{
    private readonly LLMService _llmService;

    public MyService(LLMService llmService)
    {
        _llmService = llmService;
    }

    public async Task<string> GetResponse(string userMessage)
    {
        var messages = new List<ChatMessage>
        {
            new() { Role = "User", Content = "Hello" },
            new() { Role = "Assistant", Content = "Hi there!" }
        };

        return await _llmService.GetChatResponseAsync(userMessage, messages);
    }

    public async IAsyncEnumerable<string> GetStreamingResponse(string userMessage)
    {
        await foreach (var chunk in _llmService.GetStreamingChatResponseAsync(userMessage))
        {
            yield return chunk;
        }
    }
}
```

## Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BasePath` | string | `"D:/Project/Models/Llama-3.1-8B-Instruct-ONNX"` | Primary model path |
| `ModelId` | string | `"llama-3.1-8b-instruct"` | Model identifier |
| `ServiceId` | string | `"onnx-service-llama"` | Service identifier |
| `ExecutionProvider` | string | `"Auto"` | `"Auto"`, `"DirectML"`, or `"CPU"` |
| `DeviceId` | int | `0` | GPU device ID for DirectML |
| `CpuFallbackPath` | string | `""` | CPU fallback model path |
| `EnableLogging` | bool | `true` | Enable/disable logging |

## Execution Provider Modes

- **Auto**: Tries GPU first, falls back to CPU if GPU fails
- **DirectML**: Forces GPU/DirectML execution only
- **CPU**: Forces CPU execution only

## Required NuGet Packages

The DLL includes all necessary dependencies. When used in another project, ensure you have:

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" />
<PackageReference Include="Microsoft.Extensions.Configuration" />
<PackageReference Include="Microsoft.Extensions.Logging" />
```

## Model Setup

Ensure your model directory contains:
- `model.onnx`
- `model.onnx.data`
- `genai_config.json`
- `tokenizer.json`
- `vocab.json`

For Llama-3.1-8B-Instruct DirectML model, download using:

```bash
python -c "
from huggingface_hub import snapshot_download
snapshot_download(
    repo_id='onnx-community/Meta-Llama-3.1-8B-Instruct-ONNX-DirectML-GenAI-INT4',
    local_dir='D:/Project/Models/Llama-3.1-8B-Instruct-ONNX'
)
print('Download complete!')
"
```

## Error Handling

The service includes comprehensive error handling:
- Automatic GPU/CPU fallback in Auto mode
- Detailed logging for debugging
- Graceful degradation when services are unavailable

## Logging

Enable logging to see model loading status and errors:

```
🚀 Attempting to load GPU (DirectML)...
✅ GPU model loaded successfully
```

or fallback scenarios:

```
⚠️ GPU failed, falling back to CPU...
✅ CPU fallback model loaded successfully
```
