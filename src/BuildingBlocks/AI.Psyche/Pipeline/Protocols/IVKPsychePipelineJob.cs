using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines a pipeline job in Psyche that orchestrates multiple tasks (e.g. EchoJob, PersonaJob, WeavingJob).
/// Inherits from <see cref="IVKPipelineJob{TContext, TResult}"/>.
/// </summary>
public interface IVKPsychePipelineJob<TResult> : IVKPipelineJob<VKPsycheContext, TResult>;
