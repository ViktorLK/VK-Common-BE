namespace VK.Blocks.Observability.AspNetCore.Options;

/// <summary>
/// HTTP リクエストロギングの動作を制御するオプション。
/// </summary>
public sealed class RequestLoggingOptions
{
    /// <summary>
    /// リクエストボディのキャプチャを有効にするかどうか。
    /// <para>有効にするとメモリ使用量が増加するため、本番環境では慎重に使用すること。</para>
    /// </summary>
    public bool LogRequestBody { get; set; } = false;

    /// <summary>
    /// クライアント IP アドレス (<c>RemoteIpAddress</c>) のロギングを有効にするかどうか。
    /// GDPR 等のプライバシー要件を考慮し、デフォルトは <see langword="false"/>。
    /// </summary>
    public bool LogClientIp { get; set; } = false;

    /// <summary>
    /// レスポンスボディのキャプチャを有効にするかどうか。
    /// <para>有効にするとメモリ使用量が増加するため、本番環境では慎重に使用すること。</para>
    /// </summary>
    public bool LogResponseBody { get; set; } = false;

    /// <summary>
    /// キャプチャするボディの最大バイト数。
    /// このサイズを超えた部分は打ち切られる。デフォルト: 32 KB。
    /// </summary>
    public int MaxBodySizeBytes { get; set; } = 32 * 1024;

    /// <summary>
    /// ログ記録をスキップするパス プレフィックスの一覧。
    /// デフォルトでヘルスチェック・Swagger UI を除外する。
    /// </summary>
    public IReadOnlyList<string> ExcludedPaths { get; set; } =
    [
        "/health",
        "/healthz",
        "/ready",
        "/metrics",
        "/swagger",
        "/favicon.ico"
    ];

    /// <summary>
    /// 脱敏 (マスキング) 対象のフィールド名一覧（大文字小文字を区別しない）。
    /// JSON ボディ内でこれらのキーに一致する値を "<redacted>" に置換する。
    /// </summary>
    public IReadOnlyList<string> SensitiveFields { get; set; } =
    [
        "password",
        "passwd",
        "secret",
        "token",
        "accessToken",
        "access_token",
        "refreshToken",
        "refresh_token",
        "authorization",
        "apiKey",
        "api_key",
        "creditCard",
        "credit_card",
        "ssn",
        "cvv"
    ];

    /// <summary>
    /// リクエストログのログレベル。デフォルトは <see cref="Microsoft.Extensions.Logging.LogLevel.Information"/>。
    /// </summary>
    public Microsoft.Extensions.Logging.LogLevel LogLevel { get; set; } =
        Microsoft.Extensions.Logging.LogLevel.Information;

    /// <summary>
    /// エラー (4xx/5xx) 時のログレベル。デフォルトは <see cref="Microsoft.Extensions.Logging.LogLevel.Warning"/>。
    /// </summary>
    public Microsoft.Extensions.Logging.LogLevel ErrorLogLevel { get; set; } =
        Microsoft.Extensions.Logging.LogLevel.Warning;
}
