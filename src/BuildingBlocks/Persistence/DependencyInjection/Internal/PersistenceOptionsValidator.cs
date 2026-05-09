using Microsoft.Extensions.Options;

namespace VK.Blocks.Persistence.DependencyInjection.Internal;

/// <summary>
/// Validator for <see cref="VKPersistenceOptions"/>.
/// </summary>
internal sealed class PersistenceOptionsValidator : IValidateOptions<VKPersistenceOptions>
{
    public ValidateOptionsResult Validate(string? name, VKPersistenceOptions options)
    {
        // Add validation logic here if needed.
        return ValidateOptionsResult.Success;
    }
}
