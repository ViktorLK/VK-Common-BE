namespace VK.Blocks.Generators.Observability.Internal;

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
    string ParentIdentifier,
    bool IsOptional = true,
    string? Description = null)
    : DiagnosticsTargetInfo(Namespace, ClassName, Identifier, BlockName, Version, Modifiers, Description);
