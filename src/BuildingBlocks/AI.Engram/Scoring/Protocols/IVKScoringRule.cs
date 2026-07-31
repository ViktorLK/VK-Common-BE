using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines a contract for a rule evaluated by <see cref="Scoring.Internal.RuleBasedScoringStrategy"/>.
/// </summary>
public interface IVKScoringRule
{
    /// <summary>
    /// Evaluates the scoring context against this rule.
    /// </summary>
    /// <returns>A scoring result if rule matched, or null if rule was not matched.</returns>
    Task<VKResult<VKScoringResult?>> EvaluateAsync(VKScoringContext context, CancellationToken cancellationToken = default);
}
