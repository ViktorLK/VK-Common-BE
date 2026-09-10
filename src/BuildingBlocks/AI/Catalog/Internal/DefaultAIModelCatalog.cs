using System;
using System.Collections.Concurrent;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Catalog.Internal;

/// <summary>
/// Default model catalog preloaded with standard industry models via <see cref="VKAIModelIds"/>,
/// equipped with smart heuristic prefix fallback and runtime custom registration.
/// </summary>
internal sealed class DefaultAIModelCatalog : IVKVKAIModelCatalog
{
    private readonly ConcurrentDictionary<string, VKAIModelMetadata> _catalog = new(StringComparer.OrdinalIgnoreCase);

    public DefaultAIModelCatalog()
    {
        // 1. OpenAI (using VKAIModelIds.OpenAI)
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.OpenAI, ModelId = VKAIModelIds.OpenAI.Gpt4O, ContextWindowSize = 128000, MaxOutputTokens = 16384, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.OpenAI, ModelId = VKAIModelIds.OpenAI.Gpt4OMini, ContextWindowSize = 128000, MaxOutputTokens = 16384, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.OpenAI, ModelId = VKAIModelIds.OpenAI.Gpt4Turbo, ContextWindowSize = 128000, MaxOutputTokens = 4096, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.OpenAI, ModelId = VKAIModelIds.OpenAI.O1, ContextWindowSize = 200000, MaxOutputTokens = 100000, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.OpenAI, ModelId = VKAIModelIds.OpenAI.O1Mini, ContextWindowSize = 128000, MaxOutputTokens = 65536, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.OpenAI, ModelId = VKAIModelIds.OpenAI.O3Mini, ContextWindowSize = 200000, MaxOutputTokens = 100000, SupportsStructuredOutput = true });

        // 2. Anthropic (using VKAIModelIds.Anthropic)
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Anthropic, ModelId = VKAIModelIds.Anthropic.Claude35Sonnet, ContextWindowSize = 200000, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Anthropic, ModelId = VKAIModelIds.Anthropic.Claude35Haiku, ContextWindowSize = 200000, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Anthropic, ModelId = VKAIModelIds.Anthropic.Claude3Opus, ContextWindowSize = 200000, MaxOutputTokens = 4096, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Anthropic, ModelId = VKAIModelIds.Anthropic.Claude3Sonnet, ContextWindowSize = 200000, MaxOutputTokens = 4096 });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Anthropic, ModelId = VKAIModelIds.Anthropic.Claude3Haiku, ContextWindowSize = 200000, MaxOutputTokens = 4096 });

        // 3. Google Gemini (using VKAIModelIds.Google)
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Google, ModelId = VKAIModelIds.Google.Gemini20Flash, ContextWindowSize = 1048576, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Google, ModelId = VKAIModelIds.Google.Gemini20Pro, ContextWindowSize = 2097152, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Google, ModelId = VKAIModelIds.Google.Gemini15Pro, ContextWindowSize = 2097152, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Google, ModelId = VKAIModelIds.Google.Gemini15Flash, ContextWindowSize = 1048576, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Google, ModelId = VKAIModelIds.Google.Gemini15Flash8B, ContextWindowSize = 1048576, MaxOutputTokens = 8192, SupportsStructuredOutput = true });

        // 4. Common Open Weights / Local (DeepSeek, Llama3, Qwen)
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Custom, ModelId = "deepseek-chat", ContextWindowSize = 64000, MaxOutputTokens = 8192, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Custom, ModelId = "deepseek-reasoner", ContextWindowSize = 64000, MaxOutputTokens = 8192 });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Ollama, ModelId = "llama3.3:70b", ContextWindowSize = 128000, MaxOutputTokens = 8192 });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Ollama, ModelId = "llama3:70b", ContextWindowSize = 8192, MaxOutputTokens = 4096 });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Ollama, ModelId = "qwen2.5:72b", ContextWindowSize = 128000, MaxOutputTokens = 8192 });
    }

    public void Register(VKAIModelMetadata metadata)
    {
        VKGuard.NotNull(metadata);
        _catalog[BuildKey(metadata.Provider, metadata.ModelId)] = metadata;
        _catalog[metadata.ModelId] = metadata;
    }

    private static string BuildKey(VKAIProviderType provider, string modelId) => $"{provider}:{modelId}";

    public VKAIModelMetadata GetAIModelMetadata(VKAIProviderType provider, string modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return DefaultFallback with { Provider = provider };
        }

        // 1. Exact match by composite key (Provider + ModelId)
        if (_catalog.TryGetValue(BuildKey(provider, modelId), out var providerMetadata))
        {
            return providerMetadata;
        }

        // 2. Exact match from registered catalog by ModelId alone if matching provider
        if (_catalog.TryGetValue(modelId, out var metadata) && metadata.Provider == provider)
        {
            return metadata;
        }

        // 3. Intelligent Prefix/Heuristic Match for future unlisted variants
        if (provider == VKAIProviderType.Anthropic || modelId.StartsWith("claude-3", StringComparison.OrdinalIgnoreCase))
            return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 200000, MaxOutputTokens = 8192, SupportsStructuredOutput = true };

        if (provider == VKAIProviderType.Google || modelId.StartsWith("gemini", StringComparison.OrdinalIgnoreCase))
            return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 1048576, MaxOutputTokens = 8192, SupportsStructuredOutput = true };

        if (modelId.StartsWith("deepseek", StringComparison.OrdinalIgnoreCase))
            return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 64000, MaxOutputTokens = 8192, SupportsStructuredOutput = true };

        if (provider == VKAIProviderType.OpenAI || provider == VKAIProviderType.AzureOpenAI || modelId.StartsWith("gpt-4", StringComparison.OrdinalIgnoreCase) || modelId.StartsWith("o1", StringComparison.OrdinalIgnoreCase) || modelId.StartsWith("o3", StringComparison.OrdinalIgnoreCase))
        {
            if (modelId.StartsWith("o1", StringComparison.OrdinalIgnoreCase) || modelId.StartsWith("o3", StringComparison.OrdinalIgnoreCase))
                return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 200000, MaxOutputTokens = 100000, SupportsStructuredOutput = true };

            return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 128000, MaxOutputTokens = 16384, SupportsStructuredOutput = true };
        }

        // 4. Safe Conservative Baseline
        return new VKAIModelMetadata
        {
            Provider = provider,
            ModelId = modelId,
            ContextWindowSize = 8192,
            MaxOutputTokens = 2048
        };
    }

    private static readonly VKAIModelMetadata DefaultFallback = new()
    {
        Provider = VKAIProviderType.Custom,
        ModelId = "default",
        ContextWindowSize = 8192,
        MaxOutputTokens = 2048
    };
}
