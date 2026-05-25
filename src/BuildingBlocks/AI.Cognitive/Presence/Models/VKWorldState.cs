using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents the physical and situational environmental context (World State) captured in the Presence window.
/// Follows AP.01 (sealed record with required properties) and AP.03.
/// </summary>
public sealed record VKWorldState
{
    /// <summary>
    /// Gets a default unknown world state fallback.
    /// </summary>
    public static VKWorldState Default { get; } = new()
    {
        Location = "Unknown",
        UserActivity = "Unknown",
        AmbientTags = []
    };

    /// <summary>
    /// Gets the geographic location or type of environment (e.g., "Office", "Late Night Roadside", "Home").
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// Gets the user's current activity or state of interaction (e.g., "Working", "Walking", "Coding").
    /// </summary>
    public required string UserActivity { get; init; }

    /// <summary>
    /// Gets any additional ambient or situational tags (e.g., "Quiet", "Noisy", "Rainy").
    /// </summary>
    public IReadOnlyList<string> AmbientTags { get; init; } = [];
}
