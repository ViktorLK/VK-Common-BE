using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authentication.Common;

/// <summary>
/// Source-generated logger for In-Memory cleanup events.
/// </summary>
internal static partial class InMemoryCleanupLog
{
    /// <summary>
    /// Logs that no active in-memory providers were detected and the service is stopping.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "In-Memory Cleanup Service: No active in-memory authentication providers detected. The background service will terminate to save system resources.")]
    public static partial void LogNoActiveProviders(this ILogger logger);

    /// <summary>
    /// Logs that the cleanup service is starting.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="count">The number of active providers.</param>
    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "In-Memory Cleanup Service: Starting. Monitoring {Count} active in-memory providers.")]
    public static partial void LogServiceStarting(this ILogger logger, int count);

    /// <summary>
    /// Logs that the service is evaluating cleanup for providers.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="count">The number of monitored providers.</param>
    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Debug,
        Message = "Evaluating cleanup for {Count} in-memory providers.")]
    public static partial void LogEvaluatingCleanup(this ILogger logger, int count);

    /// <summary>
    /// Logs that a provider is being skipped as it is not the active one in the container.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="providerType">The name of the provider type.</param>
    /// <param name="serviceType">The name of the service type.</param>
    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Debug,
        Message = "Skipping cleanup for {ProviderType} as it is not the active provider for {ServiceType}.")]
    public static partial void LogSkippingProvider(this ILogger logger, string providerType, string serviceType);

    /// <summary>
    /// Logs an error that occurred during cleanup.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="providerType">The name of the provider type.</param>
    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Error,
        Message = "Error occurring during cleanup of {ProviderType}.")]
    public static partial void LogCleanupError(this ILogger logger, Exception ex, string providerType);

    /// <summary>
    /// Logs that the cleanup service is stopping.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Information,
        Message = "In-Memory Cleanup Service: Stopping.")]
    public static partial void LogServiceStopping(this ILogger logger);
}
