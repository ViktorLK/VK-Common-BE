using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain aggregate root representing a customizable pattern or prompt injection node in the weaving pipeline.
/// Follows AP.01, CS.01.
/// </summary>
public sealed class VKPatternEntry : VKAggregateRoot<VKPatternId>
{
    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>
    /// Gets the human-readable name of this pattern entry.
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// Gets the prompt segment definition and text content for this pattern entry.
    /// </summary>
    public VKPromptSegment Segment { get; private set; }

    // =========================================================================
    // Constructor (Private)
    // =========================================================================

    private VKPatternEntry(
        VKPatternId id,
        VKPromptSegment segment,
        string? name = null) : base(id)
    {
        Segment = segment;
        Name = name;
    }

    // =========================================================================
    // Factory Methods
    // =========================================================================

    /// <summary>
    /// Factory method to create a new pattern entry aggregate root.
    /// </summary>
    public static VKResult<VKPatternEntry> Create(
        VKPatternId id,
        VKPromptSegment segment,
        string? name = null)
    {
        // [AP.01]
        VKGuard.NotDefault(id);
        VKGuard.NotNull(segment);

        return VKResult.Success(new VKPatternEntry(id, segment, name));
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKPatternEntry Rehydrate(
        VKPatternId id,
        VKPromptSegment segment,
        string? name = null)
    {
        return new VKPatternEntry(id, segment, name);
    }

    // =========================================================================
    // Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Updates the name of this pattern entry.
    /// </summary>
    public VKResult UpdateName(string? name)
    {
        Name = name;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the prompt segment content and layout coordinates.
    /// </summary>
    public VKResult UpdateSegment(VKPromptSegment segment)
    {
        Segment = VKGuard.NotNull(segment);
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the precalculated token count for this pattern entry's segment.
    /// </summary>
    public VKResult UpdateTokenCount(int tokenCount)
    {
        if (Segment is not null)
        {
            Segment = Segment with { TokenCount = Math.Max(0, tokenCount) };
        }
        return VKResult.Success();
    }
}
