using VK.Blocks.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.ExceptionHandling.Common.DependencyInjection.Protocols;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// A marker type for the VK.Blocks.ExceptionHandling building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKExceptionHandlingBlock
{

    static partial void RegisterBlockCustom(IVKExceptionHandlingBuilder builder)
    {
        // Delegate actual feature registration to ResolutionFeature
        ResolutionFeature.Register(builder);
    }

}
