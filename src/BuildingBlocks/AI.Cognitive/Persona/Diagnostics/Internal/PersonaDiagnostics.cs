using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Persona.Diagnostics.Internal;

[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class PersonaDiagnostics
{
    [LoggerMessage(
        EventId = VKPersonaDiagnosticTokens.PersonaLoadedEventId,
        Level = LogLevel.Information,
        Message = "Persona loaded: {Name} ({Id})")]
    public static partial void PersonaLoaded(ILogger logger, string name, string id);
}
