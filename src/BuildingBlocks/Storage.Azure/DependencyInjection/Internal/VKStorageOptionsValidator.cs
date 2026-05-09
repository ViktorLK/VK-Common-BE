using Microsoft.Extensions.Options;

namespace VK.Blocks.Storage.Azure.DependencyInjection.Internal;

public sealed class VKStorageOptionsValidator : IValidateOptions<VKStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, VKStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString) && string.IsNullOrWhiteSpace(options.ServiceUri))
        {
            return ValidateOptionsResult.Fail("Either ConnectionString or ServiceUri must be provided in StorageStorage settings.");
        }
        if (options.MaxFileSizeBytes <= 0)
        {
            return ValidateOptionsResult.Fail("MaxFileSizeBytes must be greater than zero.");
        }
        if (options.AllowedExtensions == null || options.AllowedExtensions.Length == 0)
        {
            return ValidateOptionsResult.Fail("At least one allowed extension must be configured.");
        }
        return ValidateOptionsResult.Success;
    }
}
