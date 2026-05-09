using System;
using System.Linq;
using Microsoft.Extensions.Hosting;
using VK.Blocks.Core;

namespace VK.Blocks.Web.ProblemDetails.Internal;

/// <summary>
/// A mapper that converts a framework-agnostic <see cref="VKErrorResponse"/>
/// into a web-specific <see cref="VKWebProblemDetails"/>.
/// Uses Source Generation for the mapping infrastructure.
/// </summary>
[VKMapper(typeof(VKErrorResponse), typeof(VKWebProblemDetails))]
internal sealed partial class ExceptionProblemDetailsMapper(
    TimeProvider timeProvider,
    IHostEnvironment env)
{
    private readonly TimeProvider _timeProvider = VKGuard.NotNull(timeProvider);
    private readonly IHostEnvironment _env = VKGuard.NotNull(env);

    partial void OnMapping(VKErrorResponse source, VKWebProblemDetails destination)
    {
        // Custom mapping logic for Web-specific fields
        destination.Type = source.Type.ToString();
        destination.Title = source.Type.ToString();
        destination.Status = source.Type.ToStatusCode();
        destination.ErrorCode = source.Code;
        destination.Timestamp = _timeProvider.GetUtcNow();

        // Conditional Debug Info (OR.02: Security)
        destination.DebugInfo = _env.IsDevelopment() ? MapDebugInfo(source.DebugInfo) : null;

        // Metadata extensions
        foreach (var extension in source.Metadata)
        {
            destination.Extensions[extension.Key] = extension.Value;
        }

        // Detailed errors (e.g. validation)
        destination.Errors = source.Errors?.Select(e => new VKWebErrorDetail
        {
            Code = e.Code,
            Detail = e.Detail
        }).ToList();
    }

    /// <summary>
    /// Recursively maps <see cref="VKErrorDebugInfo"/> to <see cref="VKWebDebugInfo"/>.
    /// </summary>
    private static VKWebDebugInfo? MapDebugInfo(VKErrorDebugInfo? source)
    {
        if (source is null)
            return null;

        return new VKWebDebugInfo
        {
            Message = source.Message,
            Type = source.Type,
            StackTrace = source.StackTrace,
            InnerError = MapDebugInfo(source.InnerError),
            Metadata = source.Metadata
        };
    }
}

