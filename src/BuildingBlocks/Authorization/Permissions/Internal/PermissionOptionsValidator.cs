using Microsoft.Extensions.Options;

namespace VK.Blocks.Authorization.Permissions.Internal;

/// <summary>
/// Validator for <see cref="VKPermissionOptions"/>.
/// </summary>
internal sealed class PermissionOptionsValidator : IValidateOptions<VKPermissionOptions>
{
    private static readonly string Prefix = $"[{VKPermissionOptions.SectionName}] ";

    public ValidateOptionsResult Validate(string? name, VKPermissionOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.PermissionClaimType))
        {
            return ValidateOptionsResult.Fail($"{Prefix}PermissionClaimType cannot be null or whitespace.");
        }

        return ValidateOptionsResult.Success;
    }
}
