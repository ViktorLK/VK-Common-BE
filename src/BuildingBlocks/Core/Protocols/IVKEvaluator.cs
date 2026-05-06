using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Defines a generic contract for security and logic evaluators in VK.Blocks.
/// Provides a unified pattern for both programmatic and declarative evaluation.
/// </summary>
/// <typeparam name="TArgs">The type of the arguments used for evaluation.</typeparam>
public interface IVKEvaluator<in TArgs> where TArgs : class
{
    /// <summary>
    /// Evaluates the logic based on the provided user and arguments asynchronously.
    /// </summary>
    /// <param name="user">The user (claims principal) to evaluate.</param>
    /// <param name="args">The arguments (local overrides) for the evaluation.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> where <c>T</c> is <c>bool</c>.</returns>
    ValueTask<VKResult<bool>> EvaluateAsync(
        ClaimsPrincipal user,
        TArgs? args = null,
        CancellationToken ct = default);
}
