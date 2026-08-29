using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain aggregate root representing a customizable pattern or prompt injection node in the weaving pipeline.
/// Follows AP.01, CS.01.
/// </summary>
public sealed class VKPatternEntry : VKAggregateRoot<VKPatternId>, IVKFragmentMetadata
{
    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>
    /// Gets the prompt segment definition and text content for this pattern entry.
    /// </summary>
    public VKPromptSegment Segment { get; private set; }

    // =========================================================================
    // Constructor (Private)
    // =========================================================================

    private VKPatternEntry(
        VKPatternId id,
        VKPromptSegment segment) : base(id)
    {
        Segment = segment;
    }

    // =========================================================================
    // Factory Methods
    // =========================================================================

    /// <summary>
    /// Factory method to create a new pattern entry aggregate root.
    /// </summary>
    public static VKResult<VKPatternEntry> Create(
        VKPatternId id,
        VKPromptSegment segment)
    {
        // [AP.01]
        VKGuard.NotDefault(id);
        VKGuard.NotNull(segment);

        return VKResult.Success(new VKPatternEntry(id, segment));
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKPatternEntry Rehydrate(
        VKPatternId id,
        VKPromptSegment segment)
    {
        return new VKPatternEntry(id, segment);
    }

    // =========================================================================
    // Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Updates the prompt segment content and layout coordinates.
    /// </summary>
    public VKResult UpdateSegment(VKPromptSegment segment)
    {
        Segment = VKGuard.NotNull(segment);
        return VKResult.Success();
    }
}
