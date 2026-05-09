using System;
namespace VK.Blocks.Observability;

/// <summary>
/// Defines an interface for log context enrichers.
/// </summary>
public interface IVKLogContextEnricher
{
    IDisposable Enrich();
}
