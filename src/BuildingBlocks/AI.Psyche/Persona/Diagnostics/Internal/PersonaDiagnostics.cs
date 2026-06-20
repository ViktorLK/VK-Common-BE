using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class PersonaDiagnostics
{
    [LoggerMessage(
        EventId = VKPersonaDiagnosticsConstants.Logs.PersonaResolved,
        Level = LogLevel.Debug,
        Message = "Persona anchor resolved: {PersonaId} ({Name})")]
    public static partial void PersonaResolved(this ILogger logger, VKPersonaId personaId, string name);

    [LoggerMessage(
        EventId = VKPersonaDiagnosticsConstants.Logs.PersonaRendered,
        Level = LogLevel.Debug,
        Message = "Persona system prompt rendered for {PersonaId} ({Length} chars)")]
    public static partial void PersonaRendered(this ILogger logger, VKPersonaId personaId, int length);
}
