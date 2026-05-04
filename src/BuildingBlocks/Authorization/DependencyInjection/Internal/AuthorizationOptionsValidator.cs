using Microsoft.Extensions.Options;

namespace VK.Blocks.Authorization.DependencyInjection.Internal;

/// <summary>
/// Comprehensive validator for <see cref="VKAuthorizationOptions"/> configuration.
/// </summary>
internal sealed class AuthorizationOptionsValidator : IValidateOptions<VKAuthorizationOptions>
{
    private const string Prefix = $"[{VKAuthorizationBlock.BlockName} Check] ";

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKAuthorizationOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.RoleClaimType))
        {
            return ValidateOptionsResult.Fail($"{Prefix}RoleClaimType cannot be null or whitespace. This is required for SuperAdmin bypass evaluation.");
        }

        // SuperAdminRole can be null (to disable bypass), but if provided, it must not be empty/whitespace
        if (options.SuperAdminRole is not null && string.IsNullOrWhiteSpace(options.SuperAdminRole))
        {
            return ValidateOptionsResult.Fail($"{Prefix}SuperAdminRole must be null or a non-whitespace string.");
        }

        return ValidateOptionsResult.Success;
    }
}
