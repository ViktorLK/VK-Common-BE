using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKProviderCapabilityDetector
{
    VKAIEidosProviderCapabilities DetectCapabilities(VKAIProviderType provider, string modelId);
}
