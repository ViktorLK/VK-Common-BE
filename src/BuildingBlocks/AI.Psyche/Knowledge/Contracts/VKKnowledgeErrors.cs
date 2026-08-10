using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Standard error constants for the Knowledge feature.
/// </summary>
public static class VKKnowledgeErrors
{
    /// <summary>
    /// Error returned when the Knowledge is not found.
    /// </summary>
    public static readonly VKError NotFound = new("AI.Knowledge.NotFound", "The requested Knowledge was not found.");

    public static readonly VKError AlreadyExists = new("AI.Knowledge.AlreadyExists", "The requested Knowledge was already exists.");
    public static readonly VKError InvalidMetadataType = new("AI.Knowledge.InvalidMetadataType", "The metadata provided to the formatter was not of the expected Knowledge type.");
    public static readonly VKError MissingPersona = new("AI.Knowledge.MissingPersona", "PersonaId is required.");
}
