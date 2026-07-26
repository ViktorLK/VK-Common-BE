using System;

namespace VK.Blocks.Core;

/// <summary>
/// Optional configurations for pipeline component execution in <see cref="VKPipelineRunner"/>.
/// Decouples execution features (short-circuiting, aborting, parallelization) from individual component contracts.
/// Follows AP.01 (sealed record).
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public sealed record VKPipelineComponentOptions<TContext, TResult>(
    Func<TResult, bool>? ShortCircuitPredicate = null,
    Func<TContext, bool>? AbortPredicate = null,
    Func<TContext, VKResult<TResult>>? AbortResultFactory = null,
    bool EnableParallelExecution = false,
    Func<IVKPipelineComponent<TContext, TResult>, int?>? ParallelGroupSelector = null
) where TContext : class;
