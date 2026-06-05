using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class DirectiveDiagnostics
{
    [LoggerMessage(
        EventId = VKDirectiveDiagnostic.DirectiveInitializedEventId,
        Level = LogLevel.Information,
        Message = "Directive provider initialized.")]
    public static partial void DirectiveInitialized(ILogger logger);

    [LoggerMessage(
        EventId = VKDirectiveDiagnostic.DirectiveResolvedEventId,
        Level = LogLevel.Information,
        Message = "Resolved Directive {DirectiveId}.")]
    public static partial void DirectiveResolved(ILogger logger, string directiveId);
}
