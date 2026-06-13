using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultCoordinateResolveTask : IVKWeavingTask
{
    private readonly VKWeavingOptions _options;

    public DefaultCoordinateResolveTask(IOptions<VKWeavingOptions> options)
    {
        _options = VKGuard.NotNull(options).Value;
    }

    public int TaskOrder => VKWeavingTaskOrder.CoordinateResolve;
    public bool IsParallel => false;
    public int? ParallelGroup => null;

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var tierOrderOverrides = context.Args<VKWeavingArgs>()?.TierRenderOrderOverrides ?? _options.TierRenderOrderOverrides;

        var dict = tierOrderOverrides is not null
            ? tierOrderOverrides.Select((t, index) => new { t, index }).ToDictionary(x => x.t, x => x.index * PsycheConstants.Layout.TierCoordinateGap)
            : PromptLayout.DefaultRenderOrders;

        foreach (var f in context.Fragments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Ignore Absolute (handled at Weaving stage) and already Fixed fragments (RenderOrder already set)
            if (f.Segment.AbsoluteDepth is not null || f.RenderOrder is not null)
            {
                continue;
            }

            var coord = PromptPositionResolver.Resolve(f.Segment, dict);
            f.RenderOrder = coord.RenderOrder;
        }

        return Task.FromResult(VKResult.Success());
    }
}
