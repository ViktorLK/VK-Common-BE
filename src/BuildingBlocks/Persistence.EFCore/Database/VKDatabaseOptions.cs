using System;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Configuration options for the Database feature.
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>
[VKFeature(typeof(VKPersistenceEFCoreBlock))]
public sealed partial record VKDatabaseOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the database connection string.
    /// Mandatory property (AP.01).
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>
    /// Gets the command timeout in seconds.
    /// Default is 30 seconds.
    /// </summary>
    public int CommandTimeout { get; init; } = 30;

    /// <summary>
    /// Gets a value indicating whether sensitive data logging is enabled.
    /// WARNING: Set to false in production to prevent PII leakage.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether detailed errors are enabled.
    /// </summary>
    public bool EnableDetailedErrors { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether AsNoTracking is applied by default (CS.04).
    /// </summary>
    public bool UseNoTrackingByDefault { get; init; } = true;

    /// <summary>
    /// Gets the maximum number of retry attempts (OR.03).
    /// </summary>
    public int MaxRetryCount { get; init; } = 3;

    /// <summary>
    /// Gets the maximum delay between retries.
    /// </summary>
    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(5);
}
