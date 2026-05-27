using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Reasoning.Internal;

/// <summary>
/// A no-op / pass-through implementation of <see cref="IVKReasoningPlanner"/> that returns a single-step plan.
/// Avoids calling LLM, ensuring deterministic industrial baseline execution.
/// </summary>
internal sealed class NoopReasoningPlanner : IVKReasoningPlanner
{
    private readonly IVKGuidGenerator _guidGenerator;

    public NoopReasoningPlanner(IVKGuidGenerator guidGenerator)
    {
        _guidGenerator = VKGuard.NotNull(guidGenerator);
    }

    public Task<VKResult<VKGoal>> PlanAsync(
        string goal,
        VKReasoningArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(goal);

        var steps = new List<VKStep>
        {
            new()
            {
                Id = _guidGenerator.Create().ToString(),
                Name = "Execute Goal",
                Description = goal,
                Target = "SystemDefault"
            }
        };

        var vkGoal = new VKGoal
        {
            Id = _guidGenerator.Create().ToString(),
            Title = $"Direct Plan: {(goal.Length > 30 ? goal.Substring(0, 30) + "..." : goal)}",
            Description = goal,
            Steps = steps
        };

        return Task.FromResult(VKResult.Success(vkGoal));
    }
}
