using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Corpus.Common.DependencyInjection.Internal;

/// <summary>
/// Validates options bound to <see cref="VKCorpusOptions"/>.
/// Follows BB.05 / AP.01.
/// </summary>
internal sealed class VKCorpusOptionsValidator : IValidateOptions<VKCorpusOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKCorpusOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Corpus options are null.");
        }

        return ValidateOptionsResult.Success;
    }
}
