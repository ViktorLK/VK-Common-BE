using System;
using Microsoft.AspNetCore.Http;
namespace VK.Blocks.Web;

/// <summary>
/// Defines a factory for creating <see cref="VKWebProblemDetails"/> instances.
/// </summary>
public interface IVKProblemDetailsFactory
{
    /// <summary>
    /// Creates a <see cref="VKWebProblemDetails"/> instance for the specified exception and status code.
    /// </summary>
    VKWebProblemDetails Create(HttpContext context, Exception exception, int statusCode, string? errorCode = null);
}
