using System;
using Microsoft.AspNetCore.Http;

namespace VK.Blocks.ExceptionHandling.Abstractions.Contracts;

/// <summary>
/// Provides context for the exception being handled.
/// </summary>
public sealed class ExceptionContext(HttpContext httpContext, Exception exception)
{
    /// <summary>
    /// Gets the current HTTP context.
    /// </summary>
    public HttpContext HttpContext { get; } = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

    /// <summary>
    /// Gets the exception that occurred.
    /// </summary>
    public Exception Exception { get; } = exception ?? throw new ArgumentNullException(nameof(exception));

    /// <summary>
    /// Gets or sets the trace identifier associated with the request.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the exception has been handled.
    /// </summary>
    public bool Handled { get; set; }
}
