using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;
using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// A marker type for the VK.Blocks.AI.SemanticKernel building block.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker(Dependencies = [typeof(VKAIBlock)])]
public sealed partial class VKAISemanticKernelBlock
{

    static partial void RegisterBlockCustom(IVKAISemanticKernelBuilder builder)
    {
        builder.Services.AddVKAISemanticKernelImplementations(builder.Configuration);
    }

}
