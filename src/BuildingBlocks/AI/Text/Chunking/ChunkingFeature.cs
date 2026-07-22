using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Chunking feature marker and registration hub.
/// </summary>
[VKFeature(typeof(global::VK.Blocks.AI.TextFeature), OptionsType = typeof(VKChunkingOptions))]
internal sealed partial class ChunkingFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKChunkingOptions options)
    {
        _ = services;
        _ = options;
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKChunkingOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
