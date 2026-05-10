using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;

/// <summary>
/// An AISK filter that bridges SK events to the VK Diagnostics system.
/// Implements <see cref="IFunctionInvocationFilter"/> and <see cref="IPromptRenderFilter"/>.
/// </summary>
internal sealed class AISKDiagnosticsFilter(ILogger<AISKDiagnosticsFilter> logger)
    : IFunctionInvocationFilter, IPromptRenderFilter
{
    private readonly ILogger<AISKDiagnosticsFilter> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        _logger.LogFunctionInvoking(context.Function.PluginName ?? "Global", context.Function.Name);

        await next(context).ConfigureAwait(false);

        _logger.LogFunctionInvoked(context.Function.PluginName ?? "Global", context.Function.Name);
    }

    /// <inheritdoc />
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        await next(context).ConfigureAwait(false);

        _logger.LogPromptRendered(context.Function.PluginName ?? "Global", context.Function.Name, context.RenderedPrompt ?? "N/A");
    }
}
