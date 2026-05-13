using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Text.Internal;

/// <summary>
/// Validates the <see cref="VKTextOptions"/>.
/// </summary>
internal sealed class TextOptionsValidator : IValidateOptions<VKTextOptions>
{
    public ValidateOptionsResult Validate(string? name, VKTextOptions options)
    {
        if (options == null)
            return ValidateOptionsResult.Fail("Options cannot be null.");

        if (options.Enabled && string.IsNullOrWhiteSpace(options.ModelId) && options.Provider != null)
        {
            // Note: Some providers might have a default model, so we don't strictly enforce ModelId here
            // but it's good practice to have it.
        }

        return ValidateOptionsResult.Success;
    }
}
