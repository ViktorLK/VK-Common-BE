using Microsoft.Extensions.Options;
using VK.Blocks.Observability.Conventions;
using VK.Blocks.Observability.Options;

namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// An enricher that adds application metadata (service name, version, environment) to the log context.
/// </summary>
public sealed class ApplicationEnricher(IOptions<ObservabilityOptions> options) : ILogEnricher
{
    #region Fields

    private readonly ObservabilityOptions _options = options.Value;

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
