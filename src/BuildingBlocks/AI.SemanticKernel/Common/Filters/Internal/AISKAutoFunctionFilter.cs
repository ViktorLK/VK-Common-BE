using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Filters.Internal;

/// <summary>
/// A Semantic Kernel filter that intercepts automatic function invocations triggered by the LLM.
/// Enforces round limits, records observability data, and supports emergency termination.
/// Implements <see cref="IAutoFunctionInvocationFilter"/>.
/// </summary>
/// <remarks>
/// This filter is active only when <see cref="VKChatOptions.EnableAutoToolCalling"/> is <c>true</c>.
/// It provides the industrial-grade safeguard layer around SK's native auto tool-calling loop.
/// </remarks>
internal sealed class AISKAutoFunctionFilter(
    ILogger<AISKAutoFunctionFilter> logger,
    IOptions<VKChatOptions> chatOptions) : IAutoFunctionInvocationFilter // [AP.01]
{
    private readonly ILogger<AISKAutoFunctionFilter> _logger = VKGuard.NotNull(logger); // [AP.01]
    private readonly VKChatOptions _chatOptions = VKGuard.NotNull(chatOptions?.Value); // [AP.01]

    /// <inheritdoc />
    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        VKGuard.NotNull(context); // [AP.01]
        VKGuard.NotNull(next);    // [AP.01]

        var pluginName = context.Function.PluginName ?? "Global";
        var functionName = context.Function.Name;
        var round = context.RequestSequenceIndex;

        // 1. Guard: Enforce maximum auto-invocation rounds to prevent runaway loops.
        var maxRounds = _chatOptions.MaxAutoToolCallRounds;
        if (round >= maxRounds) // [CS.01]
        {
            _logger.LogAutoToolCallTerminated(maxRounds);
            context.Terminate = true;
            return;
        }

        // 2. Observability: Audit log at start.
        _logger.LogAutoToolCallInvoking(pluginName, functionName, round); // [OR.01]

        await next(context).ConfigureAwait(false); // [CS.03]

        // 3. Observability: Record metric and completion log.
        AISKMetrics.RecordAutoToolCall(pluginName, functionName);
        _logger.LogAutoToolCallCompleted(pluginName, functionName, round); // [OR.01]
    }
}
