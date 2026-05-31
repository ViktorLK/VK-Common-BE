using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class PersonaDiagnostics
{
    [LoggerMessage(
        EventId = VKPersonaDiagnostics.PersonaResolvedEventId,
        Level = LogLevel.Information,
        Message = "Persona resolved from store. PersonaId: {PersonaId}, Name: {Name}")]
    public static partial void PersonaResolved(ILogger logger, string personaId, string name);

    [LoggerMessage(
        EventId = VKPersonaDiagnostics.PersonaRenderedEventId,
        Level = LogLevel.Information,
        Message = "Persona rendered successfully. PersonaId: {PersonaId}, RenderedLength: {Length}")]
    public static partial void PersonaRendered(ILogger logger, string personaId, int length);
}
