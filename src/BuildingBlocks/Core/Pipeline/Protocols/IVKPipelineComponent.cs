using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Root non-generic marker contract for all named executable pipeline elements in the system
/// (Components, Middleware, Jobs, Stages, Pipelines).
/// Used for unified diagnostics, logging, and inspection tools.
/// </summary>
public interface IVKPipelineComponent
{
    /// <summary>
    /// Gets the explicit scheduling, phase, and concurrency configuration for this component.
    /// </summary>
    VKPipelineSchedule Schedule => new(0);

    /// <summary>
    /// Gets a value indicating whether this pipeline component is currently active and should be executed.
    /// Defaults to true.
    /// </summary>
    bool IsActive => true;

    /// <summary>
    /// Gets the element display name.
    /// </summary>
    string Name => GetType().Name;
}

/// <summary>
/// Specialized pipeline component interface for void return pipeline nodes (returns non-generic VKResult).
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
public interface IVKPipelineComponent<in TContext> : IVKPipelineComponent where TContext : class
{
    /// <summary>
    /// Executes the component logic without strong result value.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A non-generic VKResult indicating success or failure.</returns>
    Task<VKResult> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Root component contract for all nodes in the pipeline composite hierarchy (Tasks, Jobs, Stages, Pipelines).
/// Follows classic Composite Pattern and inherits from non-generic IVKPipelineComponent.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
/// <typeparam name="TResult">The execution result type.</typeparam>
public interface IVKPipelineComponent<in TContext, TResult> : IVKPipelineComponent where TContext : class
{
    /// <summary>
    /// Executes the component logic.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the component output.</returns>
    Task<VKResult<TResult>> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}
