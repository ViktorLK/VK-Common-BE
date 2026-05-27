using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// A non-generic formatter to format raw fragments into model-specific string contents.
/// </summary>
public interface IVKPromptFormatter
{
    // Check if this formatter supports a given fragment or tier type
    bool CanFormat(VKPromptFragment fragment);

    // Format the fragment content using the pipeline context
    VKResult<string> Format(VKPromptFragment fragment, VKOrchestrationPipelineContext context);
}
