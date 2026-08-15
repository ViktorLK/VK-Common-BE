using System;
using System.Collections.Concurrent;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Internal;

/// <summary>
/// Default model catalog preloaded with standard industry models via <see cref="VKAIModelIds"/>,
/// equipped with smart heuristic prefix fallback and runtime custom registration.
/// </summary>
internal sealed class DefaultModelCatalog : IVKModelCatalog
{
    private readonly ConcurrentDictionary<string, VKModelMetadata> _catalog = new(StringComparer.OrdinalIgnoreCase);

    public DefaultModelCatalog()
    {
        // 1. OpenAI (using VKAIModelIds.OpenAI)
        Register(new VKModelMetadata { ModelId = VKAIModelIds.OpenAI.Gpt4O, ContextWindowSize = 128000, MaxOutputTokens = 16384, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.OpenAI.Gpt4OMini, ContextWindowSize = 128000, MaxOutputTokens = 16384, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.OpenAI.Gpt4Turbo, ContextWindowSize = 128000, MaxOutputTokens = 4096, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.OpenAI.O1, ContextWindowSize = 200000, MaxOutputTokens = 100000, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.OpenAI.O1Mini, ContextWindowSize = 128000, MaxOutputTokens = 65536, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.OpenAI.O3Mini, ContextWindowSize = 200000, MaxOutputTokens = 100000, SupportsStructuredOutput = true });

        // 2. Anthropic (using VKAIModelIds.Anthropic)
        Register(new VKModelMetadata { ModelId = VKAIModelIds.Anthropic.Claude35Sonnet, ContextWindowSize = 200000, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.Anthropic.Claude35Haiku, ContextWindowSize = 200000, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.Anthropic.Claude3Opus, ContextWindowSize = 200000, MaxOutputTokens = 4096, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.Anthropic.Claude3Sonnet, ContextWindowSize = 200000, MaxOutputTokens = 4096 });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.Anthropic.Claude3Haiku, ContextWindowSize = 200000, MaxOutputTokens = 4096 });

        // 3. Google Gemini (using VKAIModelIds.Google)
        Register(new VKModelMetadata { ModelId = VKAIModelIds.Google.Gemini20Flash, ContextWindowSize = 1048576, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.Google.Gemini20Pro, ContextWindowSize = 2097152, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.Google.Gemini15Pro, ContextWindowSize = 2097152, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.Google.Gemini15Flash, ContextWindowSize = 1048576, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = VKAIModelIds.Google.Gemini15Flash8B, ContextWindowSize = 1048576, MaxOutputTokens = 8192, SupportsStructuredOutput = true });

        // 4. Common Open Weights / Local (DeepSeek, Llama3, Qwen)
        Register(new VKModelMetadata { ModelId = "deepseek-chat", ContextWindowSize = 64000, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKModelMetadata { ModelId = "deepseek-reasoner", ContextWindowSize = 64000, MaxOutputTokens = 8192 });
        Register(new VKModelMetadata { ModelId = "llama3.3:70b", ContextWindowSize = 128000, MaxOutputTokens = 8192 });
        Register(new VKModelMetadata { ModelId = "llama3:70b", ContextWindowSize = 8192, MaxOutputTokens = 4096 });
        Register(new VKModelMetadata { ModelId = "qwen2.5:72b", ContextWindowSize = 128000, MaxOutputTokens = 8192 });
    }

    public void Register(VKModelMetadata metadata)
    {
        VKGuard.NotNull(metadata);
        _catalog[metadata.ModelId] = metadata;
    }

    public VKModelMetadata GetModelMetadata(string modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return DefaultFallback;
        }

        // 1. Exact match from registered catalog
        if (_catalog.TryGetValue(modelId, out var metadata))
        {
            return metadata;
        }

        // 2. Intelligent Prefix/Heuristic Match for future unlisted variants
        if (modelId.StartsWith("gpt-4", StringComparison.OrdinalIgnoreCase))
            return new VKModelMetadata { ModelId = modelId, ContextWindowSize = 128000, MaxOutputTokens = 16384, SupportsStructuredOutput = true };
        if (modelId.StartsWith("o1", StringComparison.OrdinalIgnoreCase) || modelId.StartsWith("o3", StringComparison.OrdinalIgnoreCase))
            return new VKModelMetadata { ModelId = modelId, ContextWindowSize = 200000, MaxOutputTokens = 100000, SupportsStructuredOutput = true };
        if (modelId.StartsWith("claude-3", StringComparison.OrdinalIgnoreCase))
            return new VKModelMetadata { ModelId = modelId, ContextWindowSize = 200000, MaxOutputTokens = 8192, SupportsStructuredOutput = true };
        if (modelId.StartsWith("gemini", StringComparison.OrdinalIgnoreCase))
            return new VKModelMetadata { ModelId = modelId, ContextWindowSize = 1048576, MaxOutputTokens = 8192, SupportsStructuredOutput = true };
        if (modelId.StartsWith("deepseek", StringComparison.OrdinalIgnoreCase))
            return new VKModelMetadata { ModelId = modelId, ContextWindowSize = 64000, MaxOutputTokens = 8192, SupportsStructuredOutput = true };

        // 3. Safe Conservative Baseline
        return new VKModelMetadata
        {
            ModelId = modelId,
            ContextWindowSize = 8192,
            MaxOutputTokens = 2048
        };
    }

    private static readonly VKModelMetadata DefaultFallback = new()
    {
        ModelId = "default",
        ContextWindowSize = 8192,
        MaxOutputTokens = 2048
    };
}
