using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Level 3 (Composite): Defines a pipeline job composite that orchestrates and executes child tasks.
/// Follows Pipeline-Stage-Job-Task Composite Pattern with Type-Safe Role Constraints.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
/// <typeparam name="TResult">The job output result type.</typeparam>
public interface IVKPipelineJob<in TContext, TResult>
    : IVKPipelineChild<TContext, TResult>, IVKStageChild<TContext, TResult>
    where TContext : class
{
    /// <summary>
    /// Gets the collection of child components that are valid inside a Job.
    /// </summary>
    IEnumerable<IVKJobChild<TContext, TResult>> Children => [];

    /// <summary>
    /// Default execution implementation: automatically evaluates child components using <see cref="VKPipelineRunner"/>.
    /// </summary>
    Task<VKResult<TResult>> IVKPipelineComponent<TContext, TResult>.ExecuteAsync(TContext context, CancellationToken cancellationToken)
        => VKPipelineRunner.ExecuteComponentsAsync(Children, context, options: null, default!, cancellationToken);
}

/// <summary>
/// Level 3 (Composite): Specialized pipeline job composite with non-generic VKResult return.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
public interface IVKPipelineJob<in TContext>
    : IVKPipelineChild<TContext>, IVKStageChild<TContext>
    where TContext : class
{
    /// <summary>
    /// Gets the collection of child components that are valid inside a Job.
    /// </summary>
    IEnumerable<IVKJobChild<TContext>> Children => [];

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
            checkCompletedFunc: ctx => false,
            isParallelSelector: c => c.Schedule.IsParallel,
            executeFunc: (c, ctx, ct) => c.ExecuteAsync(ctx, ct),
            cancellationToken: cancellationToken);
    }
}
