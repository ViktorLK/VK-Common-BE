using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// A marker type for the VK.Blocks.AI.Synapse building block.
/// </summary>
[ExcludeFromCodeCoverage]
[VKBlockMarker(Dependencies = [typeof(VKAIBlock), typeof(VKResilienceBlock)], Toggleable = false)]
public sealed partial class VKAISynapseBlock
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKAISynapseBuilder builder)
    {
        // [BB.06]
        builder.AddVKAISynapseDefaultFeatures();
    }

    // [SG Hook]
    static partial void ValidateBlockCustom(VKAISynapseOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
