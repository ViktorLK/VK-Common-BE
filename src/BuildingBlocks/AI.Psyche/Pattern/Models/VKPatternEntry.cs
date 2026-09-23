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
    /// Gets the layout coordinates and placement rules for this pattern entry.
    /// </summary>
    public VKPromptCoordinates Coordinates { get; private set; }

    /// <summary>
    /// Gets the prompt text payload and token count for this pattern entry.
    /// </summary>
    public VKPromptPayload Payload { get; private set; }


    // =========================================================================
    // Constructor (Private)
    // =========================================================================

    private VKPatternEntry(
        VKPatternId id,
        VKPromptCoordinates coordinates,
        VKPromptPayload payload,
        string? name = null) : base(id)
    {
        Coordinates = coordinates;
        Payload = payload;
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
        VKPromptCoordinates coordinates,
        VKPromptPayload payload,
        string? name = null)
    {
        // [AP.01]
        VKGuard.NotDefault(id);
        VKGuard.NotNull(coordinates);
        VKGuard.NotNull(payload);

        return VKResult.Success(new VKPatternEntry(id, coordinates, payload, name));
    }

    /// <summary>
    /// Factory method to create a new pattern entry aggregate root from a prompt segment.
    /// </summary>
    public static VKResult<VKPatternEntry> Create(
        VKPatternId id,
        VKPromptSegment segment,
        string? name = null)
    {
        VKGuard.NotNull(segment);
        return Create(id, segment.Coordinates, segment.Payload, name);
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKPatternEntry Rehydrate(
        VKPatternId id,
        VKPromptCoordinates coordinates,
        VKPromptPayload payload,
        string? name = null)
    {
        return new VKPatternEntry(id, coordinates, payload, name);
    }

    /// <summary>
    /// Rehydration factory overload from segment.
    /// </summary>
    internal static VKPatternEntry Rehydrate(
        VKPatternId id,
        VKPromptSegment segment,
        string? name = null)
    {
        return new VKPatternEntry(id, segment.Coordinates, segment.Payload, name);
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
    /// Updates the layout coordinates for this pattern entry.
    /// </summary>
    public VKResult UpdateCoordinates(VKPromptCoordinates coordinates)
    {
        Coordinates = VKGuard.NotNull(coordinates);
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the text payload for this pattern entry.
    /// </summary>
    public VKResult UpdatePayload(VKPromptPayload payload)
    {
        Payload = VKGuard.NotNull(payload);
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the prompt segment (both coordinates and payload).
    /// </summary>
    public VKResult UpdateSegment(VKPromptSegment segment)
    {
        VKGuard.NotNull(segment);
        Coordinates = segment.Coordinates;
        Payload = segment.Payload;
        return VKResult.Success();
    }

    /// <summary>
    /// Updates the precalculated token count for this pattern entry.
    /// </summary>
    public VKResult UpdateTokenCount(int tokenCount)
    {
        if (Payload is not null)
        {
            Payload = Payload with { TokenCount = Math.Max(0, tokenCount) };
        }

        return VKResult.Success();
    }
}
