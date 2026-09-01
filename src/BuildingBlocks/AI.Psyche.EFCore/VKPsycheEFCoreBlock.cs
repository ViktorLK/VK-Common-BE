using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Psyche.EFCore.Echo.Internal;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// AI.Psyche.EFCore Building Block Marker.
/// Provides EFCore-backed implementations for all AI.Psyche stores and auto-generated entity & aggregate repositories.
/// Follows BB.02, AP.01, AP.02.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker(Dependencies = [typeof(VKAIPsycheBlock), typeof(VKPersistenceEFCoreBlock)], Toggleable = false)]
public sealed partial class VKAIPsycheEFCoreBlock
{
    static partial void RegisterBlockCustom(IVKAIPsycheEFCoreBuilder builder)
    {
        var services = builder.Services;

        // Echo Sliding-Window Buffer
        services.AddScoped<IVKEchoStore, EchoStore>();
    }
}
