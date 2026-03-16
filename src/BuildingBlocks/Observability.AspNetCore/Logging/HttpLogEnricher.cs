using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Observability.AspNetCore.Options;

namespace VK.Blocks.Observability.AspNetCore.Logging;

/// <summary>
/// <see cref="HttpContext"/> の情報 (TraceId, SpanId, HTTP メソッド, パス等) を
/// <see cref="ILogger"/> のスコープに注入するエンリッチャー。
/// <para>
/// <c>using</c> ブロック内で使用することで、スコープのライフタイムをリクエストに紐付ける。
/// </para>
/// </summary>
public sealed class HttpLogEnricher
{
    private readonly ILogger<HttpLogEnricher> _logger;
    private readonly RequestLoggingOptions _options;

    public HttpLogEnricher(
        ILogger<HttpLogEnricher> logger,
        IOptions<RequestLoggingOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// 指定した <see cref="HttpContext"/> の診断情報をログスコープとして開始する。
    /// </summary>
    /// <param name="context">現在の <see cref="HttpContext"/>。</param>
    /// <returns>
    /// スコープを表す <see cref="IDisposable"/>。
    /// <c>using</c> ブロックや <c>await using</c> で囲んで使用すること。
    /// </returns>
    public IDisposable? BeginScope(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var activity = System.Diagnostics.Activity.Current;

        var scopeProperties = new Dictionary<string, object?>
        {
            [HttpLogPropertyNames.Method]     = context.Request.Method,
            [HttpLogPropertyNames.Path]       = context.Request.Path.Value,
            [HttpLogPropertyNames.Scheme]     = context.Request.Scheme,
            [HttpLogPropertyNames.Host]       = context.Request.Host.Value,
            [HttpLogPropertyNames.TraceId]    = activity?.TraceId.ToString(),
            [HttpLogPropertyNames.SpanId]     = activity?.SpanId.ToString(),
            [HttpLogPropertyNames.RequestId]  = context.TraceIdentifier
        };

        if (_options.LogClientIp && context.Connection.RemoteIpAddress != null)
        {
            scopeProperties[HttpLogPropertyNames.ClientIp] = context.Connection.RemoteIpAddress.ToString();
        }

        return _logger.BeginScope(scopeProperties);
    }
}
