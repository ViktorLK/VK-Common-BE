namespace VK.Blocks.Observability.AspNetCore.Logging;

/// <summary>
/// HTTP リクエスト/レスポンスの構造化ログエントリ。
/// ILogger の structured logging に渡すためのモデル。
/// </summary>
public sealed record HttpLogEntry
{
    /// <summary>HTTP メソッド (GET, POST, etc.)。</summary>
    public required string Method { get; init; }

    /// <summary>リクエストパス (例: /api/users)。</summary>
    public required string Path { get; init; }

    /// <summary>クエリ文字列 (例: ?page=1&amp;size=10)。</summary>
    public string? QueryString { get; init; }

    /// <summary>HTTP レスポンスステータスコード。</summary>
    public int StatusCode { get; init; }

    /// <summary>リクエスト処理にかかった時間 (ミリ秒)。</summary>
    public long DurationMs { get; init; }

    /// <summary>クライアント IP アドレス。</summary>
    public string? ClientIp { get; init; }

    /// <summary>User-Agent ヘッダー値。</summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// リクエストボディ文字列。
    /// <see cref="Options.RequestLoggingOptions.LogRequestBody"/> が有効な場合のみ設定される。
    /// 機密フィールドは脱敏済み。
    /// </summary>
    public string? RequestBody { get; init; }

    /// <summary>
    /// レスポンスボディ文字列。
    /// <see cref="Options.RequestLoggingOptions.LogResponseBody"/> が有効な場合のみ設定される。
    /// </summary>
    public string? ResponseBody { get; init; }

    /// <summary>W3C TraceContext の TraceId。</summary>
    public string? TraceId { get; init; }

    /// <summary>W3C TraceContext の SpanId。</summary>
    public string? SpanId { get; init; }

    /// <summary>ログ記録のタイムスタンプ (UTC)。</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// ステータスコードが 4xx または 5xx かどうかを示す。
    /// </summary>
    public bool IsError => StatusCode >= 400;
}
