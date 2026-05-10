using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Tokenics.Internal;

/// <summary>
/// Validator for <see cref="VKTokenicsOptions"/>.
/// </summary>
internal sealed class TokenicsOptionsValidator : IValidateOptions<VKTokenicsOptions>
{
    public ValidateOptionsResult Validate(string? name, VKTokenicsOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Tokenics options cannot be null.");
        }

        if (options.Enabled)
        {
            if (options.GlobalTokenLimit.HasValue && options.GlobalTokenLimit.Value < 0)
            {
                return ValidateOptionsResult.Fail("GlobalTokenLimit must be greater than or equal to 0.");
            }

            if (options.RateLimitPerMinute.HasValue && options.RateLimitPerMinute.Value <= 0)
            {
                return ValidateOptionsResult.Fail("RateLimitPerMinute must be greater than zero.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}
