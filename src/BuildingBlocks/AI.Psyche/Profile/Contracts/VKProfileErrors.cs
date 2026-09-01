using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Standard error constants for the Profile feature.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static error definitions and constant descriptors.")]
public static class VKProfileErrors
{
    /// <summary>
    /// Error returned when the user profile was not found.
    /// </summary>
    public static readonly VKError NotFound = new("AI.Profile.NotFound", "The requested profile was not found.");
}
