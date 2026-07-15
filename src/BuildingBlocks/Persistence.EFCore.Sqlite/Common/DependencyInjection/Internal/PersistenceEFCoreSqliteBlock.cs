using System;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Sqlite.Common.DependencyInjection.Internal;

// [SG Registration]
internal sealed partial class PersistenceEFCoreSqliteBlock
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKPersistenceEFCoreSqliteBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        var options = services.GetVKServiceInstance<VKPersistenceEFCoreSqliteOptions>()!;

        // 2. Options Registration

        // 3. Mark-Self

        // 4. Options Validation

        // 5. Feature Toggle
    }
}
