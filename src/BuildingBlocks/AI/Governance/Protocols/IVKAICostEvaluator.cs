using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI;

/// <summary>
/// Evaluates and calculates monetary costs for requests based on token counts and model pricing models.
/// </summary>
public interface IVKAICostEvaluator
{
    /// <summary>
    /// Calculates the cost based on token counts and pricing rules for the specified model.
    /// </summary>
    ValueTask<VKAICostUsage> EvaluateCostAsync(
        string modelId,
        int inputTokens,
        int outputTokens,
        CancellationToken cancellationToken = default);
}
