using System;
using System.Collections.Generic;

namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Defines the core contract for a VK.Blocks building block marker.
/// Used for zero-reflection block identification, dependency validation, and diagnostics.
/// </summary>
public interface IVKBlockMarker
{
    /// <summary>
    /// Gets the unique machine-readable identifier (slug) for the building block.
    /// Used for configuration section names, metric meters, and trace sources.
    /// </summary>
    static abstract string Identifier { get; }

    /// <summary>
    /// Gets the version of the building block in SemVer format.
    /// </summary>
    static abstract string Version { get; }

    /// <summary>
    /// Gets the collection of building blocks that this block depends on.
    /// </summary>
    static abstract IReadOnlyList<Type> Dependencies { get; }

    /// <summary>
    /// Gets the name of the ActivitySource used for distributed tracing.
    /// </summary>
    static abstract string ActivitySourceName { get; }

    /// <summary>
    /// Gets the name of the Meter used for metrics instrumentation.
    /// </summary>
    static abstract string MeterName { get; }
}
