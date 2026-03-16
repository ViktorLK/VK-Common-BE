namespace VK.Blocks.Observability.Serilog;

/// <summary>
/// Contains constant property names used across Serilog enrichers.
/// Complies with VK.Blocks Rule 13 for constant visibility.
/// </summary>
public static class SerilogPropertyNames
{
    /// <summary>The trace identifier for distributed tracing.</summary>
    public const string TraceId = "TraceId";

    /// <summary>The span identifier for distributed tracing.</summary>
    public const string SpanId = "SpanId";

    /// <summary>The parent span identifier for distributed tracing.</summary>
    public const string ParentId = "ParentId";

    /// <summary>The name of the hosting application.</summary>
    public const string ApplicationName = "ApplicationName";

    /// <summary>The current hosting environment (e.g., Development, Production).</summary>
    public const string Environment = "Environment";

    /// <summary>The version of the entry assembly.</summary>
    public const string Version = "Version";

    /// <summary>The user identifier from the current execution context.</summary>
    public const string UserId = "UserId";

    /// <summary>The tenant identifier from the current execution context.</summary>
    public const string TenantId = "TenantId";
}
