using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Scoring.Internal;

/// <summary>
/// Task for LLM-based heuristic engram scoring.
/// Order = 30.
/// </summary>
internal sealed class LlmHeuristicScoringTask : IVKScoringTask
{
    private readonly IVKTextEngine? _textEngine;

    public VKPipelineSchedule Schedule => new(30);

    public LlmHeuristicScoringTask(IVKTextEngine? textEngine = null)
    {
        _textEngine = textEngine;
    }

    public async Task<VKResult<VKScoringResult>> ExecuteAsync(VKScoringContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (_textEngine is null)
        {
            // If LLM text engine is not registered/available, safely fallback (return 0.0)
            return VKResult.Success(VKScoringResult.SuccessScore(0.0));
        }

        try
        {
            var prompt = $"Evaluate the cognitive importance of the following memory entry from 0.0 (trivial) to 1.0 (crucial). Return ONLY a floating point number.\nContent: {context.Content}";
            var responseResult = await _textEngine.GenerateAsync(prompt, args: null, cancellationToken: cancellationToken).ConfigureAwait(false); // [CS.03]
            
            if (responseResult.IsSuccess && double.TryParse(responseResult.Value.Text.Trim(), out var score))
            {
                return VKResult.Success(VKScoringResult.SuccessScore(System.Math.Clamp(score, 0.0, 1.0)));
            }
        }
        catch
        {
            // Fail open / fallback safely on LLM invocation exceptions
        }

        return VKResult.Success(VKScoringResult.SuccessScore(0.0));
    }
}
