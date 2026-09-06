using Microsoft.Extensions.Options;

namespace VK.Blocks.Infrastructure.Azure.DependencyInjection.Internal;

/// <summary>
/// Validator for Azure shared options.
/// </summary>
internal sealed class AzureSharedOptionsValidator : IValidateOptions<VKAzureSharedOptions>
{
    public ValidateOptionsResult Validate(string? name, VKAzureSharedOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        // Basic validation
        if (string.IsNullOrWhiteSpace(options.ConnectionString) &&
            !options.Identity.UseManagedIdentity &&
            string.IsNullOrWhiteSpace(options.Identity.ClientSecret))
        {
            return ValidateOptionsResult.Fail("Either ConnectionString or Identity credentials must be provided for Azure Infrastructure.");
        }

        return ValidateOptionsResult.Success;
    }
}
