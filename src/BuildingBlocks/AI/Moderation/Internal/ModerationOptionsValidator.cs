using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Moderation.Internal;

/// <summary>
/// Validator for <see cref="VKModerationOptions"/>.
/// </summary>
internal sealed class ModerationOptionsValidator : IValidateOptions<VKModerationOptions>
{
    public ValidateOptionsResult Validate(string? name, VKModerationOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Moderation options cannot be null.");
        }

        if (options.Enabled)
        {
            if (string.IsNullOrWhiteSpace(options.ModelId))
            {
                return ValidateOptionsResult.Fail("ModelId is required when Moderation is enabled.");
            }

            if (options.SensitivityThreshold < 0 || options.SensitivityThreshold > 1)
            {
                return ValidateOptionsResult.Fail("SensitivityThreshold must be between 0 and 1.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}
