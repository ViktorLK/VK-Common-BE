using System;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Configuration options for the EF Core persistence layer.
/// </summary>
public sealed record VKPersistenceEFCoreOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:Persistence:EFCore";

    /// <summary>
    /// Gets a value indicating whether the persistence block is enabled.
    /// Default is <c>true</c>.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether auditing is enabled.
    /// If null, falls back to the global Persistence options.
    /// </summary>
    public bool? EnableAuditing { get; init; }

    /// <summary>
    /// Gets a value indicating whether soft delete is enabled.
    /// If null, falls back to the global Persistence options.
    /// </summary>
    public bool? EnableSoftDelete { get; init; }

    /// <summary>
    /// Gets a value indicating whether multi-tenancy is enabled.
    /// If null, falls back to the global Persistence options.
    /// </summary>
    public bool? EnableMultiTenancy { get; init; }

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

