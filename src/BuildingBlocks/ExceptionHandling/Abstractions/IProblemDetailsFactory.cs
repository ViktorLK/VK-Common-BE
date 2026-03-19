using System;
using Microsoft.AspNetCore.Http;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;

namespace VK.Blocks.ExceptionHandling.Abstractions;

/// <summary>
/// Defines a factory for creating <see cref="VKProblemDetails"/> instances.
/// </summary>
public interface IProblemDetailsFactory
{
    /// <summary>
    /// Creates a <see cref="VKProblemDetails"/> instance for the specified exception and status code.
    /// </summary>
    VKProblemDetails Create(HttpContext context, Exception exception, int statusCode, string? errorCode = null);
}
