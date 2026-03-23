namespace VK.Blocks.Observability.EfCore.Options;

/// <summary>
/// Configuration options for EF Core observability features.
/// </summary>
public sealed class EfCoreObservabilityOptions
{
    /// <summary>
    /// The threshold duration for logging a query as a slow query warning.
    /// Default is 1 second.
    /// </summary>
    public TimeSpan SlowQueryThreshold { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Indicates whether to log parameter values in SQL queries.
    /// Default is false.
    /// </summary>
    public bool LogParameterValues { get; set; } = false;

    /// <summary>
    /// Indicates whether to mask sensitive data in log outputs.
    /// Default is true.
    /// </summary>
    public bool MaskSensitiveData { get; set; } = true;
}
