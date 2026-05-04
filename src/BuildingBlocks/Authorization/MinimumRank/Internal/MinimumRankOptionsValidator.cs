using Microsoft.Extensions.Options;

namespace VK.Blocks.Authorization.MinimumRank.Internal;

/// <summary>
/// Validator for <see cref="VKMinimumRankOptions"/>.
/// </summary>
internal sealed class MinimumRankOptionsValidator : IValidateOptions<VKMinimumRankOptions>
{
    private static readonly string Prefix = $"[{VKMinimumRankOptions.SectionName}] ";

    public ValidateOptionsResult Validate(string? name, VKMinimumRankOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.RankClaimType))
        {
            return ValidateOptionsResult.Fail($"{Prefix}RankClaimType cannot be null or whitespace.");
        }

        return ValidateOptionsResult.Success;
    }
}
