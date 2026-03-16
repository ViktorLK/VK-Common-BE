namespace VK.Blocks.Observability.AspNetCore;

/// <summary>
/// Contains constant property names used across ASP.NET Core observability enrichers.
/// Complies with VK.Blocks Rule 13 for constant visibility.
/// </summary>
public static class HttpLogPropertyNames
{
    /// <summary>The HTTP method (e.g., GET, POST).</summary>
    public const string Method = "Http.Method";

    /// <summary>The request path.</summary>
    public const string Path = "Http.Path";

    /// <summary>The HTTP scheme (e.g., http, https).</summary>
    public const string Scheme = "Http.Scheme";

    /// <summary>The host name.</summary>
    public const string Host = "Http.Host";

    /// <summary>The trace identifier for distributed tracing.</summary>
    public const string TraceId = "TraceId";

    /// <summary>The span identifier for distributed tracing.</summary>
    public const string SpanId = "SpanId";

    /// <summary>The unique request identifier defined by ASP.NET Core.</summary>
    public const string RequestId = "RequestId";

    /// <summary>The client's IP address.</summary>
    public const string ClientIp = "ClientIp";
}
