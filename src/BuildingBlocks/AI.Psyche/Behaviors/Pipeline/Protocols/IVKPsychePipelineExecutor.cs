using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Entry point for executing the Psyche behavior pipeline chain.
/// Follows CS.01, CS.03.
/// </summary>
public interface IVKPsychePipelineExecutor
{
    /// <summary>
    /// Executes the entire middleware chain for the given context.
    /// </summary>
    Task<VKResult<VKPsycheResponse>> ExecuteAsync(
        VKPsycheContext context,
        CancellationToken cancellationToken = default);
}
