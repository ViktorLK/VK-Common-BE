using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines an atomic scoring task in AI.Engram that evaluates memory content/context and produces a score or routing directive.
/// Inherits from <see cref="IVKPipelineTask{TContext, TResult}"/>.
/// </summary>
public interface IVKScoringTask : IVKPipelineTask<VKScoringContext, VKScoringResult>;
