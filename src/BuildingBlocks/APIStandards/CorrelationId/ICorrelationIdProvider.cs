
using Microsoft.AspNetCore.Http;

namespace VK.Blocks.APIStandards.CorrelationId;
public interface ICorrelationIdProvider
{
    string GetCorrelationId(HttpContext context, CorrelationIdOptions options);
}
