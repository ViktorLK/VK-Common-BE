using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Filters;

/// <summary>
/// A Semantic Kernel filter that checks for sensitive content based on <see cref="VKAISafetySettings"/>.
/// </summary>
public sealed class VKSensitiveContentFilter(
    ILogger<VKSensitiveContentFilter> logger,
    IOptions<VKAISafetySettings> safetySettings) : IFunctionInvocationFilter
{
    private readonly ILogger<VKSensitiveContentFilter> _logger = VKGuard.NotNull(logger);
    private readonly VKAISafetySettings _settings = safetySettings?.Value ?? new VKAISafetySettings();

    /// <inheritdoc />
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        await next(context).ConfigureAwait(false);

        if (_settings.EnableContentFilter == true && context.Result is not null)
        {
            var content = context.Result?.ToString();

            if (content?.Contains("VIOLENCE_FLAG") == true || content?.Contains("HATE_SPEECH") == true)
            {
                _logger.LogSensitiveContentDetected();
                throw new InvalidOperationException("Generated content violates safety guidelines.");
            }
        }
    }
}
