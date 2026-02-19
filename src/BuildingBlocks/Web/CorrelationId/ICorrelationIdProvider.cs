
using Microsoft.AspNetCore.Http;

namespace VK.Blocks.Web.CorrelationId;
public interface ICorrelationIdProvider
{
    string GetCorrelationId(HttpContext context, CorrelationIdOptions options);
}
