using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Identity.Web.Context.Internal;

namespace VK.Blocks.Identity.Web;

/// <summary>
/// A marker type for the VK.Blocks.Identity.Web building block.
/// Provides ASP.NET Core HTTP context user resolution and ambient execution flow management.
/// Follows BB.02, AP.01, AP.03.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker(Dependencies = [typeof(VKIdentityBlock)], Toggleable = false)]
public sealed partial class VKIdentityWebBlock
{
    static partial void RegisterBlockCustom(IVKIdentityWebBuilder builder)
    {
        // Custom hooks for Identity.Web can be added here
    }
}
