using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Standard error constants for the Persona feature.
/// </summary>
public static class VKPersonaErrors
{
    /// <summary>
    /// Error returned when the persona is not found.
    /// </summary>
    public static readonly VKError NotFound = new("AI.Persona.NotFound", "The requested persona was not found.");

    /// <summary>
    /// Error returned when the persona already exists.
    /// </summary>
    // [CS.01]
    public static readonly VKError AlreadyExists = new("AI.Persona.AlreadyExists", "The persona already exists.");

    public static readonly VKError InvalidMetadataType = new("AI.Persona.InvalidMetadataType", "The metadata provided to the formatter was not of the expected Persona type.");
    
    public static readonly VKError FormattingFailed = new("AI.Persona.FormattingFailed", "An error occurred while formatting the persona anchor.");
}
