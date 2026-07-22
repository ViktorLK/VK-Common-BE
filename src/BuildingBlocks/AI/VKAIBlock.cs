using VK.Blocks.Core;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI;

// [SG Marker] - This attribute triggers the Source Generator to generate module metadata and base implementation.
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)], Toggleable = false)]
public sealed partial class VKAIBlock
{

    static partial void RegisterBlockCustom(IVKAIBuilder builder)
    {
        builder.Services.TryAddSingleton<VK.Blocks.AI.IVKEngineRouter, VK.Blocks.AI.Common.Routing.Internal.NoOpVKEngineRouter>();
    }

    static partial void ValidateBlockCustom(VKAIOptions options, List<string> failures)
    {
        if (options.RetryCount < 0)
        {
            failures.Add("Global RetryCount cannot be negative.");
        }
    }

}
