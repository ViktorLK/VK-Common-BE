using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Embeddings.Internal;

/// <summary>
/// Validator for <see cref="VKEmbeddingOptions"/>.
/// </summary>
internal sealed class EmbeddingsOptionsValidator : IValidateOptions<VKEmbeddingOptions>
{
    public ValidateOptionsResult Validate(string? name, VKEmbeddingOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Embeddings options cannot be null.");
        }

        if (options.Enabled && options.Dimensions is <= 0)
        {
            return ValidateOptionsResult.Fail("Dimensions must be positive.");
        }

        return ValidateOptionsResult.Success;
    }
}
