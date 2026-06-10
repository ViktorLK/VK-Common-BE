using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pattern.Internal;

internal sealed class DefaultPatternStage : IVKPsycheBeforePipelineStage
{
    private readonly VKPatternOptions _options;
    private readonly IVKPatternStore _store;
    private readonly VKWeavingOptions _weavingOptions;

    public int StageOrder => VKWeavingStageOrder.Pattern;

    public bool IsActive => _options.Enabled;

    public bool IsParallel => true;

    public int? ParallelGroup => 2;

    public DefaultPatternStage(
        IOptions<VKPatternOptions> options,
        IVKPatternStore store,
        IOptions<VKWeavingOptions> weavingOptions)
    {
        _options = VKGuard.NotNull(options).Value;
        _store = VKGuard.NotNull(store);
        _weavingOptions = VKGuard.NotNull(weavingOptions?.Value);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.WeavingArgs?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Pattern))
        {
            return VKResult.Success();
        }

        var patternsResult = await _store.GetCurrentPatternsAsync(ct).ConfigureAwait(false); // [CS.03]
        if (patternsResult.IsFailure)
        {
            return VKResult.Failure(patternsResult.Errors); // [CS.01]
        }

        var currentPatterns = patternsResult.Value.ToList();

        var groups = currentPatterns
            .GroupBy(entry =>
            {
                var coord = PromptPositionResolver.Resolve(entry.Position, entry.Priority, PromptLayout.DefaultRenderOrders);
                return (Role: coord.Role, Depth: coord.Depth, RenderOrderOffset: coord.RenderOrder);
            });

        foreach (var group in groups)
        {
            var key = group.Key;
            var entriesForSlot = group.ToList();

            for (int i = 0; i < entriesForSlot.Count; i++)
            {
                context.AddFragment(new VKPromptFragment
                {
                    TierType = VKPromptTierType.Pattern,
                    Role = key.Role,
                    Depth = key.Depth,
                    RenderOrder = key.RenderOrderOffset + i,
                    Content = entriesForSlot[i].Content,
                    Metadata = entriesForSlot[i]
                });
            }
        }

        return VKResult.Success();
    }
}
