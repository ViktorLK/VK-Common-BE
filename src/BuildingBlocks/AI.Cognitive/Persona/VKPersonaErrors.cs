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
}
