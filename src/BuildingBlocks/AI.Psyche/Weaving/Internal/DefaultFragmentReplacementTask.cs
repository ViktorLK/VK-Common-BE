using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultFragmentReplacementTask : IVKWeavingPipelineTask
{
    private readonly IVKPromptTemplateEngine _templateEngine;
    private readonly VKWeavingOptions _options;

    public DefaultFragmentReplacementTask(IVKPromptTemplateEngine templateEngine, VKWeavingOptions options)
    {
        _templateEngine = VKGuard.NotNull(templateEngine);
        _options = VKGuard.NotNull(options);
    }

    public VKPipelineSchedule Schedule => new(VKWeavingTaskOrder.Replacement);

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var variables = context.Args<VKWeavingArgs>()?.Variables;
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
