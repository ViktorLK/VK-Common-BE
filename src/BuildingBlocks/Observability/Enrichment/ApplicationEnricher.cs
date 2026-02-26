using Microsoft.Extensions.Options;
using VK.Blocks.Observability.Conventions;
using VK.Blocks.Observability.Options;

namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// アプリケーション識別情報 (サービス名・バージョン・環境) をログコンテキストに追加するエンリッチャー。
/// </summary>
public class ApplicationEnricher(IOptions<ObservabilityOptions> options) : ILogEnricher
{
    private readonly ObservabilityOptions _options = options.Value;

    /// <inheritdoc />
    public void Enrich(Action<string, object?> propertyAdder)
    {
        propertyAdder(FieldNames.ServiceName, _options.ApplicationName);
        propertyAdder(FieldNames.ServiceVersion, _options.ServiceVersion);
        propertyAdder(FieldNames.DeploymentEnvironment, _options.Environment);
    }
}
