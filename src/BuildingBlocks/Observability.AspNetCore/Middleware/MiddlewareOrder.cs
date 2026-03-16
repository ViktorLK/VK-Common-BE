namespace VK.Blocks.Observability.AspNetCore.Middleware;

/// <summary>
/// ASP.NET Core ミドルウェアの推奨登録順序定数。
/// <c>app.UseMiddleware</c> の呼び出し順序が適切になるよう、
/// DI 拡張メソッド内でこれらの定数を参照する。
/// </summary>
public static class MiddlewareOrder
{
    /// <summary>
    /// <see cref="TraceContextMiddleware"/> の登録順序。
    /// 最初のミドルウェアとして TraceId をリクエストに付与する。
    /// </summary>
    public const int TraceContext = -2000;

    /// <summary>
    /// <see cref="RequestLoggingMiddleware"/> の登録順序。
    /// TraceContext の後、ルーティング前に配置してすべての HTTP ログを記録する。
    /// </summary>
    public const int RequestLogging = -1000;
}
