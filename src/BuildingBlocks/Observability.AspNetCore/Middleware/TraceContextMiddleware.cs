using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VK.Blocks.Observability.AspNetCore.Options;

namespace VK.Blocks.Observability.AspNetCore.Middleware;

/// <summary>
/// 受信リクエストの W3C TraceContext (traceparent) ヘッダーを読み取り、
/// <see cref="Activity.Current"/> に関連付けるとともに、
/// 設定に応じてレスポンスヘッダーへ TraceId を伝播するミドルウェア。
/// </summary>
public sealed class TraceContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TraceContextOptions _options;

    public TraceContextMiddleware(
        RequestDelegate next,
        IOptions<TraceContextOptions> options)
    {
        _next    = next    ?? throw new ArgumentNullException(nameof(next));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // ── 受信ヘッダーから TraceId を読み取り ──────────────────────────
        if (context.Request.Headers.TryGetValue(
                _options.IncomingTraceIdHeader, out var incoming)
            && !string.IsNullOrEmpty(incoming))
        {
            // ASP.NET Core + OTel SDK が既に traceparent を処理するため、
            // ここでは Activity.Current が存在しない場合のフォールバックとして
            // TraceIdentifier を上書きするにとどめる。
            if (Activity.Current is null)
                context.TraceIdentifier = incoming.ToString();
        }

        // ── ダウンストリーム処理 ──────────────────────────────────────────
        await _next(context);

        // ── レスポンスヘッダーへの伝播 ────────────────────────────────────
        if (!_options.PropagateToResponseHeaders)
            return;

        // ヘッダーが送信済みの場合はスキップ
        if (context.Response.HasStarted)
            return;

        var activity = Activity.Current;

        if (activity is not null)
        {
            TryAppendHeader(context.Response,
                _options.OutgoingTraceIdHeader,
                activity.TraceId.ToString());
        }

        TryAppendHeader(context.Response,
            _options.OutgoingRequestIdHeader,
            context.TraceIdentifier);
    }

    private static void TryAppendHeader(HttpResponse response, string header, string? value)
    {
        if (!string.IsNullOrEmpty(value)
            && !response.Headers.ContainsKey(header))
        {
            response.Headers.TryAdd(header, value);
        }
    }
}
