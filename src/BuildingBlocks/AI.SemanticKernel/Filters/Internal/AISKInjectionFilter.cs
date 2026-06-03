using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Filters.Internal;

/// <summary>
/// A Semantic Kernel filter that detects prompt injection attempts before they are sent to the LLM.
/// Implements the Industrial DNA requirement for prompt injection defense.
/// </summary>
internal sealed class AISKInjectionFilter(IVKInjectionDetector injectionDetector) : IPromptRenderFilter
{
    private readonly IVKInjectionDetector _injectionDetector = VKGuard.NotNull(injectionDetector);

    /// <inheritdoc />
    public void OnPromptRender(PromptRenderContext context)
    {
        // Synchronous intercept not implemented because detection is asynchronous.
    }

    /// <inheritdoc />
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        // Let SK render the prompt first
        await next(context).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(context.RenderedPrompt))
        {
            var result = await _injectionDetector.DetectAsync(context.RenderedPrompt).ConfigureAwait(false);
            if (result.IsSuccess && result.Value?.IsInjectionDetected == true)
            {
                // Throw an exception to immediately short-circuit the execution
                throw new InvalidOperationException($"Prompt injection detected. Type: {result.Value.DetectedPatternType}, Confidence: {result.Value.ConfidenceScore}");
            }
        }
    }
}
