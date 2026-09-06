using System;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

/// <summary>
/// Capability detector inspecting Provider, Model identifiers, and configurable override policies.
/// Supports both strongly-typed <see cref="VKAIProviderType"/> and string-based fallbacks.
/// </summary>
internal sealed class BasicProviderCapabilityDetector(
    IOptions<VKNegotiationOptions>? options = null) : IVKProviderCapabilityDetector
{
    private readonly VKNegotiationOptions _options = options?.Value ?? new VKNegotiationOptions();

    public VKAIEidosProviderCapabilities DetectCapabilities(VKAIProviderType provider, string modelId)
    {
        VKGuard.NotNullOrWhiteSpace(modelId);

        // 1. Check explicit user overrides
        var specificKey = $"{provider}:{modelId}";
        if (_options.ProviderCapabilitiesOverrides.TryGetValue(specificKey, out var specificOverride))
        {
            return specificOverride;
        }

        var providerKey = provider.ToString();
        if (_options.ProviderCapabilitiesOverrides.TryGetValue(providerKey, out var providerOverride))
        {
            return providerOverride;
        }

        // 2. Strongly-typed capability heuristics
        var m = modelId.ToLowerInvariant();

        return provider switch
        {
            VKAIProviderType.OpenAI or VKAIProviderType.AzureOpenAI =>
                m.Contains("instruct") || m.Contains("davinci") || m.Contains("gpt-3.5")
                    ? Create(provider, modelId, nativeStructured: false)
                    : Create(provider, modelId, nativeStructured: true),

            VKAIProviderType.Anthropic =>
                m.Contains("claude-2") || m.Contains("claude-instant")
                    ? Create(provider, modelId, nativeStructured: false)
                    : Create(provider, modelId, nativeStructured: true),

            VKAIProviderType.Google =>
                m.Contains("gemini-1.0") || m.Contains("vision")
                    ? Create(provider, modelId, nativeStructured: false)
                    : Create(provider, modelId, nativeStructured: true),

            VKAIProviderType.Ollama =>
                Create(provider, modelId, nativeStructured: false),

            _ =>
                // Custom or Third-Party Provider (e.g. DeepSeek-R1 reasoning models)
                m.Contains("reasoner") || m.Contains("r1")
                    ? Create(provider, modelId, nativeStructured: false)
                    : Create(provider, modelId, nativeStructured: true)
        };
    }

    private static VKAIEidosProviderCapabilities Create(VKAIProviderType provider, string model, bool nativeStructured)
    {
        return new VKAIEidosProviderCapabilities
        {
            Provider = provider,
            ModelId = model,
            SupportsNativeStructuredOutput = nativeStructured
        };
    }
}
