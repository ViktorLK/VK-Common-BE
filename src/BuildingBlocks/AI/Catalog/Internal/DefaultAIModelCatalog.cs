using System;
using System.Collections.Concurrent;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Catalog.Internal;

/// <summary>
/// Default model catalog preloaded with standard industry models via <see cref="VKAIModelIds"/>,
/// equipped with smart heuristic prefix fallback and runtime custom registration.
/// </summary>
internal sealed class DefaultAIModelCatalog : IVKAIModelCatalog
{
    private readonly ConcurrentDictionary<string, VKAIModelMetadata> _catalog = new(StringComparer.OrdinalIgnoreCase);

    public DefaultAIModelCatalog()
    {
        // 1. OpenAI (using VKAIModelIds.OpenAI)
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.OpenAI, ModelId = VKAIModelIds.OpenAI.Gpt6Astra, ContextWindowSize = 1048576, MaxOutputTokens = 131072, SupportsStructuredOutput = true, SupportsReasoning = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.OpenAI, ModelId = VKAIModelIds.OpenAI.Gpt61Sol, ContextWindowSize = 1048576, MaxOutputTokens = 131072, SupportsStructuredOutput = true, SupportsReasoning = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.OpenAI, ModelId = VKAIModelIds.OpenAI.Gpt6Sol, ContextWindowSize = 1048576, MaxOutputTokens = 131072, SupportsStructuredOutput = true, SupportsReasoning = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.OpenAI, ModelId = VKAIModelIds.OpenAI.Gpt6Luna, ContextWindowSize = 1048576, MaxOutputTokens = 131072, SupportsStructuredOutput = true, SupportsReasoning = true });

        // 2. Anthropic (using VKAIModelIds.Anthropic)
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Anthropic, ModelId = VKAIModelIds.Anthropic.ClaudeFable52, ContextWindowSize = 1048576, MaxOutputTokens = 131072, SupportsStructuredOutput = true, SupportsReasoning = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Anthropic, ModelId = VKAIModelIds.Anthropic.ClaudeOpus55, ContextWindowSize = 1048576, MaxOutputTokens = 131072, SupportsStructuredOutput = true, SupportsReasoning = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Anthropic, ModelId = VKAIModelIds.Anthropic.ClaudeSonnet55, ContextWindowSize = 1048576, MaxOutputTokens = 131072, SupportsStructuredOutput = true, SupportsReasoning = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Anthropic, ModelId = VKAIModelIds.Anthropic.ClaudeHaiku55, ContextWindowSize = 1048576, MaxOutputTokens = 131072, SupportsStructuredOutput = true, SupportsReasoning = true });

        // 3. Google Gemini (using VKAIModelIds.Google)
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Google, ModelId = VKAIModelIds.Google.Gemini4Argon, ContextWindowSize = 1048576, MaxOutputTokens = 65536, SupportsStructuredOutput = true, SupportsReasoning = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Google, ModelId = VKAIModelIds.Google.Gemini38Flash, ContextWindowSize = 1048576, MaxOutputTokens = 65536, SupportsStructuredOutput = true, SupportsReasoning = true, ReasoningEffortLevels = ["low", "medium", "high"] });

        // 4. xAI (using VKAIModelIds.XAI)
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.XAI, ModelId = VKAIModelIds.XAI.Grok47, ContextWindowSize = 500000, MaxOutputTokens = 65536, SupportsStructuredOutput = true, SupportsReasoning = true });

        // 5. DeepSeek (using VKAIModelIds.DeepSeek)
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.DeepSeek, ModelId = VKAIModelIds.DeepSeek.V4Pro, ContextWindowSize = 1048576, MaxOutputTokens = 393216, SupportsStructuredOutput = true, SupportsReasoning = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.DeepSeek, ModelId = VKAIModelIds.DeepSeek.V4Flash, ContextWindowSize = 1048576, MaxOutputTokens = 393216, SupportsStructuredOutput = true, SupportsReasoning = true });

        // 6. Mistral (using VKAIModelIds.Mistral)
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Mistral, ModelId = VKAIModelIds.Mistral.Large4, ContextWindowSize = 256000, MaxOutputTokens = 32768, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Mistral, ModelId = VKAIModelIds.Mistral.Large3, ContextWindowSize = 256000, MaxOutputTokens = 32768, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Mistral, ModelId = VKAIModelIds.Mistral.Medium35, ContextWindowSize = 256000, MaxOutputTokens = 32768, SupportsStructuredOutput = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Mistral, ModelId = VKAIModelIds.Mistral.Small4, ContextWindowSize = 256000, MaxOutputTokens = 32768, SupportsStructuredOutput = true });

        // 7. Open Weights / Local (Ollama)
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Ollama, ModelId = "qwen3.5:32b", ContextWindowSize = 262144, MaxOutputTokens = 16384 });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Ollama, ModelId = "gemma4:27b", ContextWindowSize = 131072, MaxOutputTokens = 16384 });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Ollama, ModelId = "gpt-oss:120b", ContextWindowSize = 131072, MaxOutputTokens = 16384, SupportsReasoning = true });
        Register(new VKAIModelMetadata { Provider = VKAIProviderType.Ollama, ModelId = "gpt-oss:20b", ContextWindowSize = 131072, MaxOutputTokens = 16384, SupportsReasoning = true });
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
        if (provider == VKAIProviderType.Anthropic || modelId.StartsWith("claude-", StringComparison.OrdinalIgnoreCase))
        {
            return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 1048576, MaxOutputTokens = 131072, SupportsStructuredOutput = true, SupportsReasoning = true };
        }

        if (provider == VKAIProviderType.Google || modelId.StartsWith("gemini-", StringComparison.OrdinalIgnoreCase))
        {
            return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 1048576, MaxOutputTokens = 65536, SupportsStructuredOutput = true, SupportsReasoning = true };
        }

        if (provider == VKAIProviderType.DeepSeek || modelId.StartsWith("deepseek-", StringComparison.OrdinalIgnoreCase))
        {
            return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 1048576, MaxOutputTokens = 393216, SupportsStructuredOutput = true, SupportsReasoning = true };
        }

        if (provider == VKAIProviderType.XAI || modelId.StartsWith("grok-", StringComparison.OrdinalIgnoreCase))
        {
            return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 500000, MaxOutputTokens = 65536, SupportsStructuredOutput = true, SupportsReasoning = true };
        }

        if (provider == VKAIProviderType.Mistral || modelId.StartsWith("mistral", StringComparison.OrdinalIgnoreCase) || modelId.StartsWith("ministral", StringComparison.OrdinalIgnoreCase))
        {
            return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 256000, MaxOutputTokens = 32768, SupportsStructuredOutput = true };
        }

        if (provider == VKAIProviderType.OpenAI || provider == VKAIProviderType.AzureOpenAI || modelId.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase) || modelId.StartsWith("o1", StringComparison.OrdinalIgnoreCase) || modelId.StartsWith("o3", StringComparison.OrdinalIgnoreCase) || modelId.StartsWith("o4", StringComparison.OrdinalIgnoreCase))
        {
            if (modelId.StartsWith("gpt-6", StringComparison.OrdinalIgnoreCase))
            {
                return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 1048576, MaxOutputTokens = 131072, SupportsStructuredOutput = true, SupportsReasoning = true };
            }

            if (modelId.StartsWith("o1", StringComparison.OrdinalIgnoreCase) || modelId.StartsWith("o3", StringComparison.OrdinalIgnoreCase) || modelId.StartsWith("o4", StringComparison.OrdinalIgnoreCase))
            {
                return new VKAIModelMetadata { Provider = provider, ModelId = modelId, ContextWindowSize = 200000, MaxOutputTokens = 100000, SupportsStructuredOutput = true, SupportsReasoning = true };
            }

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
