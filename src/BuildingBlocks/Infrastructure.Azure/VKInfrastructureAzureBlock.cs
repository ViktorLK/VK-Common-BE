using VK.Blocks.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Infrastructure.Azure.Abstractions;
using VK.Blocks.Infrastructure.Azure.Identity.Internal;

namespace VK.Blocks.Infrastructure.Azure;
/// <summary>
/// A marker type for the VK.Blocks.Infrastructure.Azure building block.
/// </summary>
[VKBlockMarker("Infrastructure.Azure", Dependencies = [typeof(VKCoreBlock), typeof(VKInfrastructureBlock)])]
public sealed partial class VKInfrastructureAzureBlock
{

}
