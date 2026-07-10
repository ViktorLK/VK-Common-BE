using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Persistence.EFCore.Database.Internal;

/// <summary>
/// Database feature marker and registration hub.
/// </summary>
internal sealed partial class DatabaseFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKDatabaseOptions options)
    {
        _ = services;
        _ = options;
    }

    static partial void ValidateFeatureCustom(VKDatabaseOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            failures.Add($"{nameof(options.ConnectionString)} cannot be null or empty.");
        }

        if (options.CommandTimeout <= 0)
        {
            failures.Add($"{nameof(options.CommandTimeout)} must be greater than 0.");
        }

        if (options.MaxRetryCount < 0)
        {
            failures.Add($"{nameof(options.MaxRetryCount)} cannot be negative.");
        }

        if (options.MaxRetryDelay <= System.TimeSpan.Zero)
        {
            failures.Add($"{nameof(options.MaxRetryDelay)} must be greater than zero.");
        }
    }
}
