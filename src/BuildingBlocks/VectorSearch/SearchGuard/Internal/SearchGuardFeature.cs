using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.VectorSearch.SearchGuard.Internal;

/// <summary>
/// Search Guard feature marker and registration hub.
/// </summary>
internal sealed partial class SearchGuardFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKSearchGuardOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKVectorSearchMiddleware, DefaultSearchGuardMiddleware>();
    }

    static partial void ValidateCustom(VKSearchGuardOptions options, List<string> failures)
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
