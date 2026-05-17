using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Cognitive.Common.DependencyInjection.Internal;

/// <summary>
/// Validator for AI Cognitive options.
/// </summary>
internal sealed class VKAICognitiveOptionsValidator : IValidateOptions<VKAICognitiveOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKAICognitiveOptions options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("Options cannot be null.");
        }

        // Add specific validation logic here as the module grows.
        return ValidateOptionsResult.Success;
    }
}
