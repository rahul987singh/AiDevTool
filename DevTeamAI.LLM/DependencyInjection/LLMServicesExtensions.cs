using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using DevTeamAI.LLM.Configuration;
using DevTeamAI.LLM.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class LLMServicesExtensions
{
    public static IServiceCollection AddLLMServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<LLMConfiguration>(config => configuration.GetSection("LLM").Bind(config));
        
        // Validate configuration
        var llmConfig = configuration.GetSection("LLM").Get<LLMConfiguration>();
        if (llmConfig == null)
        {
            throw new InvalidOperationException("LLM configuration section is missing. Please add 'LLM' section to appsettings.json");
        }
        
        if (string.IsNullOrWhiteSpace(llmConfig.BasePath))
        {
            throw new InvalidOperationException("LLM BasePath is required. Please set 'LLM:BasePath' in appsettings.json");
        }
        
        if (string.IsNullOrWhiteSpace(llmConfig.ModelId))
        {
            throw new InvalidOperationException("LLM ModelId is required. Please set 'LLM:ModelId' in appsettings.json");
        }
        
        if (string.IsNullOrWhiteSpace(llmConfig.ExecutionProvider))
        {
            throw new InvalidOperationException("LLM ExecutionProvider is required. Please set 'LLM:ExecutionProvider' in appsettings.json");
        }
        
        // Register Custom ONNX Chat Completion Service with GPU->CPU fallback
        services.AddSingleton<IChatCompletionService>(sp =>
        {
            var rootLogger = sp.GetService<ILogger<LLMService>>();
            var serviceLogger = sp.GetService<ILogger<CustomOnnxChatCompletionService>>();
            var config = sp.GetRequiredService<IConfiguration>();
            llmConfig = configuration.GetSection("LLM").Get<LLMConfiguration>() ?? new LLMConfiguration();

            var modelBase = llmConfig.BasePath;
            var executionProvider = llmConfig.ExecutionProvider ?? ExecutionProviderOptions.Auto;
            var deviceId = llmConfig.DeviceId;

            // If ExecutionProvider is Auto or DirectML, attempt GPU first
            if (string.Equals(executionProvider, ExecutionProviderOptions.DirectML, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(executionProvider, ExecutionProviderOptions.Auto, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    rootLogger?.LogInformation("🚀 Attempting to initialize ONNX service with DirectML (GPU)");
                    return new CustomOnnxChatCompletionService(
                        modelId: llmConfig.ModelId,
                        modelPath: modelBase,
                        executionProvider: "dml",
                        deviceId: deviceId,
                        logger: serviceLogger
                    );
                }
                catch (Exception ex)
                {
                    rootLogger?.LogWarning(ex, "⚠️ DirectML initialization failed, attempting CPU fallback");
                    // fall through to CPU attempt below
                }
            }

            // CPU fallback or explicit CPU mode
            var cpuPath = !string.IsNullOrWhiteSpace(llmConfig.CpuFallbackPath) ? llmConfig.CpuFallbackPath : modelBase;
            rootLogger?.LogInformation("🔁 Initializing ONNX service with CPU provider (fallback) using path {Path}", cpuPath);
            return new CustomOnnxChatCompletionService(
                modelId: llmConfig.ModelId,
                modelPath: cpuPath,
                executionProvider: "cpu",
                deviceId: deviceId,
                logger: serviceLogger
            );
        });

        // Register Semantic Kernel
        services.AddKernel();

        // Register LLM Service wrapper
        services.AddSingleton<LLMService>();

        return services;
    }

    public static IServiceCollection AddLLMServices(this IServiceCollection services, Action<LLMConfiguration> configureOptions)
    {
        var testConfig = new LLMConfiguration();
        configureOptions(testConfig);
        
        if (string.IsNullOrWhiteSpace(testConfig.BasePath))
        {
            throw new ArgumentException("BasePath is required in LLM configuration", nameof(configureOptions));
        }
        
        if (string.IsNullOrWhiteSpace(testConfig.ModelId))
        {
            throw new ArgumentException("ModelId is required in LLM configuration", nameof(configureOptions));
        }
        
        if (string.IsNullOrWhiteSpace(testConfig.ExecutionProvider))
        {
            throw new ArgumentException("ExecutionProvider is required in LLM configuration", nameof(configureOptions));
        }
        
        services.Configure(configureOptions);
        
        // Note: This approach requires an IConfiguration to be available in the DI container
        // if AddLLMServices(IConfiguration) is called internally.
        // For simplicity in this project, we assume the IConfiguration variant is the primary one.
        return services;
    }
}
