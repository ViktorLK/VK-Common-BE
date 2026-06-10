using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Standard error constants for the Pattern feature.
/// </summary>
public static class VKPatternErrors
{
    /// <summary>
    /// Error returned when the Pattern is not found.
    /// </summary>
    public static readonly VKError NotFound = new("AI.Pattern.NotFound", "The requested Pattern was not found.");

    /// <summary>
    /// Error returned when the Pattern already exists.
    /// </summary>
    public static readonly VKError AlreadyExists = new("AI.Pattern.AlreadyExists", "The requested Pattern already exists.");

    /// <summary>
    /// Error returned when metadata does not match Pattern type.
    /// </summary>
    public static readonly VKError InvalidMetadataType = new("AI.Pattern.InvalidMetadataType", "The metadata provided to the formatter was not of the expected Pattern type.");
}
