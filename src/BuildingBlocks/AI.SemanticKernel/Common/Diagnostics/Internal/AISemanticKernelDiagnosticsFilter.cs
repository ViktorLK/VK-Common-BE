using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;

/// <summary>
/// An AISemanticKernel filter that bridges SK events to the VK Diagnostics system.
/// Implements <see cref="IFunctionInvocationFilter"/> and <see cref="IPromptRenderFilter"/>.
/// Respects <see cref="VKAIOptions.EnableSensitiveDataLogging"/> for privacy compliance.
/// </summary>
internal sealed class AISemanticKernelDiagnosticsFilter(
    ILogger<AISemanticKernelDiagnosticsFilter> logger,
    IOptions<VKAIOptions> globalAiOptions)
    : IFunctionInvocationFilter, IPromptRenderFilter
{
    private readonly ILogger<AISemanticKernelDiagnosticsFilter> _logger = VKGuard.NotNull(logger);
    private readonly VKAIOptions _globalOptions = VKGuard.NotNull(globalAiOptions.Value);

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

        if (_globalOptions.EnableSensitiveDataLogging)
        {
            _logger.LogPromptRendered(context.Function.PluginName ?? "Global", context.Function.Name, context.RenderedPrompt ?? "N/A");
        }
    }
}
