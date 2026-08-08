using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public sealed record VKAIEidosProviderCapabilities
{
    public required string ProviderName { get; init; }
    public required string ModelId { get; init; }
    public bool SupportsNativeStructuredOutput { get; init; } = true;
    public bool SupportsToolCalling { get; init; } = true;
}
