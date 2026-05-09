using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Persistence building block.
/// </summary>
internal static class PersistenceBlockRegistration
{
    public static IVKPersistenceBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKPersistenceOptions, VKPersistenceOptions>? transformOptions = null)
    {
        var builder = new PersistenceBlockBuilder(services, configuration);

        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKPersistenceBlock>())
        {
            return builder;
        }
        services.EnsureCoreBlockRegistered<VKPersistenceBlock>();

        // 2. Options Registration
        var options = services.AddVKBlockOptions<VKPersistenceOptions>(configuration, transformOptions);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKPersistenceBlock>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKPersistenceOptions>, PersistenceOptionsValidator>();

        // 5. Diagnostics/Static Metadata

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 7. Core Services
        // Currently this block only contains interfaces. 
        // Implementations are provided by Persistence.EFCore etc.

        return builder;
    }
}
