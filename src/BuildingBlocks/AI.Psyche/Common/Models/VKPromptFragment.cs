using VK.Blocks.AI.Psyche.Common.Internal;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents a raw, atomic text fragment extracted from a source with unified layout coordinates.
/// </summary>
public sealed class VKPromptFragment
{
    /// <summary>
    /// Gets the semantic layout tier defining where this fragment belongs in the final prompt.
    /// </summary>
    public required VKPromptTierType TierType { get; set; }

    /// <summary>
    /// Gets the metadata associated with this fragment.
    /// </summary>
    public required IVKFragmentMetadata Metadata { get; set; }

    /// <summary>
    /// Gets or sets the prompt layout segment defining layout strategy and content.
    /// </summary>
    public required VKPromptSegment Segment { get; set; }

    /// <summary>
    /// Gets or sets the absolute sort order of this fragment within its tier.
    /// </summary>
    public int? RenderOrder { get; set; }

    /// <summary>
    /// Gets the string used to separate this fragment from the next when flattening.
    /// </summary>
    public string Separator { get; set; } = PsycheConstants.Separators.DefaultSegment;
}
