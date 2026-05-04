using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.MinimumRank.Internal;

/// <summary>
/// Handles <see cref="VKMinimumRankRequirement"/> by evaluating the user's rank.
/// Also provides programmatic evaluation via <see cref="IVKMinimumRankEvaluator"/>.
/// </summary>
internal sealed class MinimumRankAuthorizationHandler(
    IVKRankProvider rankProvider,
    IOptions<VKAuthorizationOptions> globalOptions,
    ILogger<MinimumRankAuthorizationHandler> logger)
    : AuthorizationHandler<VKMinimumRankRequirement>, IVKMinimumRankEvaluator
{
    private static string PolicyName => MinimumRankConstants.FeatureName;

    private readonly IVKRankProvider _rankProvider = VKGuard.NotNull(rankProvider);
    private readonly VKAuthorizationOptions _globalOptions = VKGuard.NotNull(globalOptions).Value;
    private readonly ILogger<MinimumRankAuthorizationHandler> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VKMinimumRankRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var result = await HasMinimumRankAsync(
                context.User,
                requirement.MinimumRankValue,
                requirement.EnumType)
            .ConfigureAwait(false);

        context.ApplyResult(requirement, result, this);
    }

    /// <inheritdoc />
    public async ValueTask<VKResult<bool>> HasMinimumRankAsync(
        ClaimsPrincipal user,
        int minimumRank,
        Type? enumType = null,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(user);
        var userId = user.Identity?.Name ?? VKBlocksConstants.UnknownIdentity;

        // 1. SuperAdmin Bypass Logic (Centralized via extension)
        if (user.IsSuperAdmin(_globalOptions))
        {
            _logger.LogAuthorizationSucceeded(userId, 99999, minimumRank);
            return VKResult.Success(true);
        }


        var sw = Stopwatch.StartNew();

        // 2. Resolve rank via provider
        var rankValueStr = await _rankProvider.GetRankAsync(user, ct).ConfigureAwait(false);

        if (string.IsNullOrEmpty(rankValueStr))
        {
            sw.RecordEvaluation(PolicyName, VKResult.Success(false));
            _logger.LogMissingRankClaim(userId);
            return VKResult.Success(false);
        }


        // 3. Parse rank
        int userRankValue;
        if (int.TryParse(rankValueStr, out var parsedInt))
        {
            userRankValue = parsedInt;
        }
        else if (enumType is not null && Enum.TryParse(enumType, rankValueStr, out var parsedEnum) && parsedEnum is not null)
        {
            userRankValue = (int)parsedEnum;
        }
        else
        {
            sw.RecordEvaluation(PolicyName, VKResult.Success(false));
            _logger.LogMissingRankClaim(userId);
            return VKResult.Success(false);
        }


        // 4. Compare and Record
        var isAllowed = userRankValue >= minimumRank;
        sw.RecordEvaluation(PolicyName, VKResult.Success(isAllowed));

        if (isAllowed)
        {
            _logger.LogAuthorizationSucceeded(userId, userRankValue, minimumRank);
            return VKResult.Success(true);
        }

        _logger.LogAuthorizationFailed(userId, userRankValue, minimumRank);
        return VKResult.Success(false);
    }
}
