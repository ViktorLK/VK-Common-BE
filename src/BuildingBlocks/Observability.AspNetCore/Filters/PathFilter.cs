using VK.Blocks.Observability.AspNetCore.Options;

namespace VK.Blocks.Observability.AspNetCore.Filters;

/// <summary>
/// リクエストパスに基づいて Observability 処理 (ロギング・メトリクス) の
/// 対象/除外を判定するフィルター。
/// </summary>
public sealed class PathFilter
{
    private readonly IReadOnlyList<string> _excludedPaths;

    /// <summary>
    /// <see cref="RequestLoggingOptions.ExcludedPaths"/> を使用してフィルターを初期化する。
    /// </summary>
    public PathFilter(RequestLoggingOptions options)
        : this(options.ExcludedPaths) { }

    /// <summary>
    /// 除外パスのリストを明示的に指定してフィルターを初期化する。
    /// </summary>
    public PathFilter(IReadOnlyList<string> excludedPaths)
    {
        _excludedPaths = excludedPaths
            ?? throw new ArgumentNullException(nameof(excludedPaths));
    }

    /// <summary>
    /// 指定されたパスがロギング対象かどうかを返す。
    /// </summary>
    /// <param name="path">判定するリクエストパス (例: "/api/users")。</param>
    /// <returns>
    /// ロギングを実行すべき場合は <see langword="true"/>、
    /// 除外パスに一致した場合は <see langword="false"/>。
    /// </returns>
    public bool ShouldLog(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return true;

        foreach (var excluded in _excludedPaths)
        {
            if (path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }
}
