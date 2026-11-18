using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

// [SG Marker] - This attribute triggers the Source Generator to generate module metadata and base implementation.
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)], Toggleable = false)]
public sealed partial class VKAIBlock
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKAIBuilder builder)
    {
        _ = builder;
    }

    // [SG Hook]
    static partial void ValidateBlockCustom(VKAIOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
