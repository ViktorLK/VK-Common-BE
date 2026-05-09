using Microsoft.Extensions.Logging;

namespace VK.Blocks.Web.Tenancy.Internal;

/// <summary>
/// Logger messages for Tenancy operations.
/// Complies with OR.01 (Observability) for structured, allocation-free logging.
/// </summary>
internal static partial class TenancyLog
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Resolved TenantId '{TenantId}' from header '{HeaderName}'")]
    public static partial void LogTenantResolvedFromHeader(
        this ILogger logger,
        string tenantId,
        string headerName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Resolved TenantId '{TenantId}' from query string '{ParameterName}'")]
    public static partial void LogTenantResolvedFromQuery(
        this ILogger logger,
        string tenantId,
        string parameterName);
}

