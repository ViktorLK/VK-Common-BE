using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace VK.Blocks.Persistence.EFCore.Sqlite.Internal;

/// <summary>
/// Model convention contributor for SQLite that applies <see cref="DateTimeOffsetToStringConverter"/>
/// to all <see cref="DateTimeOffset"/> and nullable <see cref="DateTimeOffset"/> properties.
/// Ensures native SQL ORDER BY and comparison support while maintaining backward compatibility with existing TEXT data.
/// Follows AP.01 (sealed class default) and CS.08.
/// </summary>
internal sealed class SqliteModelConventionContributor : IVKModelConventionContributor // [AP.01] [CS.08]
{
    public void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToStringConverter>();
    }
}
