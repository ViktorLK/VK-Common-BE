using VK.Blocks.Core;
using System.Collections.Generic;

namespace VK.Blocks.AI;

// [SG Marker] - This attribute triggers the Source Generator to generate module metadata and base implementation.
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)], Toggleable = false)]
public sealed partial class VKAIBlock
{
    static partial void RegisterBlockCustom(IVKAIBuilder builder)
    {
    }

    static partial void ValidateBlockCustom(VKAIOptions options, List<string> failures)
    {
        if (options.RetryCount < 0)
        {
            failures.Add("Global RetryCount cannot be negative.");
        }
    }

}
