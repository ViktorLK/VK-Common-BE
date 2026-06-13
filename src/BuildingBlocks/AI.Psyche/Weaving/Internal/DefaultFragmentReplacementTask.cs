using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultFragmentReplacementTask : IVKWeavingTask
{
    private readonly IVKPromptTemplateEngine _templateEngine;
    private readonly VKWeavingOptions _options;

    public DefaultFragmentReplacementTask(
        IVKPromptTemplateEngine templateEngine,
        IOptions<VKWeavingOptions> options)
    {
        _templateEngine = VKGuard.NotNull(templateEngine);
        _options = VKGuard.NotNull(options).Value;
    }

    public int TaskOrder => VKWeavingTaskOrder.Replacement;
    public bool IsParallel => false;
    public int? ParallelGroup => null;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var variables = context.Args<VKWeavingArgs>()?.Variables ?? _options.Variables;
        if (variables is null || variables.Count == 0)
        {
            return VKResult.Success();
        }

        var newFragments = new List<VKPromptFragment>(context.Fragments.Count);
        foreach (var fragment in context.Fragments)
        {
            // [CS.01] Skip Echo (real history) to prevent prompt injection via history variables
            if (string.IsNullOrWhiteSpace(fragment.Segment.Content) || fragment.TierType == VKPromptTierType.Echo)
            {
                newFragments.Add(fragment);
                continue;
            }

            var msgResult = await _templateEngine.RenderAsync(fragment.Segment.Content, variables, cancellationToken).ConfigureAwait(false);
            if (msgResult.IsSuccess)
            {
                fragment.Segment = fragment.Segment with { Content = msgResult.Value };
            }
            newFragments.Add(fragment);
        }

        context.SetFragments(newFragments);

        return VKResult.Success();
    }
}
