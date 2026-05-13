using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.VectorStore.Sqlite.DependencyInjection.Internal;

/// <summary>
/// Validates <see cref="VKAIVectorStoreSqliteOptions"/>.
/// </summary>
internal sealed class AIVectorStoreSqliteOptionsValidator : IValidateOptions<VKAIVectorStoreSqliteOptions>
{
    public ValidateOptionsResult Validate(string? name, VKAIVectorStoreSqliteOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.Connection))
        {
            return ValidateOptionsResult.Fail("SQLite Connection string is required when AI Vector Store Sqlite is enabled.");
        }

        return ValidateOptionsResult.Success;
    }
}
