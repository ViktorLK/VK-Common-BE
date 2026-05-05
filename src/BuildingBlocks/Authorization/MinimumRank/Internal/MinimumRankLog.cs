using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authorization.MinimumRank.Internal;

/// <summary>
/// Source-generated logging for the MinimumRank feature.
/// </summary>
internal static partial class MinimumRankLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Minimum rank authorization succeeded for user {UserId}. User rank {UserRank} satisfies minimum {MinimumRank}.")]
    internal static partial void LogAuthorizationSucceeded(
        this ILogger logger,
        string userId,
        int userRank,
        int minimumRank);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Minimum rank authorization failed for user {UserId}. User rank {UserRank} is below minimum {MinimumRank}.")]
    internal static partial void LogAuthorizationFailed(
        this ILogger logger,
        string userId,
        int userRank,
        int minimumRank);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Minimum rank authorization failed for user {UserId}. Missing or invalid rank claim.")]
    internal static partial void LogMissingRankClaim(
        this ILogger logger,
        string userId);
}
