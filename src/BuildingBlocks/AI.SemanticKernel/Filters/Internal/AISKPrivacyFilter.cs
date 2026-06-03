using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Filters.Internal;

/// <summary>
/// A Semantic Kernel filter that masks PII in prompts before they are sent to the LLM.
/// Implements the Industrial DNA requirement for privacy protection.
/// </summary>
internal sealed class AISKPrivacyFilter(IVKPrivacyFilter privacyFilter) : IPromptRenderFilter
{
    private readonly IVKPrivacyFilter _privacyFilter = VKGuard.NotNull(privacyFilter);

    /// <inheritdoc />
    public void OnPromptRender(PromptRenderContext context)
    {
        // Synchronous intercept not implemented because mask operation is asynchronous.
    }

    /// <inheritdoc />
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        // Let SK render the prompt first
        await next(context).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(context.RenderedPrompt))
        {
            var result = await _privacyFilter.MaskAsync(context.RenderedPrompt).ConfigureAwait(false);
            if (result.IsSuccess && result.Value != null)
            {
                // Replace the rendered prompt with the masked version
                context.RenderedPrompt = result.Value.MaskedText;
            }
        }
    }
}
