using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// AI.Psyche Block Marker.
/// Follows BB.02.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker(Dependencies = [typeof(VKAIBlock)], Toggleable = false)]
public sealed partial class VKAIPsycheBlock
{
    static partial void RegisterBlockCustom(IVKAIPsycheBuilder builder)
    {
        builder.Services.TryAddScoped<IVKPsycheModelFactory, DefaultPsycheModelFactory>();
    }
}
