using Microsoft.Extensions.Options;

namespace VK.Blocks.Authentication.Options;

/// <summary>
/// Validates the <see cref="VKAuthenticationOptions"/> during application startup.
/// </summary>
public sealed class VKAuthenticationOptionsValidator : IValidateOptions<VKAuthenticationOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKAuthenticationOptions options)
    {
        // If authentication is disabled, we don't need to strictly validate its sub-components
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        // Validate JWT settings if enabled
        if (options.Jwt == null)
        {
            return ValidateOptionsResult.Fail("Authentication is enabled, but the 'Jwt' section is missing in the configuration.");
        }

        if (string.IsNullOrWhiteSpace(options.Jwt.SecretKey))
        {
            return ValidateOptionsResult.Fail($"A valid JWT SecretKey is required when Authentication is enabled. Please check the '{VKAuthenticationOptions.SectionName}:Jwt:SecretKey' configuration value.");
        }

        return ValidateOptionsResult.Success;
    }
}
