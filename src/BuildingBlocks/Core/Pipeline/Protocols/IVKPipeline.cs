using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Level 1 (Composite Root): Defines the top-level pipeline component that orchestrates stages, jobs, or tasks with generic result return.
/// Follows Pipeline-Stage-Job-Task Composite Pattern with Type-Safe Role Constraints.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
/// <typeparam name="TResult">The pipeline output result type.</typeparam>
public interface IVKPipeline<in TContext, TResult> : IVKPipelineComponent<TContext, TResult>
    where TContext : class
{
    /// <summary>
    /// Gets the collection of child components that are valid inside a Pipeline.
    /// </summary>
    IEnumerable<IVKPipelineChild<TContext, TResult>> Children => [];

    /// <summary>
    /// Default execution implementation: automatically evaluates child components using <see cref="VKPipelineRunner"/>.
    /// </summary>
    Task<VKResult<TResult>> IVKPipelineComponent<TContext, TResult>.ExecuteAsync(TContext context, CancellationToken cancellationToken)
        => VKPipelineRunner.ExecuteComponentsAsync(Children, context, options: null, default!, cancellationToken);
}

/// <summary>
/// Level 1 (Composite Root): Specialized top-level pipeline component with non-generic VKResult return.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
public interface IVKPipeline<in TContext> : IVKPipelineComponent<TContext>
    where TContext : class
{
    /// <summary>
    /// Gets the collection of child components that are valid inside a Pipeline.
    /// </summary>
    IEnumerable<IVKPipelineChild<TContext>> Children => [];

    /// <summary>
    /// Default execution implementation: automatically evaluates child components using <see cref="VKPipelineRunner"/>.
    /// </summary>
    Task<VKResult> IVKPipelineComponent<TContext>.ExecuteAsync(TContext context, CancellationToken cancellationToken)
    {
        var sorted = System.Linq.Enumerable.ToList(System.Linq.Enumerable.OrderBy(Children, c => c.Schedule.Order));
        var chunks = VKPipelineRunner.ChunkStages(
            sorted,
            c => c.Schedule.Order,
            c => c.Schedule.ParallelGroup);

        return VKPipelineRunner.ExecuteChunksAsync(
            chunks,
            context,
            checkAbortedFunc: ctx => false,
            abortResultFunc: ctx => VKResult.Success(),
            isParallelSelector: c => c.Schedule.IsParallel,
            executeFunc: (c, ctx, ct) => c.ExecuteAsync(ctx, ct),
            cancellationToken: cancellationToken);
    }
}
