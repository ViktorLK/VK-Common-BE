using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.VectorStore.Common.DependencyInjection.Internal;

internal sealed class AIVectorStoreOptionsValidator : IValidateOptions<VKAIVectorStoreOptions>
{
    public ValidateOptionsResult Validate(string? name, VKAIVectorStoreOptions options)
    {
        // Global validation logic here.
        // Specific connection validation is now handled by provider-specific validators.
        return ValidateOptionsResult.Success;
    }
}
