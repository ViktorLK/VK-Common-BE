using Microsoft.Extensions.Options;

namespace VK.Blocks.Authorization.Roles.Internal;

/// <summary>
/// Validator for <see cref="VKRoleOptions"/>.
/// </summary>
internal sealed class RoleOptionsValidator : IValidateOptions<VKRoleOptions>
{
    private static readonly string Prefix = $"[{VKRoleOptions.SectionName}] ";

    public ValidateOptionsResult Validate(string? name, VKRoleOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.RoleClaimType))
        {
            return ValidateOptionsResult.Fail($"{Prefix}RoleClaimType cannot be null or whitespace.");
        }

        return ValidateOptionsResult.Success;
    }
}
