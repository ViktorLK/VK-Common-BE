using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.VectorStore.Retrieval.Internal;

/// <summary>
/// Validator for <see cref="VKRetrievalOptions"/>.
/// </summary>
internal sealed class RetrievalOptionsValidator : IValidateOptions<VKRetrievalOptions>
{
    public ValidateOptionsResult Validate(string? name, VKRetrievalOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Retrieval options cannot be null.");
        }

        if (options.Enabled && string.IsNullOrWhiteSpace(options.DefaultCollection))
        {
            return ValidateOptionsResult.Fail("DefaultCollection is required when Retrieval is enabled.");
        }

        return ValidateOptionsResult.Success;
    }
}
