using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// The top-level weaving coordinator executing the sequential pipeline stages.
/// </summary>
public interface IVKPromptWeavingEngine
{
    VKResult<VKPromptTapestry> WeavePrompt(VKWeavingContext context);
}
