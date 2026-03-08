namespace VK.Blocks.Observability.Conventions;

/// <summary>
/// Contains standard field names used for logging and telemetry.
/// </summary>
public static class FieldNames
{
    #region Fields

    /// <summary>Field for deployment environment.</summary>
    public const string DeploymentEnvironment = "deployment.environment";
    /// <summary>Field for trace ID.</summary>
    public const string TraceId = "trace.id";
    /// <summary>Field for span ID.</summary>
    public const string SpanId = "span.id";
    /// <summary>Field for parent span ID.</summary>
    public const string ParentSpanId = "span.parent_id";
    /// <summary>Field for service name.</summary>
    public const string ServiceName = "service.name";
    /// <summary>Field for service version.</summary>
    public const string ServiceVersion = "service.version";
    /// <summary>Field for environment (deprecated, use DeploymentEnvironment).</summary>
    public const string Environment = "deployment.environment";
    /// <summary>Field for HTTP method.</summary>
    public const string HttpMethod = "http.request.method";
    /// <summary>Field for HTTP status code.</summary>
    public const string HttpStatusCode = "http.response.status_code";
    /// <summary>Field for full HTTP URL.</summary>
    public const string HttpUrl = "url.full";
    /// <summary>Field for user ID.</summary>
    public const string UserId = "vk.user.id";
    /// <summary>Field for user name.</summary>
    public const string UserName = "vk.user.name";
    /// <summary>Field for tenant ID.</summary>
    public const string TenantId = "vk.tenant.id";
    /// <summary>Field for correlation ID.</summary>
    public const string CorrelationId = "vk.correlation.id";

    /// <summary>Field for result code.</summary>
    public const string ResultCode = "result.code";
    /// <summary>Field for result message.</summary>
    public const string ResultMessage = "result.message";

    /// <summary>Field for success result.</summary>
    public const string ResultSuccess = "result.success";
    /// <summary>Field for failure result.</summary>
    public const string ResultFailure = "result.failure";
    /// <summary>Field for error type.</summary>
    public const string ErrorType = "error.type";

    #endregion
}
