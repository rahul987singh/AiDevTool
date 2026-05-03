using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntimeGenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DevTeamAI.LLM.Services;

public class CustomOnnxChatCompletionService : IChatCompletionService, IDisposable
{
    private readonly Model _model;
    private readonly Tokenizer _tokenizer;
    private readonly string _modelId;
    private readonly ILogger<CustomOnnxChatCompletionService>? _logger;
    private bool _disposed;

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public CustomOnnxChatCompletionService(string modelId, string modelPath, string executionProvider = "cpu", int deviceId = 0, ILogger<CustomOnnxChatCompletionService>? logger = null)
    {
        _modelId = modelId;
        _logger = logger;

        // Basic validation for model files
        try
        {
            if (string.IsNullOrWhiteSpace(modelPath))
            {
                throw new FileNotFoundException("Model path is empty");
            }

            var modelFile = Path.Combine(modelPath, "model.onnx");
            if (!File.Exists(modelFile))
            {
                throw new FileNotFoundException($"Required model file not found: {modelFile}");
            }

            var config = new Config(modelPath);
            config.ClearProviders();
            if (string.Equals(executionProvider, "dml", StringComparison.OrdinalIgnoreCase))
            {
                config.AppendProvider("dml");
                config.SetProviderOption("dml", "device_id", deviceId.ToString());
                _logger?.LogInformation("Attempting to load model with DirectML provider (device {DeviceId})", deviceId);
            }
            else
            {
                _logger?.LogInformation("Loading model with CPU provider");
            }

            _model = new Model(config);
            _tokenizer = new Tokenizer(_model);
            _logger?.LogInformation("Model loaded successfully: {ModelId}", _modelId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize ONNX model ({ModelId}) with provider {Provider}", modelId, executionProvider);
            throw;
        }
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(chatHistory);
        using var sequences = _tokenizer.Encode(prompt);

        using var generatorParams = new GeneratorParams(_model);
        generatorParams.SetSearchOption("max_length", 2048);
        
        using var generator = new Generator(_model, generatorParams);
        generator.AppendTokenSequences(sequences);
        
        var responseText = "";
        using var tokenizerStream = _tokenizer.CreateStream();

        await Task.Run(() =>
        {
            while (!generator.IsDone())
            {
                if (cancellationToken.IsCancellationRequested) break;
                
                generator.GenerateNextToken();
                var nextToken = generator.GetSequence(0)[^1];
                var decoded = tokenizerStream.Decode(nextToken);
                if (!string.IsNullOrEmpty(decoded))
                {
                    responseText += decoded;
                }
            }
        }, cancellationToken);

        return new List<ChatMessageContent>
        {
            new ChatMessageContent(AuthorRole.Assistant, responseText, modelId: _modelId)
        };
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(chatHistory);
        using var sequences = _tokenizer.Encode(prompt);

        using var generatorParams = new GeneratorParams(_model);
        generatorParams.SetSearchOption("max_length", 2048);

        using var generator = new Generator(_model, generatorParams);
        generator.AppendTokenSequences(sequences);
        
        using var tokenizerStream = _tokenizer.CreateStream();

        while (!generator.IsDone())
        {
            if (cancellationToken.IsCancellationRequested) break;

            string? text = null;
            await Task.Run(() =>
            {
                generator.GenerateNextToken();
                var nextToken = generator.GetSequence(0)[^1];
                text = tokenizerStream.Decode(nextToken);
            }, cancellationToken);

            if (!string.IsNullOrEmpty(text))
            {
                yield return new StreamingChatMessageContent(AuthorRole.Assistant, text, modelId: _modelId);
            }
        }
    }

    private string BuildPrompt(ChatHistory chatHistory)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("<|begin_of_text|>");
        foreach (var message in chatHistory)
        {
            var role = message.Role == AuthorRole.System ? "system" : 
                       message.Role == AuthorRole.User ? "user" : "assistant";
            sb.Append($"<|start_header_id|>{role}<|end_header_id|>\n\n{message.Content}<|eot_id|>");
        }
        sb.Append("<|start_header_id|>assistant<|end_header_id|>\n\n");
        return sb.ToString();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (_tokenizer is IDisposable td)
            {
                td.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Exception while disposing tokenizer");
        }

        try
        {
            if (_model is IDisposable md)
            {
                md.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Exception while disposing model");
        }
    }
}
