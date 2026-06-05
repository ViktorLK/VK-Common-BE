using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class PersonaDiagnostics
{
    [LoggerMessage(200, LogLevel.Debug, "Persona anchor resolved: {PersonaId} ({Name})")]
    public static partial void PersonaResolved(ILogger logger, VKPersonaId personaId, string name);

    [LoggerMessage(201, LogLevel.Debug, "Persona system prompt rendered for {PersonaId} ({Length} chars)")]
    public static partial void PersonaRendered(ILogger logger, VKPersonaId personaId, int length);
}
