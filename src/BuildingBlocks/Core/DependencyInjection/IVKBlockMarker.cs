using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Core;

/// <summary>
/// Defines the instance-level contract for a building block registration marker.
/// </summary>
/// <remarks>
/// <para>
/// <b>Source Generated Marker Infrastructure:</b>
/// This interface does not contain static members, allowing it to be used as a type argument
/// in collections like <see cref="IReadOnlyList{IVKBlockMarker}"/>.
/// </para>
/// <para>
/// The singleton 'Instance' for each marker is automatically injected by the Source Generator
/// alongside the <see cref="IVKBlockMarkerProvider{TSelf}"/> interface.
/// </para>
/// </remarks>
public interface IVKBlockMarker
{
    /// <summary>
    /// Gets the unique identifier for the building block (e.g., "Caching").
    /// </summary>
    string Identifier { get; }

    /// <summary>
    /// Gets the current version of the building block.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the collection of building blocks that this block depends on.
    /// </summary>
    IReadOnlyList<IVKBlockMarker> Dependencies { get; }

    /// <summary>
    /// Gets the name of the ActivitySource used for distributed tracing.
    /// </summary>
    string ActivitySourceName { get; }

    /// <summary>
    /// Gets the name of the Meter used for metrics.
    /// </summary>
    string MeterName { get; }

    /// <summary>
    /// Recursively ensures that all dependencies of this building block are registered.
    /// Following Rule 13 (Check-Prerequisite), this performs a deep-scan of the dependency tree.
    /// </summary>
    /// <param name="services">The service collection to check.</param>
    /// <param name="dependentId">The identifier of the block that requires these dependencies.</param>
    /// <exception cref="VKDependencyException">Thrown if any required dependency is not registered.</exception>
    void EnsureDependenciesRegistered(IServiceCollection services, string dependentId)
    {
        EnsureDependenciesRecursive(this, services, dependentId, []);
    }

    private static void EnsureDependenciesRecursive(IVKBlockMarker marker, IServiceCollection services, string dependentId, HashSet<string> visited)
    {
        // 1. Protection against circular dependencies
        if (!visited.Add(marker.Identifier))
        {
            return;
        }

        foreach (IVKBlockMarker dependency in marker.Dependencies)
        {
            // 2. Check if the direct dependency is registered FIRST (Pre-order) using Identifier
            if (!services.IsVKBlockRegistered(dependency.Identifier))
            {
                throw VKDependencyException.MissingDependency(dependency.Identifier, dependentId);
            }

            // 3. Recurse only if the parent is registered
            EnsureDependenciesRecursive(dependency, services, marker.Identifier, visited);
        }
    }
}
