using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Options for Eidos Streaming Feature slice.
/// Governed by [BB.05] & [BB.07].
/// </summary>
public sealed partial record VKStreamingOptions : IVKBlockOptions // [AP.01]
{
    public bool EnableSpeculativeClosing { get; init; } = true;
}
