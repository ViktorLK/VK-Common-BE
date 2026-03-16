namespace VK.Blocks.Observability.AspNetCore.Options;

/// <summary>
/// トレースコンテキストを HTTP ヘッダーへ伝播する際の設定オプション。
/// </summary>
public sealed class TraceContextOptions
{
    /// <summary>
    /// 受信リクエストから読み取るトレース ID ヘッダー名。
    /// W3C TraceContext 形式 (traceparent) または独自形式を指定可能。
    /// </summary>
    public string IncomingTraceIdHeader { get; set; } = "traceparent";

    /// <summary>
    /// レスポンスに追加するトレース ID ヘッダー名。
    /// クライアント側でトレースを追跡しやすくするためのカスタム x-trace-id。
    /// </summary>
    public string OutgoingTraceIdHeader { get; set; } = "x-trace-id";

    /// <summary>
    /// レスポンスに追加するリクエスト ID ヘッダー名。
    /// </summary>
    public string OutgoingRequestIdHeader { get; set; } = "x-request-id";

    /// <summary>
    /// レスポンスヘッダーへトレース ID を付与するかどうか。
    /// デフォルト: <see langword="true"/>。
    /// </summary>
    public bool PropagateToResponseHeaders { get; set; } = true;
}
