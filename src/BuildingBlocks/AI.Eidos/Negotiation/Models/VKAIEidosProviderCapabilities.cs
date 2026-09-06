using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public sealed record VKAIEidosProviderCapabilities
{
    public VKAIProviderType Provider { get; init; } = VKAIProviderType.OpenAI;
    public required string ModelId { get; init; }
    public bool SupportsNativeStructuredOutput { get; init; } = true;
}
