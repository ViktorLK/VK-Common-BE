namespace VK.Blocks.Core;

/// <summary>
/// Defines a sequential pipeline stage.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
public interface IVKSequentialPipelineStage<in TContext> : IVKBeforePipelineStage<TContext>
    where TContext : class;
