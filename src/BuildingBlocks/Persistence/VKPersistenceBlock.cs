using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Persistence.Auditing.Internal;

namespace VK.Blocks.Persistence;

/// <summary>
/// A marker type for the VK.Blocks.Persistence building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKPersistenceBlock
{
    static partial void RegisterBlockCustom(IVKPersistenceBuilder builder)
    {
        builder.Services.TryAddScoped<IVKAuditProvider, NoOpAuditProvider>();
    }

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
