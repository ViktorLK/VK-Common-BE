using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Authorization.DependencyInjection;
using VK.Blocks.Authorization.Diagnostics;
using VK.Blocks.Authorization.Features.MinimumRank.Internal;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.MinimumRank;

/// <summary>
/// Evaluates <see cref="MinimumRankRequirement"/> against the user's rank.
/// Supports SuperAdmin bypass.
/// </summary>
public sealed class MinimumRankAuthorizationHandler(
    IRankProvider rankProvider,
    IOptions<VKAuthorizationOptions> options,
    ILogger<MinimumRankAuthorizationHandler> logger)
    : AuthorizationHandler<MinimumRankRequirement>, IMinimumRankEvaluator
{
    private static string PolicyName => MinimumRankConstants.PolicyName;

    private readonly VKAuthorizationOptions _options = options.Value;

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumRankRequirement requirement)
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
    public async ValueTask<Result<bool>> HasMinimumRankAsync(
        ClaimsPrincipal user,
        int minimumRank,
        Type? enumType = null,
        CancellationToken ct = default)
    {
        var userId = user.Identity?.Name ?? "Unknown";

        // 1. SuperAdmin Bypass Logic (Centralized via extension)
        if (user.IsSuperAdmin(_options))
        {
            logger.LogAuthorizationSucceeded(userId, 99999, minimumRank);
            return Result.Success(true);
        }

        var sw = Stopwatch.StartNew();

        // 2. Resolve rank via provider
        var rankValueStr = await rankProvider.GetRankAsync(user, ct).ConfigureAwait(false);

        if (string.IsNullOrEmpty(rankValueStr))
        {
            sw.RecordEvaluation(PolicyName, Result.Success(false));
            logger.LogMissingRankClaim(userId);
            return Result.Success(false);
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
            sw.RecordEvaluation(PolicyName, Result.Success(false));
            logger.LogMissingRankClaim(userId);
            return Result.Success(false);
        }

        // 4. Compare and Record
        var isAllowed = userRankValue >= minimumRank;
        sw.RecordEvaluation(PolicyName, Result.Success(isAllowed));

        if (isAllowed)
        {
            logger.LogAuthorizationSucceeded(userId, userRankValue, minimumRank);
            return Result.Success(true);
        }

        logger.LogAuthorizationFailed(userId, userRankValue, minimumRank);
        return Result.Success(false);
    }
}
