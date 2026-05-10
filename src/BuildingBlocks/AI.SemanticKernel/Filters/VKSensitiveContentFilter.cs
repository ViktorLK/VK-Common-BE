using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Filters;

/// <summary>
/// A Semantic Kernel filter that checks for sensitive content based on <see cref="VKAISafetySettings"/>.
/// </summary>
public sealed class VKSensitiveContentFilter(ILogger<VKSensitiveContentFilter> logger) : IFunctionInvocationFilter
{
    private readonly ILogger<VKSensitiveContentFilter> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Sensitive content detection logic here
        await next(context).ConfigureAwait(false);
    }
}
