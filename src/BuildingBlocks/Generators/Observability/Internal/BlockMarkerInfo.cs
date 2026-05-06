namespace VK.Blocks.Generators.Observability.Internal;

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
    string[]? DependencyTypes = null,
    string? Description = null)
    : DiagnosticsTargetInfo(Namespace, ClassName, Identifier, BlockName, Version, Modifiers, Description);
