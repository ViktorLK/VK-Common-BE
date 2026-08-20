using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Scoring.Internal;

/// <summary>
/// Task for rule-based engram scoring.
/// Order = 10.
/// </summary>
internal sealed class RuleBasedScoringTask : IVKScoringTask
{
    private readonly IEnumerable<IVKScoringRule> _rules;

    public VKPipelineSchedule Schedule => new(10);

    public RuleBasedScoringTask(IEnumerable<IVKScoringRule> rules)
    {
        _rules = VKGuard.NotNull(rules);
    }

    public async Task<VKResult<VKScoringResult>> ExecuteAsync(VKScoringContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        foreach (var rule in _rules)
        {
            var result = await rule.EvaluateAsync(context, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (result.IsFailure)
            {
                return VKResult.Failure<VKScoringResult>(result.Errors);
            }

            if (result.IsSuccess)
            {
                try
                {
                    if (result.Value is not null)
                    {
                        return VKResult.Success(result.Value);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Result was Success with null value
                }
            }
        }

        // Pass through if no rule matched
        return VKResult.Success(VKScoringResult.SuccessScore(0.0));
    }
}
