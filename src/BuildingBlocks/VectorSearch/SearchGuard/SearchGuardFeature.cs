using VK.Blocks.VectorSearch.SearchGuard.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Search Guard feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), OptionsType = typeof(VKSearchGuardOptions))]
internal sealed partial class SearchGuardFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKSearchGuardOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKVectorSearchMiddleware, DefaultSearchGuardMiddleware>();
    }

    static partial void ValidateFeatureCustom(VKSearchGuardOptions options, List<string> failures)
    {
        if (options.MinLength < 0)
        {
            failures.Add("MinLength must be greater than or equal to 0.");
        }
        if (options.MaxLength <= 0)
        {
            failures.Add("MaxLength must be greater than 0.");
        }
        if (options.MaxLength < options.MinLength)
        {
            failures.Add("MaxLength cannot be less than MinLength.");
        }
    }
}
