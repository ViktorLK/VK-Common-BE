using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Afferent.Common.DependencyInjection.Internal;

/// <summary>
/// Validator for <see cref="VKAIAfferentOptions"/>.
/// Complies with AP.01.
/// </summary>
internal sealed class AIAfferentOptionsValidator : IValidateOptions<VKAIAfferentOptions>
{
    public ValidateOptionsResult Validate(string? name, VKAIAfferentOptions options)
    {
        return ValidateOptionsResult.Success;
    }
}
