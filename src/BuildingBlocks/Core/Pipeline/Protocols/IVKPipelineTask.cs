namespace VK.Blocks.Core;

/// <summary>
/// Level 4 (Leaf): Defines an ordered atomic task component evaluated within a pipeline job, stage, or pipeline with generic result output.
/// Follows Pipeline-Stage-Job-Task Composite Pattern with Role Marker Interfaces.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
/// <typeparam name="TResult">The task result type.</typeparam>
public interface IVKPipelineTask<in TContext, TResult>
    : IVKPipelineChild<TContext, TResult>, IVKStageChild<TContext, TResult>, IVKJobChild<TContext, TResult>
    where TContext : class;

/// <summary>
/// Level 4 (Leaf): Defines an ordered atomic void task component evaluated within a pipeline job, stage, or pipeline.
/// Follows Pipeline-Stage-Job-Task Composite Pattern with Role Marker Interfaces.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
public interface IVKPipelineTask<in TContext>
    : IVKPipelineChild<TContext>, IVKStageChild<TContext>, IVKJobChild<TContext>
    where TContext : class;
