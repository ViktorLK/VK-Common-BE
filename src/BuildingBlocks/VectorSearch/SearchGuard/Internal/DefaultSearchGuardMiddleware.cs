using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Common.Diagnostics.Internal;

namespace VK.Blocks.VectorSearch.SearchGuard.Internal;

/// <summary>
/// Upgraded middleware to perform advanced query guard checks (SQLi, Prompt Injection, and Blocklist checks) before invoking stages.
/// </summary>
internal sealed partial class DefaultSearchGuardMiddleware : IVKVectorSearchMiddleware
{
    private readonly VKSearchGuardOptions _options;
    private readonly ILogger<DefaultSearchGuardMiddleware> _logger;

    [GeneratedRegex(@"union\s+select|;\s*drop\s+table|;\s*delete\s+from|'\s*or\s*'.*?\s*=\s*'.*?'|--", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SqlInjectionRegex();

    [GeneratedRegex(@"ignore\s+all\s+previous\s+instructions|ignore\s+system\s+prompt|you\s+are\s+now\s+a\b|bypass\s+restrictions", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PromptInjectionRegex();

    public DefaultSearchGuardMiddleware(
        IOptions<VKSearchGuardOptions> options,
        ILogger<DefaultSearchGuardMiddleware> logger)
    {
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public int MiddlewareOrder => VKVectorSearchPipelineScheduler.Middleware.SearchGuard;

    public async Task<VKResult> InvokeAsync(
        VKVectorSearchContext context,
        VKPipelineDelegate next,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);
        VKGuard.NotNull(next);

        if (!_options.Enabled)
        {
            return await next().ConfigureAwait(false); // [CS.03]
        }

        var text = context.Query.Text;

        // 1. Length boundaries check
        if (text.Length < _options.MinLength)
        {
            _logger.SearchGuardBlocked(text, $"Length {text.Length} is less than MinLength {_options.MinLength}");
            return VKResult.Failure(VKVectorSearchPipelineErrors.QueryTooShort);
        }

        if (text.Length > _options.MaxLength)
        {
            _logger.SearchGuardBlocked(text, $"Length {text.Length} exceeds MaxLength {_options.MaxLength}");
            return VKResult.Failure(VKVectorSearchPipelineErrors.QueryTooLong);
        }

        // 2. SQL Injection check
        if (_options.EnableSqlInjectionProtection && SqlInjectionRegex().IsMatch(text))
        {
            _logger.SearchGuardBlocked(text, "SQL Injection pattern detected.");
            return VKResult.Failure(VKVectorSearchPipelineErrors.QuerySecurityViolation);
        }

        // 3. Prompt Injection / Jailbreak check
        if (_options.EnablePromptInjectionProtection && PromptInjectionRegex().IsMatch(text))
        {
            _logger.SearchGuardBlocked(text, "Prompt Injection / Jailbreak pattern detected.");
            return VKResult.Failure(VKVectorSearchPipelineErrors.QuerySecurityViolation);
        }

        // 4. Custom blocklist phrases check
        if (_options.BlockedPhrases.Count > 0)
        {
            foreach (var phrase in _options.BlockedPhrases)
            {
                if (text.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.SearchGuardBlocked(text, $"Blocked phrase '{phrase}' matched.");
                    return VKResult.Failure(VKVectorSearchPipelineErrors.QuerySecurityViolation);
                }
            }
        }

        return await next().ConfigureAwait(false); // [CS.03]
    }
}
