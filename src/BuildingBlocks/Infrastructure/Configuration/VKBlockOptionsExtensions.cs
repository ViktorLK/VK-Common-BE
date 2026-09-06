using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.Infrastructure.Configuration.Internal;

namespace VK.Blocks.Infrastructure.Configuration;

/// <summary>
/// Enhanced options registration extensions using Infrastructure-level binders.
/// </summary>
public static class VKBlockOptionsExtensions
{
    /// <summary>
    /// An infrastructure-enhanced version of options registration that supports zero-reflection binding.
    /// </summary>
    public static TOptions AddVKInfrastructureOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<TOptions, TOptions>? transform = null)
        where TOptions : class, IVKBlockOptions, new()
    {
        // 1. Get Section
        var section = configuration.GetSection(TOptions.SectionName);

        // 2. Bind using optimized binder
        var options = VKConfigurationBinder.BindOptions<TOptions>(section);

        // 3. Apply transformation (ADR-016)
        if (transform is not null)
        {
            options = transform(options);
        }

        // 4. Register using Core's dual-registration logic
        // Note: We might want to move the actual registration logic here or call Core.
        return services.AddVKBlockOptions<TOptions>(configuration, _ => options);
    }
}
