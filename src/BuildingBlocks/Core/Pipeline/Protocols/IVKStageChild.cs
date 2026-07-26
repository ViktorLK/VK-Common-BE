namespace VK.Blocks.Core;

/// <summary>
/// Role marker interface defining generic components that can be directly contained within an <see cref="IVKPipelineStage{TContext, TResult}"/>.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public interface IVKStageChild<in TContext, TResult> : IVKPipelineComponent<TContext, TResult>
    where TContext : class;

/// <summary>
/// Role marker interface defining non-generic void components that can be directly contained within an <see cref="IVKPipelineStage{TContext}"/>.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
public interface IVKStageChild<in TContext> : IVKPipelineComponent<TContext>
    where TContext : class;
