using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Pagination.Internal;

/// <summary>
/// Pagination feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKPersistenceEFCoreBlock), OptionsType = typeof(VKPaginationOptions))]
internal sealed partial class PaginationFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKPaginationOptions options)
    {
        if (options.UseSecureSerializer)
        {
            services.TryAddSingleton<IVKCursorSerializer, SecureCursorSerializer>();
        }
        else
        {
            services.TryAddSingleton<IVKCursorSerializer, SimpleCursorSerializer>();
        }
    }

    static partial void ValidateFeatureCustom(VKPaginationOptions options, List<string> failures)
    {
        if (options.UseSecureSerializer)
        {
            if (string.IsNullOrWhiteSpace(options.SigningKey))
            {
                failures.Add($"{nameof(options.SigningKey)} is required when {nameof(options.UseSecureSerializer)} is enabled.");
            }
            else if (options.SigningKey.Length < 32)
            {
                failures.Add($"{nameof(options.SigningKey)} must be at least 32 characters long.");
            }
        }
    }
}
