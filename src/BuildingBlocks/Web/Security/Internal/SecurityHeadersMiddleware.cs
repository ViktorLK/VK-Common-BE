using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Web.Security.Internal;

/// <summary>
/// Middleware for adding standard security headers to outgoing responses.
/// </summary>
internal sealed class SecurityHeadersMiddleware(
    RequestDelegate next,
    IOptions<VKSecurityHeadersOptions> options)
{
    private readonly RequestDelegate _next = VKGuard.NotNull(next);
    private readonly VKSecurityHeadersOptions _options = VKGuard.NotNull(options).Value;

    public async Task InvokeAsync(HttpContext context)
    {
        if (_options.Enabled)
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;

                if (!string.IsNullOrWhiteSpace(_options.XFrameOptions))
                {
                    headers.Append("X-Frame-Options", _options.XFrameOptions);
                }

                if (!string.IsNullOrWhiteSpace(_options.XContentTypeOptions))
                {
                    headers.Append("X-Content-Type-Options", _options.XContentTypeOptions);
                }

                if (!string.IsNullOrWhiteSpace(_options.XXssProtection))
                {
                    headers.Append("X-XSS-Protection", _options.XXssProtection);
                }

                if (!string.IsNullOrWhiteSpace(_options.ReferrerPolicy))
                {
                    headers.Append("Referrer-Policy", _options.ReferrerPolicy);
                }

                if (!string.IsNullOrWhiteSpace(_options.ContentSecurityPolicy))
                {
                    headers.Append("Content-Security-Policy", _options.ContentSecurityPolicy);
                }

                if (!string.IsNullOrWhiteSpace(_options.StrictTransportSecurity) && context.Request.IsHttps)
                {
                    headers.Append("Strict-Transport-Security", _options.StrictTransportSecurity);
                }

                return Task.CompletedTask;
            });
        }

        await _next(context).ConfigureAwait(false);
    }
}
