using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.VectorStore.DependencyInjection.Internal;

internal sealed class AIVectorStoreOptionsValidator : IValidateOptions<VKAIVectorStoreOptions>
{
    public ValidateOptionsResult Validate(string? name, VKAIVectorStoreOptions options)
    {
        if (options.Enabled && options.Type != VKAIVectorStoreType.InMemory && string.IsNullOrWhiteSpace(options.Connection))
        {
            return ValidateOptionsResult.Fail("Connection string is required for non-InMemory vector stores.");
        }

        return ValidateOptionsResult.Success;
    }
}
