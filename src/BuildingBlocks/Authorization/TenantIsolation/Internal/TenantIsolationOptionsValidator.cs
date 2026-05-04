using Microsoft.Extensions.Options;

namespace VK.Blocks.Authorization.TenantIsolation.Internal;

/// <summary>
/// Validator for <see cref="VKTenantIsolationOptions"/>.
/// </summary>
internal sealed class TenantIsolationOptionsValidator : IValidateOptions<VKTenantIsolationOptions>
{
    private static readonly string Prefix = $"[{VKTenantIsolationOptions.SectionName}] ";

    public ValidateOptionsResult Validate(string? name, VKTenantIsolationOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.TenantClaimType))
        {
            return ValidateOptionsResult.Fail($"{Prefix}TenantClaimType cannot be null or whitespace.");
        }

        return ValidateOptionsResult.Success;
    }
}
