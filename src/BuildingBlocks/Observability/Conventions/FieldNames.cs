namespace VK.Blocks.Observability.Conventions;

public static class FieldNames
{

    public const string DeploymentEnvironment = "deployment.environment";
    public const string TraceId = "trace.id";
    public const string SpanId = "span.id";
    public const string ParentSpanId = "span.parent_id";
    public const string ServiceName = "service.name";
    public const string ServiceVersion = "service.version";
    public const string Environment = "deployment.environment";
    public const string HttpMethod = "http.request.method";
    public const string HttpStatusCode = "http.response.status_code";
    public const string HttpUrl = "url.full";
    public const string UserId = "vk.user.id";
    public const string UserName = "vk.user.name";
    public const string TenantId = "vk.tenant.id";
    public const string CorrelationId = "vk.correlation.id";

    public const string ResultCode = "result.code";
    public const string ResultMessage = "result.message";

    public const string ResultSuccess = "result.success";
    public const string ResultFailure = "result.failure";
    public const string ErrorType = "error.type";
}
