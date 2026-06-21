using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Ingest.Common.DependencyInjection.Internal;

internal sealed class AIIngestOptionsValidator : IValidateOptions<VKAIIngestOptions>
{
    public ValidateOptionsResult Validate(string? name, VKAIIngestOptions options)
    {
        return ValidateOptionsResult.Success;
    }
}
