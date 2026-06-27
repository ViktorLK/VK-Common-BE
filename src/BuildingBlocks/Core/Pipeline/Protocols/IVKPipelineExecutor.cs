using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Defines the contract for orchestrating and executing generic pipeline.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IVKPipelineExecutor<in TContext, TResponse> where TContext : class
{
    /// <summary>
    /// Executes the pipeline with the given context.
    /// </summary>
    /// <param name="context">The pipeline context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pipeline execution response result.</returns>
    Task<VKResult<TResponse>> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}
