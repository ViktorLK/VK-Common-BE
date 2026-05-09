using Microsoft.AspNetCore.Http;

namespace VK.Blocks.Web;

/// <summary>
/// Defines a contract for providing correlation IDs for HTTP requests.
/// </summary>
public interface IVKCorrelationIdProvider
{
    /// <summary>
    /// Gets or generates a correlation ID for the specified HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context of the current request.</param>
    /// <param name="options">The correlation ID configuration options.</param>
    /// <returns>A correlation ID string.</returns>
    string GetCorrelationId(HttpContext context, VKCorrelationIdOptions options);
}
