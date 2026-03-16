using OpenTelemetry.Resources;
using VK.Blocks.Observability.OpenTelemetry.Options;
using VK.Blocks.Observability.OpenTelemetry.Providers;

namespace VK.Blocks.Observability.OpenTelemetry.Resources;

/// <summary>
/// VK.Blocks 向けの OpenTelemetry <see cref="ResourceBuilder"/> ファクトリ。
/// <para>
/// <see cref="VkObservabilityOptions"/> の設定と環境変数に基づいて、
/// 標準リソース属性を自動構築する。
/// </para>
/// <para>
/// <strong>Azure 検出:</strong> <c>WEBSITE_SITE_NAME</c> 環境変数の存在を確認する。<br/>
/// <strong>Kubernetes 検出:</strong> <c>K8S_CLUSTER_NAME</c> 環境変数の存在を確認する。
/// </para>
/// </summary>
public static class VkResourceBuilder
{
    #region Environment Variable Keys

    // Azure App Service / Azure Container Apps
    private const string EnvAzureSiteName = "WEBSITE_SITE_NAME";
    private const string EnvAzureRegion = "REGION_NAME";
    private const string EnvAzureResourceGroup = "WEBSITE_RESOURCE_GROUP";

    // Kubernetes
    private const string EnvK8sCluster = "K8S_CLUSTER_NAME";
    private const string EnvK8sNamespace = "K8S_NAMESPACE";
    private const string EnvK8sPodName = "K8S_POD_NAME";
    private const string EnvHostname = "HOSTNAME";

    #endregion

    #region Public Factory

    /// <summary>
    /// <see cref="VkObservabilityOptions"/> から <see cref="ResourceBuilder"/> を構築する。
    /// </summary>
    /// <param name="options">Fluent API から受け取った設定。</param>
    /// <param name="envProvider">環境変数プロバイダ。</param>
    /// <returns>属性が付与された <see cref="ResourceBuilder"/>。</returns>
    public static ResourceBuilder Build(VkObservabilityOptions options, IEnvironmentProvider envProvider)
    {
        var instanceId = options.ServiceInstanceId
            ?? envProvider.GetVariable("SERVICE_INSTANCE_ID")
            ?? Guid.NewGuid().ToString("N");

        var builder = ResourceBuilder
            .CreateDefault()
            .AddService(
                serviceName: options.ServiceName,
                serviceVersion: options.ServiceVersion,
                serviceInstanceId: instanceId)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = options.Environment
            });

        // Cloud resource detection
        AppendCloudAttributes(builder, options.CloudDetection, envProvider);

        return builder;
    }

    #endregion

    #region Private Helpers

    private static void AppendCloudAttributes(ResourceBuilder builder, CloudDetectionMode mode, IEnvironmentProvider envProvider)
    {
        var effectiveMode = mode == CloudDetectionMode.Auto
            ? DetectCloudEnvironment(envProvider)
            : mode;

        switch (effectiveMode)
        {
            case CloudDetectionMode.Azure:
                AppendAzureAttributes(builder, envProvider);
                break;

            case CloudDetectionMode.Container:
                AppendKubernetesAttributes(builder, envProvider);
                break;

            case CloudDetectionMode.Disabled:
            default:
                break;
        }
    }

    /// <summary>
    /// 環境変数を検査してクラウドプラットフォームを自動判定する。
    /// </summary>
    private static CloudDetectionMode DetectCloudEnvironment(IEnvironmentProvider envProvider)
    {
        if (!string.IsNullOrEmpty(envProvider.GetVariable(EnvAzureSiteName)))
        {
            return CloudDetectionMode.Azure;
        }

        if (!string.IsNullOrEmpty(envProvider.GetVariable(EnvK8sCluster)))
        {
            return CloudDetectionMode.Container;
        }

        return CloudDetectionMode.Disabled;
    }

    /// <summary>
    /// Azure App Service / Azure Container Apps のリソース属性を付与する。
    /// </summary>
    private static void AppendAzureAttributes(ResourceBuilder builder, IEnvironmentProvider envProvider)
    {
        var attrs = new Dictionary<string, object>
        {
            ["cloud.provider"] = "azure"
        };

        TryAdd(attrs, "azure.app_service.site_name", EnvAzureSiteName, envProvider);
        TryAdd(attrs, "cloud.region", EnvAzureRegion, envProvider);
        TryAdd(attrs, "azure.resource_group", EnvAzureResourceGroup, envProvider);

        builder.AddAttributes(attrs);
    }

    /// <summary>
    /// Kubernetes コンテナのリソース属性を付与する。
    /// </summary>
    private static void AppendKubernetesAttributes(ResourceBuilder builder, IEnvironmentProvider envProvider)
    {
        var attrs = new Dictionary<string, object>
        {
            ["cloud.provider"] = "kubernetes"
        };

        TryAdd(attrs, "k8s.cluster.name", EnvK8sCluster, envProvider);
        TryAdd(attrs, "k8s.namespace.name", EnvK8sNamespace, envProvider);

        // Pod 名: 専用環境変数 → HOSTNAME の順でフォールバック
        var podName = envProvider.GetVariable(EnvK8sPodName)
            ?? envProvider.GetVariable(EnvHostname);
        if (!string.IsNullOrEmpty(podName))
        {
            attrs["k8s.pod.name"] = podName;
        }

        builder.AddAttributes(attrs);
    }

    /// <summary>環境変数の値が存在する場合のみ属性辞書に追加するヘルパー。</summary>
    private static void TryAdd(Dictionary<string, object> attrs, string key, string envVar, IEnvironmentProvider envProvider)
    {
        var value = envProvider.GetVariable(envVar);
        if (!string.IsNullOrEmpty(value))
        {
            attrs[key] = value;
        }
    }

    #endregion
}
