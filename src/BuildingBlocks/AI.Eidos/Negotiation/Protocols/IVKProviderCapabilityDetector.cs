using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKProviderCapabilityDetector
{
    VKAIEidosProviderCapabilities DetectCapabilities(string providerName, string modelId);
}
