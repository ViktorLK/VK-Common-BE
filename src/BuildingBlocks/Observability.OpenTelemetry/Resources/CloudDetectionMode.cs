namespace VK.Blocks.Observability.OpenTelemetry.Resources;

/// <summary>
/// クラウド環境のリソース属性を自動検出するモードを定義する列挙型。
/// <para>
/// 検出した属性は OpenTelemetry <c>ResourceBuilder</c> に付与され、
/// バックエンド（Jaeger、Grafana 等）でのリソース識別に使用される。
/// </para>
/// </summary>
public enum CloudDetectionMode
{
    /// <summary>クラウドリソース属性の自動検出を無効化する。</summary>
    Disabled = 0,

    /// <summary>
    /// 環境変数を検査して Azure または Kubernetes を自動判定する。
    /// <c>WEBSITE_SITE_NAME</c> が存在する場合は Azure、
    /// <c>K8S_CLUSTER_NAME</c> が存在する場合は Container として処理する。
    /// </summary>
    Auto = 1,

    /// <summary>
    /// Azure App Service / Azure Container Apps リソース属性を検出する。
    /// 環境変数: <c>WEBSITE_SITE_NAME</c>, <c>REGION_NAME</c>, <c>WEBSITE_RESOURCE_GROUP</c>。
    /// </summary>
    Azure = 2,

    /// <summary>
    /// Kubernetes コンテナリソース属性を検出する。
    /// 環境変数: <c>K8S_CLUSTER_NAME</c>, <c>K8S_NAMESPACE</c>,
    /// <c>K8S_POD_NAME</c> (フォールバック: <c>HOSTNAME</c>)。
    /// </summary>
    Container = 3
}
