using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authentication.Abstractions;

/// <summary>
/// Source-generated logger for Claims transformation events.
/// </summary>
internal static partial class ClaimsLog
{
    #region Logger Messages

    /// <summary>
    /// Logs an error that occurred during claims transformation for a specific user.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="userId">The identifier of the user.</param>
    [LoggerMessage(
        EventId = 2201,
        Level = LogLevel.Error,
        Message = "Error occurring during claims transformation for UserId: {UserId}")]
    public static partial void LogClaimsTransformationError(this ILogger logger, Exception ex, string? userId);

    #endregion
}
