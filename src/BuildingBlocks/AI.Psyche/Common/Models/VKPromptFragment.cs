using VK.Blocks.AI.Psyche.Weaving.Internal;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents a raw, atomic text fragment extracted from a source with unified layout coordinates.
/// Follows AP.01 (Sealed Record) and minimizes heap allocations via flattening.
/// </summary>
public sealed record VKPromptFragment
{
    /// <summary>
    /// Gets the semantic layout tier defining where this fragment belongs in the final prompt.
    /// </summary>
    public required VKPromptTierType TierType { get; init; }

    /// <summary>
    /// Gets the metadata associated with this fragment.
    /// </summary>
    public required IVKFragmentMetadata Metadata { get; init; }

    /// <summary>
    /// Gets the chat role (e.g., System, User, Assistant) under which this fragment is presented.
    /// </summary>
    public required VKChatRole Role { get; init; }

    /// <summary>
    /// Gets or sets the absolute sort order of this fragment within its tier.
    /// </summary>
    public required int RenderOrder { get; set; }

    /// <summary>
    /// Gets the hierarchical depth or nesting level of this fragment, if applicable.
    /// </summary>
    public int? Depth { get; init; } = null;

    /// <summary>
    /// Gets the string used to separate this fragment from the next when flattening.
    /// </summary>
    public string Separator { get; init; } = PsycheConstants.Separators.DefaultSegment;

    /// <summary>
    /// Gets the raw text content of the fragment.
    /// </summary>
    public string? Content { get; init; }
}
