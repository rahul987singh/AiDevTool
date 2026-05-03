# LLM Integration Guide

This guide explains how to consume the DevTeamAI.LLM DLL in client applications and integrate it with GPU configuration support.

## Overview

The DevTeamAI.LLM DLL provides a configurable, production-ready solution for LLM model invocation with:
- GPU (DirectML) acceleration with automatic CPU fallback
- Semantic Kernel integration
- Dependency injection support
- Configurable model selection and execution providers

## Prerequisites

1. **DevTeamAI.LLM DLL** - Located at `d:\Project\Semantic Kernal\DevTeamAI.LLM\bin\Debug\net10.0\DevTeamAI.LLM.dll`
2. **Llama-3.1-8B-Instruct Model** - Downloaded to `D:\Project\Models\Llama-3.1-8B-Instruct-ONNX\`
3. **.NET 10.0** runtime environment
4. **DirectML compatible GPU** (optional, will fallback to CPU)

## ⚠️ IMPORTANT: Model Hardware Requirements

### DirectML (GPU) Support
The Llama 3.1 ONNX DirectML model is optimized for any DirectX 12 compatible GPU (AMD, Intel, or NVIDIA).

**Hardware Checklist:**
- DirectX 12 (Feature Level 12_0) support
- Latest GPU drivers installed
- Sufficient VRAM (at least 4GB recommended for 8B INT4 model)

### CPU Fallback
If a compatible GPU is not found or fails to initialize, the application will automatically fallback to CPU execution using the same ONNX model weights.

## Pre-Implementation Testing Steps

### Step 1: Verify Hardware Compatibility

**Check your processor type:**
```bash
# Windows PowerShell
Get-WmiObject -Class Win32_Processor | Select-Object Name
```

**Expected results:**
- Intel: Use CPU mode or convert model to ONNX
- AMD Ryzen AI: Can use AMD DirectML model

### Step 2: Test Basic LLM Setup

**1. Configure for CPU Mode**
```json
{
  "LLM": {
    "BasePath": "D:/Project/Models/Llama-3.1-8B-Instruct-ONNX",
    "ExecutionProvider": "CPU",
    "ModelId": "llama-3.1-8b-instruct"
  }
}
```

**2. Run the Console Application**
```bash
cd "d:\Project\Semantic Kernal\DevTeamAI"
dotnet run
```

**3. Expected Output:**
```
🤖 DevTeamAI LLM Console Application
=====================================
✅ LLM service initialized successfully!
🚀 GPU/DirectML acceleration enabled with CPU fallback
```

### Step 3: Test GPU Acceleration (If Available)

**1. Check GPU Compatibility**
```bash
# Verify DirectX 12 support
dxdiag
```

**2. Update Configuration for GPU:**
```json
{
  "LLM": {
    "BasePath": "D:/Project/Models/Llama-3.1-8B-Instruct-ONNX",
    "ExecutionProvider": "Auto",
    "ModelId": "llama-3.1-8b-instruct"
  }
}
```

**3. Monitor Logs for GPU Usage:**
- Look for: `🚀 Attempting to load GPU (DirectML)...`
- Success: `✅ GPU model loaded successfully`
- Fallback: `⚠️ GPU failed, falling back to CPU...`

### Step 4: Test Basic Functionality

**Run these test commands in the console:**

1. **Basic Chat Test:**
   ```
   You: Hello, can you introduce yourself?
   ```

2. **Status Check:**
   ```
   You: status
   ```

3. **Clear History:**
   ```
   You: clear
   ```

### Step 5: Troubleshooting Common Issues

**Error: "onnxruntime_vitis_ai_custom_ops.dll missing"**
- **Cause**: Using AMD-specific model on Intel system
- **Solution**: Switch to CPU mode or convert model

**Error: "Model files not found"**
- **Cause**: Incorrect model path or missing files
- **Solution**: Verify model files exist in configured directory

**Error: "GPU failed to load"**
- **Cause**: Incompatible GPU or drivers
- **Solution**: Use CPU mode or update GPU drivers

### Step 6: Performance Testing

**CPU Mode Performance:**
- Expect 2-5 seconds response time
- Monitor CPU usage during inference

**GPU Mode Performance (if available):**
- Expect sub-second response time
- Monitor GPU memory usage

### Step 7: Integration Validation

**Verify DLL Integration:**
1. Check that DevTeamAI.LLM.dll is referenced
2. Confirm appsettings.json is being loaded
3. Validate logging output shows model loading

**Test Dependency Injection:**
- LLMService should be properly injected
- Configuration should be bound correctly
- Logging should capture initialization

## Before Development Checklist

- [ ] Hardware type identified (Intel/AMD)
- [ ] Correct model format downloaded
- [ ] Configuration set for appropriate execution provider
- [ ] Basic console application runs without errors
- [ ] Test chat responses work correctly
- [ ] GPU/CPU fallback tested (if applicable)
- [ ] Error handling verified
- [ ] Performance baseline established

## Next Steps After Testing

Once you've completed the testing steps and confirmed the LLM is working:

1. **Integrate into your actual application**
2. **Customize the LLMService as needed**
3. **Add application-specific error handling**
4. **Implement user authentication if required**
5. **Add response caching for performance**

## Quick Integration Steps

### 1. Add DLL Reference

In your client project, add a reference to the DevTeamAI.LLM DLL:

**Option A: Project Reference (if in same solution)**
```xml
<ProjectReference Include="..\DevTeamAI.LLM\DevTeamAI.LLM.csproj" />
```

**Option B: DLL Reference**
```xml
<Reference Include="DevTeamAI.LLM">
  <HintPath>..\DevTeamAI.LLM\bin\Debug\net10.0\DevTeamAI.LLM.dll</HintPath>
</Reference>
```

### 2. Install Required NuGet Packages

```bash
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Logging.Console
```

### 3. Configure appsettings.json

Create or update `appsettings.json` in your client project:

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
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "DevTeamAI.LLM.Services.LLMService": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### 4. Register Services

In your `Program.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DevTeamAI.LLM.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add LLM services
builder.Services.AddLLMServices(builder.Configuration);

// Add your application services
builder.Services.AddTransient<MyApplicationService>();

var host = builder.Build();

// Run your application
await host.Services.GetRequiredService<MyApplicationService>().RunAsync();
```

### 5. Use LLM Service in Your Application

```csharp
using DevTeamAI.LLM.Services;

public class MyApplicationService
{
    private readonly LLMService _llmService;
    private readonly ILogger<MyApplicationService> _logger;

    public MyApplicationService(LLMService llmService, ILogger<MyApplicationService> logger)
    {
        _llmService = llmService;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        // Check if LLM service is available
        if (!_llmService.IsKernelAvailable())
        {
            _logger.LogError("LLM service is not available");
            return;
        }

        // Simple chat example
        var response = await _llmService.GetChatResponseAsync("Hello, how are you?");
        Console.WriteLine($"LLM Response: {response}");

        // Streaming chat example
        Console.WriteLine("Streaming response:");
        await foreach (var chunk in _llmService.GetStreamingChatResponseAsync("Tell me a short story"))
        {
            Console.Write(chunk);
        }
        Console.WriteLine();
    }
}
```

## Configuration Options

### Execution Provider Modes

| Mode | Description | Use Case |
|------|-------------|----------|
| `Auto` | Tries GPU first, falls back to CPU | Recommended for most applications |
| `DirectML` | Forces GPU execution only | When you want to guarantee GPU usage |
| `CPU` | Forces CPU execution only | For testing or CPU-only environments |

### Model Paths

- **BasePath**: Primary model directory (GPU model)
- **CpuFallbackPath**: CPU model directory for fallback
- **DeviceId**: GPU device ID (0 for primary GPU)

### Logging Configuration

Enable detailed logging to monitor model loading and execution:

```json
{
  "Logging": {
    "LogLevel": {
      "DevTeamAI.LLM.Services.LLMService": "Information"
    }
  }
}
```

Expected log messages:
```
🚀 Attempting to load GPU (DirectML)...
✅ GPU model loaded successfully
```
or fallback:
```
⚠️ GPU failed, falling back to CPU...
✅ CPU fallback model loaded successfully
```

## Advanced Usage

### Custom Configuration

```csharp
// Programmatic configuration instead of appsettings.json
builder.Services.AddLLMServices(config =>
{
    config.BasePath = "D:/Project/Models/CustomModel";
    config.ExecutionProvider = ExecutionProviderOptions.DirectML;
    config.DeviceId = 1; // Use second GPU
    config.EnableLogging = true;
});
```

### Chat History Management

```csharp
var messages = new List<ChatMessage>
{
    new() { Role = "User", Content = "What is Semantic Kernel?" },
    new() { Role = "Assistant", Content = "Semantic Kernel is an AI SDK..." }
};

var response = await _llmService.GetChatResponseAsync("Can you explain more?", messages);
```

### Error Handling

The LLM service includes built-in error handling:

```csharp
try
{
    var response = await _llmService.GetChatResponseAsync(userMessage);
    return response;
}
catch (Exception ex)
{
    _logger.LogError(ex, "LLM request failed");
    return "I apologize, but I'm currently unable to process your request.";
}
```

## Deployment Considerations

### Production Checklist

- [ ] Model files are present in the configured directory
- [ ] GPU drivers are up to date (for DirectML)
- [ ] Application has read permissions to model directory
- [ ] Logging is configured appropriately
- [ ] Error handling is implemented
- [ ] Resource limits are considered (memory usage)

### Performance Optimization

1. **GPU Usage**: Ensure `ExecutionProvider` is set to `"Auto"` or `"DirectML"`
2. **Memory**: Monitor memory usage, especially for large models
3. **Concurrency**: Consider request queuing for high-load scenarios
4. **Caching**: Implement response caching where appropriate

## Troubleshooting

### Common Issues

**Issue: "GPU failed, falling back to CPU"**
- Check GPU driver compatibility
- Verify DirectML support
- Ensure model is in correct format

**Issue: "ChatCompletionService unavailable"**
- Verify model files exist in the configured path
- Check file permissions
- Review configuration settings

**Issue: High memory usage**
- Consider using smaller models
- Implement request batching
- Monitor resource usage

### Debug Mode

Enable detailed logging for troubleshooting:

```json
{
  "Logging": {
    "LogLevel": {
      "DevTeamAI.LLM": "Debug"
    }
  }
}
```

## Example: Complete Console Application

```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DevTeamAI.LLM.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false);

builder.Services.AddLLMServices(builder.Configuration);
builder.Services.AddTransient<ChatApplication>();

var host = builder.Build();
await host.Services.GetRequiredService<ChatApplication>().RunAsync();

public class ChatApplication
{
    private readonly LLMService _llmService;
    private readonly ILogger<ChatApplication> _logger;

    public ChatApplication(LLMService llmService, ILogger<ChatApplication> logger)
    {
        _llmService = llmService;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("🤖 LLM Chat Application");
        Console.WriteLine("Type 'exit' to quit\n");

        while (true)
        {
            Console.Write("You: ");
            var input = Console.ReadLine();
            
            if (input?.ToLower() == "exit") break;
            if (string.IsNullOrWhiteSpace(input)) continue;

            Console.Write("Assistant: ");
            await foreach (var chunk in _llmService.GetStreamingChatResponseAsync(input))
            {
                Console.Write(chunk);
            }
            Console.WriteLine("\n");
        }
    }
}
```

This integration guide provides everything needed to successfully consume the DevTeamAI.LLM DLL in any .NET application with full GPU/CPU configuration support.
