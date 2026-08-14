using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Synapse.Internal;

using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// A marker type for the VK.Blocks.AI.Synapse building block.
/// </summary>
[ExcludeFromCodeCoverage]
[VKBlockMarker(Dependencies = [typeof(VKAIBlock), typeof(VKResilienceBlock)], Toggleable = false)]
public sealed partial class VKAISynapseBlock
{
    static partial void RegisterBlockCustom(IVKAISynapseBuilder builder)
    {
        builder.Services.TryAddSingleton<IVKAISynapseModelFactory, Common.Internal.DefaultAISynapseModelFactory>();
    }
}
