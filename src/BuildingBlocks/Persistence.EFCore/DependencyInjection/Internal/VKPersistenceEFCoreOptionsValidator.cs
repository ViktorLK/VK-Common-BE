using Microsoft.Extensions.Options;

namespace VK.Blocks.Persistence.EFCore.DependencyInjection.Internal;

internal sealed class VKPersistenceEFCoreOptionsValidator : IValidateOptions<VKPersistenceEFCoreOptions>
{
    public ValidateOptionsResult Validate(string? name, VKPersistenceEFCoreOptions options)
    {
        // Add validation logic if needed
        return ValidateOptionsResult.Success;
    }
}
