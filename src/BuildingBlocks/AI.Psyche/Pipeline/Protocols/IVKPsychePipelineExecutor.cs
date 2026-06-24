using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Entry point for executing the Psyche behavior pipeline chain.
/// Follows CS.01, CS.03.
/// </summary>
public interface IVKPsychePipelineExecutor : IVKPipelineExecutor<VKPsycheContext, VKPsycheResponse>;
