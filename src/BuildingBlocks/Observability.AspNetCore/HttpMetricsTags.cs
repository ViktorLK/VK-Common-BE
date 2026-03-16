namespace VK.Blocks.Observability.AspNetCore;

/// <summary>
/// Contains constant tag names used across ASP.NET Core metrics collectors.
/// Aligned with OpenTelemetry Semantic Conventions.
/// Complies with VK.Blocks Rule 13 for constant visibility.
/// </summary>
public static class HttpMetricsTags
{
    /// <summary>The HTTP request method.</summary>
    public const string Method = "http.request.method";

    /// <summary>The URL path.</summary>
    public const string Path = "url.path";

    /// <summary>The HTTP response status code.</summary>
    public const string StatusCode = "http.response.status_code";

    /// <summary>The type of error or exception that occurred.</summary>
    public const string ErrorType = "error.type";
}
