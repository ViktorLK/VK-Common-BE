using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Middleware interface for controlling the flow of Psyche pipeline executions (Onion model).
/// Follows CS.01, CS.03.
/// </summary>
public interface IVKPsycheMiddleware : IVKMiddleware<VKPsycheContext>;
