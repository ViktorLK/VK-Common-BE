using VK.Blocks.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Infrastructure;

/// <summary>
/// A marker type for the VK.Blocks.Infrastructure building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKInfrastructureBlock
{

}
