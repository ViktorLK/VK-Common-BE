using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Features.OAuth.Internal;

namespace VK.Blocks.Authentication.Features.OAuth;

/// <summary>
/// Validates the <see cref="VKOAuthOptions"/> during application startup.
/// </summary>
public sealed class VKOAuthOptionsValidator : IValidateOptions<VKOAuthOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKOAuthOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (options.Providers is null || options.Providers.Count is 0)
        {
            return ValidateOptionsResult.Fail(VKOAuthErrors.MissingProviders);
        }

        foreach (var (providerName, provider) in options.Providers.Where(p => p.Value.Enabled))
        {
            if (string.IsNullOrWhiteSpace(provider.ClientId))
            {
                return ValidateOptionsResult.Fail(string.Format(VKOAuthErrors.MissingClientIdTemplate, providerName));
            }

            if (string.IsNullOrWhiteSpace(provider.ClientSecret))
            {
                return ValidateOptionsResult.Fail(string.Format(VKOAuthErrors.MissingClientSecretTemplate, providerName));
            }

            if (string.IsNullOrWhiteSpace(provider.Authority))
            {
                return ValidateOptionsResult.Fail(string.Format(VKOAuthErrors.MissingAuthorityTemplate, providerName));
            }

            if (string.IsNullOrWhiteSpace(provider.CallbackPath))
            {
                return ValidateOptionsResult.Fail(string.Format(VKOAuthErrors.MissingCallbackPathTemplate, providerName));
            }
        }

        return ValidateOptionsResult.Success;
    }
}
