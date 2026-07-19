using Microsoft.CodeAnalysis;

namespace VK.Tools.SourceGenerators.Observability.Internal;

/// <summary>
/// Information specific to a root Building Block marker.
/// </summary>
internal sealed record BlockMarkerInfo(
    string Namespace,
    string ClassName,
    string Identifier,
    string? BlockName,
    string? Version,
    string Modifiers,
    bool IsPartial,
    Location Location,
    string[]? DependencyTypes = null,
    string? Description = null)
    : DiagnosticsTargetInfo(Namespace, ClassName, Identifier, BlockName, Version, Modifiers, IsPartial, Location, Description);
