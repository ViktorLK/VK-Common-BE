using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Cognitive.Persona.Internal;

/// <summary>
/// Source-generated logger messages for the Persona feature.
/// </summary>
internal static partial class PersonaLog
{
    [LoggerMessage(
        EventId = 400,
        Level = LogLevel.Information,
        Message = "Persona loaded: {Name} ({Id})")]
    public static partial void PersonaLoaded(ILogger logger, string name, string id);
}
