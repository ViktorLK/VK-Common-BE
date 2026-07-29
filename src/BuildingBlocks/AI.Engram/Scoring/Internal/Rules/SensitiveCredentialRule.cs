using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Scoring.Internal.Rules;

/// <summary>
/// Rule for intercepting sensitive credentials (passwords, tokens, secret keys).
/// </summary>
internal sealed class SensitiveCredentialRule : IVKScoringRule
{
    private static readonly Regex PasswordRegex = new(
        @"\b(password|passwd|pwd|口令|密码|secret_key|private_key|token|access_token|api_key)\s*[:=]\s*\S+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public Task<VKResult<VKScoringResult?>> EvaluateAsync(VKScoringContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (PasswordRegex.IsMatch(context.Content))
        {
            var res = VKScoringResult.Reject("Sensitive credential detected in memory content");
            return Task.FromResult(VKResult.Success<VKScoringResult?>(res));
        }

        return Task.FromResult(VKResult.Success<VKScoringResult?>(null));
    }
}
