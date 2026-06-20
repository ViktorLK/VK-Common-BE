using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class DirectiveDiagnostics
{
    [LoggerMessage(
        EventId = VKDirectiveDiagnosticsConstants.Logs.DirectiveInitialized,
        Level = LogLevel.Information,
        Message = "Directive provider initialized.")]
    public static partial void DirectiveInitialized(this ILogger logger);

    [LoggerMessage(
        EventId = VKDirectiveDiagnosticsConstants.Logs.DirectiveResolved,
        Level = LogLevel.Information,
        Message = "Resolved Directive {DirectiveId}.")]
    public static partial void DirectiveResolved(this ILogger logger, string directiveId);
}
