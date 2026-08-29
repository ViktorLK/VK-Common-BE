using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain error constants for the Profile feature slice.
/// Follows CS.01.
/// </summary>
public static class VKProfileErrors
{
    /// <summary>
    /// Error returned when the user profile was not found.
    /// </summary>
    public static readonly VKError NotFound = new("AI.Profile.NotFound", "The requested profile was not found.");
}
