using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Standard error constants for the Directive feature.
/// </summary>
public static class VKDirectiveErrors
{
    /// <summary>
    /// Error returned when the directive is not found.
    /// </summary>
    public static readonly VKError NotFound = new("AI.Directive.NotFound", "The requested directive was not found.");

    /// <summary>
    /// Error returned when the directive already exists.
    /// </summary>
    // [CS.01]
    public static readonly VKError AlreadyExists = new("AI.Directive.AlreadyExists", "The directive already exists.");

    public static readonly VKError InvalidMetadataType = new("AI.Directive.InvalidMetadataType", "The metadata provided to the formatter was not of the expected Directive type.");

    public static readonly VKError FormattingFailed = new("AI.Directive.FormattingFailed", "An error occurred while formatting the directive anchor.");
}
