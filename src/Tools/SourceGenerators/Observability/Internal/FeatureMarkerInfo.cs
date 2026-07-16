using Microsoft.CodeAnalysis;

namespace VK.Tools.SourceGenerators.Observability.Internal;

/// <summary>
/// Information specific to a sub-feature marker.
/// </summary>
internal sealed record FeatureMarkerInfo(
    string Namespace,
    string ClassName,
    string Identifier,
    string? BlockName,
    string? Version,
    string Modifiers,
    bool IsPartial,
    Location Location,
    string ParentIdentifier,
    bool IsOptional = true,
    string? Description = null)
    : DiagnosticsTargetInfo(Namespace, ClassName, Identifier, BlockName, Version, Modifiers, IsPartial, Location, Description);
