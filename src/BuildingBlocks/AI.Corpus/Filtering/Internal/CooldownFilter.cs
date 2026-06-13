using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that checks turn usage history and skips entries that are currently in their cooldown period.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class CooldownFilter : IVKKnowledgeLifecycleFilter
{
    private readonly VKFilteringOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="CooldownFilter"/>.
    /// </summary>
    public CooldownFilter(IOptions<VKFilteringOptions> options)
    {
        _options = VKGuard.NotNull(options?.Value);
    }

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> EvaluateAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        string idValue = entry.Knowledge.Id.Value.ToString();
        int cooldownTurns = entry.Lifecycle.CooldownTurns ?? _options.DefaultCooldownTurns;

        if (context.LastInjectedTurns.TryGetValue(idValue, out int lastTurn))
        {
            if (cooldownTurns == -1 || context.CurrentTurn - lastTurn < cooldownTurns)
            {
                // In cooldown period or single-trigger only: filter out
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}



