using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Internal;

/// <summary>
/// Source-generated high-performance loggers for the OIDC feature.
/// </summary>
internal static partial class OidcLog
{
    #region Public Methods

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "OIDC Provider Registered: [{AuthProvider}] for Authority: [{Authority}]. TraceId: {TraceId}")]
    public static partial void LogOidcProviderRegistered(this ILogger logger, string authProvider, string authority, string traceId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "OIDC Authentication Success: Provider: [{AuthProvider}], Subject: [{Subject}]. TraceId: {TraceId}")]
    public static partial void LogOidcAuthenticationSuccess(this ILogger logger, string authProvider, string subject, string traceId);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "OIDC Authentication Failed: Provider: [{AuthProvider}], Error: [{Error}]. TraceId: {TraceId}")]
    public static partial void LogOidcAuthenticationFailed(this ILogger logger, string authProvider, string error, string traceId);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Error,
        Message = "OIDC Authentication Error: Provider: [{AuthProvider}]. TraceId: {TraceId}")]
    public static partial void LogOidcAuthenticationError(this ILogger logger, Exception exception, string authProvider, string traceId);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Error,
        Message = "OIDC Configuration Mapping Failed: Provider: [{AuthProvider}]. TraceId: {TraceId}")]
    public static partial void LogOidcMappingError(this ILogger logger, string authProvider, string traceId);

    #endregion
}
