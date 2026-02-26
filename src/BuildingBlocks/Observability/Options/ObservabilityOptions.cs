using System.ComponentModel.DataAnnotations;

namespace VK.Blocks.Observability.Options;

/// <summary>
/// Observability ブロックの設定オプション。
/// アプリケーション識別情報、トレーシング・メトリクスの有効化、PII制御を管理する。
/// </summary>
public class ObservabilityOptions
{
    /// <summary>
    /// アプリケーション名。ログおよびトレースの <c>service.name</c> 属性として使用される。
    /// </summary>
    [Required, MinLength(1)]
    public string ApplicationName { get; set; } = "Unknown";

    /// <summary>
    /// サービスバージョン。ログおよびトレースの <c>service.version</c> 属性として使用される。
    /// </summary>
    [Required, MinLength(1)]
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// デプロイ環境名 (例: Production, Staging, Development)。
    /// </summary>
    public string Environment { get; set; } = "Production";

    /// <summary>
    /// 分散トレーシングを有効にするかどうか。
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// メトリクス計測を有効にするかどうか。
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// ユーザー名 (<c>enduser.name</c>) をログに含めるかどうか。
    /// PII (個人識別情報) 保護の観点から、デフォルトでは無効。
    /// 有効にする場合は GDPR 等の規制要件を確認すること。
    /// </summary>
    public bool IncludeUserName { get; set; }
}
