using Microsoft.Extensions.Options;
using VK.Blocks.Observability.Conventions;
using VK.Blocks.Observability.Options;

namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// アプリケーション識別情報 (サービス名・バージョン・環境) をログコンテキストに追加するエンリッチャー。
/// </summary>
public class ApplicationEnricher : ILogEnricher
{
    #region Fields

    private readonly ObservabilityOptions _options;

    #endregion

    #region Constructors

    public ApplicationEnricher(IOptions<ObservabilityOptions> options)
    {
        _options = options.Value;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public void Enrich(Action<string, object?> propertyAdder)
    {
        propertyAdder(FieldNames.ServiceName, _options.ApplicationName);
        propertyAdder(FieldNames.ServiceVersion, _options.ServiceVersion);
        propertyAdder(FieldNames.DeploymentEnvironment, _options.Environment);
    }

    #endregion
}
