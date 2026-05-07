namespace VK.Blocks.Generators.Observability.Internal;

/// <summary>
/// Information for non-marker diagnostics (legacy or lightweight).
/// </summary>
internal sealed record GenericDiagnosticsInfo(
    string Namespace,
    string ClassName,
    string Identifier,
    string? BlockName,
    string? Version,
    string Modifiers,
    string? Description = null)
    : DiagnosticsTargetInfo(Namespace, ClassName, Identifier, BlockName, Version, Modifiers, Description);
