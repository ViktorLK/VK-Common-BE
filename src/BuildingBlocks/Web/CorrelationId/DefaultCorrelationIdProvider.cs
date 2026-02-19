
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace VK.Blocks.Web.CorrelationId;
internal class DefaultCorrelationIdProvider : ICorrelationIdProvider
{
    public string GetCorrelationId(HttpContext context, CorrelationIdOptions options)
    {
        if (context.Request.Headers.TryGetValue(options.Header, out var correlationId) && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        if (options.UseTraceIdIfAvailable && Activity.Current != null)
        {
            return Activity.Current.TraceId.ToHexString();
        }

        return Guid.NewGuid().ToString();
    }
}
