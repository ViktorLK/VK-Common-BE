namespace VK.Blocks.Generators.Observability.Internal;

/// <summary>
/// Base information about a diagnostic target discovered by the generator.
/// </summary>
internal abstract record DiagnosticsTargetInfo(
    string Namespace,
    string ClassName,
    string Identifier,
    string? BlockName,
    string? Version,
    string Modifiers,
    string? Description = null);
