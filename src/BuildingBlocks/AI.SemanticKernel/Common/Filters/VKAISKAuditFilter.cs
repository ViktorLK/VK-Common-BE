using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Filters;

/// <summary>
/// A Semantic Kernel filter that handles AI-specific auditing based on <see cref="VKAIAuditSettings"/>.
/// </summary>
public sealed class VKAISKAuditFilter(ILogger<VKAISKAuditFilter> logger) : IFunctionInvocationFilter, IPromptRenderFilter
{
    private readonly ILogger<VKAISKAuditFilter> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Audit logic here (e.g., recording call metadata)
        await next(context).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        await next(context).ConfigureAwait(false);

        // Audit logic here (e.g., recording rendered prompt for compliance)
    }
}
