using Microsoft.Extensions.Options;

namespace VK.Blocks.Persistence.Sqlite.DependencyInjection.Internal;

internal sealed class SqliteOptionsValidator : IValidateOptions<VKPersistenceSqliteOptions>
{
    public ValidateOptionsResult Validate(string? name, VKPersistenceSqliteOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail("SQLite connection string must not be empty.");
        }

        return ValidateOptionsResult.Success;
    }
}
