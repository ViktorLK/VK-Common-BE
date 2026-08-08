using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Negotiation.Internal;

internal sealed class BasicProviderCapabilityDetector : IVKProviderCapabilityDetector
{
    public VKAIEidosProviderCapabilities DetectCapabilities(string providerName, string modelId)
    {
        VKGuard.NotNullOrWhiteSpace(providerName);
        VKGuard.NotNullOrWhiteSpace(modelId);

        var isOllamaOrLegacy = providerName.Contains("Ollama", StringComparison.OrdinalIgnoreCase);

        return new VKAIEidosProviderCapabilities
        {
            ProviderName = providerName,
            ModelId = modelId,
            SupportsNativeStructuredOutput = !isOllamaOrLegacy,
            SupportsToolCalling = !isOllamaOrLegacy
        };
    }
}
