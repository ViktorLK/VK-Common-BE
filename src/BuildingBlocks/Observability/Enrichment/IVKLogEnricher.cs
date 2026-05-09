using System;
namespace VK.Blocks.Observability;

/// <summary>
/// Defines an interface for enrichers that add custom properties to log events.
/// </summary>
public interface IVKLogEnricher
{
    void Enrich(Action<string, object?> propertyAdder);
}
