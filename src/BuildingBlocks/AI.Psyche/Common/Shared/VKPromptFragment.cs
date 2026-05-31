// [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents a raw, atomic text fragment extracted from a source with unified layout coordinates.
/// Follows AP.01 (Sealed Record) and minimizes heap allocations via flattening.
/// </summary>
public sealed record VKPromptFragment
{
    // --- 1. Sourcing Context ---
    public required VKPromptTierType TierType { get; init; }
    public required IVKFragmentMetadata Metadata { get; init; }
    public required VKChatRole Role { get; init; }
    public required int RenderOrder { get; set; }

    public int? Depth { get; init; } = null;

    public string Separator { get; init; } = "\n\n";

    public string? Content { get; init; }
}
