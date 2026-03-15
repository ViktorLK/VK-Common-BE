namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// Defines an interface for enrichers that add custom properties to log events.
/// Implements the Strategy pattern to extend log contexts with various metadata.
/// </summary>
public interface ILogEnricher
{
    #region Public Methods

    /// <summary>
    /// Enriches the log event with custom properties.
    /// </summary>
    /// <param name="propertyAdder">
    /// A delegate to add properties. The first argument is the field name (e.g., <c>"service.name"</c>),
    /// and the second is its value.
    /// </param>
    void Enrich(Action<string, object?> propertyAdder);

    #endregion
}
