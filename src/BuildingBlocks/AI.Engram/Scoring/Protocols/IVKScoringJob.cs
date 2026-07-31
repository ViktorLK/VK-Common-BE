using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines a scoring job in AI.Engram that orchestrates multiple scoring tasks.
/// Inherits from <see cref="IVKPipelineJob{TContext, TResult}"/>.
/// </summary>
public interface IVKScoringJob : IVKPipelineJob<VKScoringContext, VKScoringResult>;
