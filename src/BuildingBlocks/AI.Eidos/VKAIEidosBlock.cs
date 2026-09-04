using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Eidos.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Architectural Marker for VK.Blocks.AI.Eidos BuildingBlock.
/// Governed by [BB.02] Marker Pattern & [BB.03] Custom Hooks.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKAIBlock)])]
public sealed partial class VKAIEidosBlock
{
    static partial void RegisterBlockCustom(IVKAIEidosBuilder builder)
    {
        builder.Services.TryAddSingleton<IVKSchemaFactory, DefaultSchemaFactory>();
    }
}
