using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// The spine mental pipeline. Executes registered psyche stages supporting both serial and parallel pipelines.
/// Follows CS.01, CS.03.
/// </summary>
public interface IVKPsychePipeline : IVKPipeline<VKPsycheRequest, VKPsycheResponse> {}
