using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authorization.Features.MinimumRank;

/// <summary>
/// Source-generated logging for the MinimumRank feature.
/// </summary>
internal static partial class MinimumRankLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Minimum rank authorization succeeded for user {UserId}. User rank {UserRank} satisfies minimum {MinimumRank}.")]
    public static partial void LogAuthorizationSucceeded(
        this ILogger logger,
        string userId,
        int userRank,
        int minimumRank);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Minimum rank authorization failed for user {UserId}. User rank {UserRank} is below minimum {MinimumRank}.")]
    public static partial void LogAuthorizationFailed(
        this ILogger logger,
        string userId,
        int userRank,
        int minimumRank);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Minimum rank authorization failed for user {UserId}. Missing or invalid rank claim.")]
    public static partial void LogMissingRankClaim(
        this ILogger logger,
        string userId);
}
