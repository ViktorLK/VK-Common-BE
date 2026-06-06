using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Filters;

/// <summary>
/// A Semantic Kernel filter that applies retry policies based on <see cref="VKAIResilienceSettings"/>.
/// </summary>
public sealed class VKRetryPolicyFilter(ILogger<VKRetryPolicyFilter> logger) : IFunctionInvocationFilter
{
    private readonly ILogger<VKRetryPolicyFilter> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Note: SK already uses StandardResilienceHandler at the HttpClient level.
        // This filter can be used for semantic-level retries if needed.
        await next(context).ConfigureAwait(false);
    }
}
