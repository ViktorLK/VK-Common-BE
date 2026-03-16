using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Observability.AspNetCore.Filters;
using VK.Blocks.Observability.AspNetCore.Logging;
using VK.Blocks.Observability.AspNetCore.Metrics;
using VK.Blocks.Observability.AspNetCore.Options;

namespace VK.Blocks.Observability.AspNetCore.Middleware;

/// <summary>
/// HTTP リクエストの開始/終了をインターセプトし、
/// 構造化ログおよびメトリクスを記録するコアミドルウェア。
/// <para>
/// 処理フロー:
/// <list type="number">
///   <item>除外パスチェック → スキップ判定</item>
///   <item>リクエストボディのバッファリング (オプション)</item>
///   <item>ダウンストリーム処理</item>
///   <item>ログエントリ生成 → 構造化ログ出力</item>
///   <item>メトリクス記録</item>
/// </list>
/// </para>
/// </summary>
public sealed partial class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;
    private readonly PathFilter _pathFilter;
    private readonly SensitiveDataRedactor _redactor;
    private readonly HttpLogEnricher _enricher;
    private readonly IHttpMetricsCollector _metrics;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        IOptions<RequestLoggingOptions> options,
        PathFilter pathFilter,
        SensitiveDataRedactor redactor,
        HttpLogEnricher enricher,
        IHttpMetricsCollector metrics)
    {
        _next     = next     ?? throw new ArgumentNullException(nameof(next));
        _logger   = logger   ?? throw new ArgumentNullException(nameof(logger));
        _options  = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _pathFilter = pathFilter ?? throw new ArgumentNullException(nameof(pathFilter));
        _redactor = redactor ?? throw new ArgumentNullException(nameof(redactor));
        _enricher = enricher ?? throw new ArgumentNullException(nameof(enricher));
        _metrics  = metrics  ?? throw new ArgumentNullException(nameof(metrics));
    }

    [LoggerMessage(EventId = 1, Message = "HTTP {Method} {Path} responded {StatusCode} in {DurationMs}ms")]
    private partial void LogRequestMessage(LogLevel logLevel, string method, string path, int statusCode, long durationMs, Exception? exception = null);

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        // パスフィルターでスキップ判定
        if (!_pathFilter.ShouldLog(path))
        {
            await _next(context);
            return;
        }

        // リクエストボディを読み取るために EnableBuffering を適用
        if (_options.LogRequestBody)
            context.Request.EnableBuffering();

        // レスポンスボディをキャプチャするためにストリームを差し替え
        Stream originalResponseBody = context.Response.Body;
        using var responseBodyBuffer = _options.LogResponseBody
            ? new MemoryStream()
            : null;

        if (responseBodyBuffer is not null)
            context.Response.Body = responseBodyBuffer;

        var sw = Stopwatch.StartNew();
        Exception? exception = null;

        try
        {
            using var scope = _enricher.BeginScope(context);
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            sw.Stop();

            string? responseBody = null;
            if (responseBodyBuffer is not null)
            {
                responseBodyBuffer.Seek(0, SeekOrigin.Begin);
                
                // Read response body before restoring the original stream
                responseBody = await ReadBodyAsync(responseBodyBuffer, responseBodyBuffer.Length, resetPosition: false);
                
                responseBodyBuffer.Seek(0, SeekOrigin.Begin);
                await responseBodyBuffer.CopyToAsync(originalResponseBody);
                context.Response.Body = originalResponseBody;
            }

            var entry = await BuildLogEntryAsync(context, sw.ElapsedMilliseconds, responseBody);

            LogEntry(entry);
            _metrics.Record(entry, exception);
        }
    }

    // -------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------

    private async Task<HttpLogEntry> BuildLogEntryAsync(
        HttpContext context, long durationMs, string? capturedResponseBody)
    {
        string? requestBody  = null;
        string? responseBody = null;

        if (_options.LogRequestBody)
        {
            requestBody = await ReadBodyAsync(
                context.Request.Body,
                context.Request.ContentLength,
                resetPosition: true);
            requestBody = _redactor.Redact(requestBody);
        }

        if (_options.LogResponseBody)
        {
            responseBody = _redactor.Redact(capturedResponseBody);
        }

        var activity = Activity.Current;

        return new HttpLogEntry
        {
            Method       = context.Request.Method,
            Path         = context.Request.Path.Value ?? "/",
            QueryString  = context.Request.QueryString.HasValue
                               ? context.Request.QueryString.Value
                               : null,
            StatusCode   = context.Response.StatusCode,
            DurationMs   = durationMs,
            ClientIp     = _options.LogClientIp ? context.Connection.RemoteIpAddress?.ToString() : null,
            UserAgent    = context.Request.Headers.UserAgent.ToString(),
            RequestBody  = requestBody,
            ResponseBody = responseBody,
            TraceId      = activity?.TraceId.ToString(),
            SpanId       = activity?.SpanId.ToString()
        };
    }

    private async Task<string?> ReadBodyAsync(
        Stream stream, long? contentLength, bool resetPosition)
    {
        if (contentLength is 0)
            return null;

        var maxBytes = _options.MaxBodySizeBytes;
        var buffer   = new byte[Math.Min(maxBytes, contentLength ?? maxBytes)];
        var read     = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));

        if (resetPosition && stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);

        return read > 0 ? System.Text.Encoding.UTF8.GetString(buffer, 0, read) : null;
    }

    private void LogEntry(HttpLogEntry entry)
    {
        var level = entry.IsError ? _options.ErrorLogLevel : _options.LogLevel;
        
        if (_logger.IsEnabled(level))
        {
            LogRequestMessage(level, entry.Method, entry.Path, entry.StatusCode, entry.DurationMs, null);
        }
    }
}
