using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Persistence.Auditing.Internal;

namespace VK.Blocks.Persistence.Common.DependencyInjection.Internal;

/// <summary>
/// Partial implementation for Persistence feature hooks.
/// </summary>
internal sealed partial class PersistenceBlock
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKPersistenceBuilder builder)
    {
        if (!builder.Services.GetVKServiceInstance<VKPersistenceOptions>()!.EnableAuditing)
        {
            builder.Services.TryAddScoped<IVKAuditProvider>(sp =>
                new NoOpAuditProvider(sp.GetRequiredService<TimeProvider>()));
        }
    }

    // [SG Hook]
    static partial void ValidateBlockCustom(VKPersistenceOptions options, List<string> failures)
    {
        if (options.DefaultCommandTimeoutSeconds <= 0)
        {
            failures.Add("DefaultCommandTimeoutSeconds must be greater than 0.");
        }
        if (options.DefaultPageSize <= 0 || options.DefaultPageSize > options.MaxPageSize)
        {
            failures.Add($"DefaultPageSize must be between 1 and {options.MaxPageSize}.");
        }
        if (options.MaxPageSize <= 0)
        {
            failures.Add("MaxPageSize must be greater than 0.");
        }
        if (options.ConcurrencyRetryCount < 0)
        {
            failures.Add("ConcurrencyRetryCount must be 0 or greater.");
        }
    }
}

