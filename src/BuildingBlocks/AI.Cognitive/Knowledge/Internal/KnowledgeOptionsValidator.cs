using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

/// <summary>
/// Validator for <see cref="VKKnowledgeOptions"/>.
/// </summary>
internal sealed class KnowledgeOptionsValidator : IValidateOptions<VKKnowledgeOptions>
{
    public ValidateOptionsResult Validate(string? name, VKKnowledgeOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Knowledge options cannot be null.");
        }

        if (options.MaxEntriesToInject < 0)
        {
            return ValidateOptionsResult.Fail("MaxEntriesToInject must be non-negative.");
        }

        if (options.ReservedTokens < 0)
        {
            return ValidateOptionsResult.Fail("ReservedTokens must be non-negative.");
        }

        if (options.SemanticThreshold is < 0 or > 1)
        {
            return ValidateOptionsResult.Fail("SemanticThreshold must be between 0 and 1.");
        }

        return ValidateOptionsResult.Success;
    }
}
