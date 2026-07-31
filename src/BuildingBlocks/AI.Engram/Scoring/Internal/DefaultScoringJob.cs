using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Scoring.Internal;

/// <summary>
/// Composite scoring job that orchestrates registered <see cref="IVKScoringTask"/> instances via Core Pipeline execution.
/// Evaluates scoring tasks in order by task.Schedule.Order and short-circuits on security rejections,
/// structured fact routing, or non-zero score output.
/// </summary>
internal sealed class DefaultScoringJob : IVKScoringJob
{
    private readonly List<IVKScoringTask> _tasks;
    private readonly VKScoringOptions _options;

    public VKPipelineSchedule Schedule => new(0);

    public IEnumerable<IVKJobChild<VKScoringContext, VKScoringResult>> Children => _tasks;

    public DefaultScoringJob(
        IEnumerable<IVKScoringTask> tasks,
        IOptions<VKScoringOptions> options)
    {
        VKGuard.NotNull(tasks);
        _options = VKGuard.NotNull(options?.Value);
        _tasks = tasks.OrderBy(t => t.Schedule.Order).ToList();
    }

    public async Task<VKResult<VKScoringResult>> ExecuteAsync(VKScoringContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // Filter active tasks based on dynamic options toggles
        var activeTasks = _tasks.Where(task =>
        {
            if (!_options.EnableRuleBasedScoring && task is RuleBasedScoringTask) return false;
            if (!_options.EnableEmotionalScoring && task is EmotionalImpactScoringTask) return false;
            if (!_options.EnableLlmScoring && task is LlmHeuristicScoringTask) return false;
            return true;
        });

        var pipelineOptions = new VKPipelineComponentOptions<VKScoringContext, VKScoringResult>
        {
            // Short-circuit when a task outputs a non-Score directive (SecurityReject / RouteToStructured) OR a positive score (> 0.0)
            ShortCircuitPredicate = res => res is not null && (res.Directive != VKScoringDirective.Score || res.Score > 0.0)
        };

        return await VKPipelineRunner.ExecuteComponentsAsync(
            activeTasks,
            context,
            pipelineOptions,
            defaultResult: VKScoringResult.SuccessScore(0.0),
            cancellationToken: cancellationToken).ConfigureAwait(false); // [CS.03]
    }
}
