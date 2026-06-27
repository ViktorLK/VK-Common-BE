using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Defines the contract for executing a request through a pipeline.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IVKPipeline<in TRequest, TResponse> where TRequest : class
{
    /// <summary>
    /// Executes the pipeline for the given request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pipeline execution response result.</returns>
    Task<VKResult<TResponse>> RunAsync(TRequest request, CancellationToken cancellationToken = default);
}
