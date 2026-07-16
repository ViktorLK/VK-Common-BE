using Microsoft.CodeAnalysis;

namespace VK.Tools.SourceGenerators.Utilities;

/// <summary>
/// Common diagnostic descriptors and helpers for VK.Blocks source generators.
/// </summary>
internal static class VKDiagnostics
{
    /// <summary>
    /// VK001: Raised when a type targeted by source generators is not declared as partial.
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMustBePartialRule = new(
        id: "VK001",
        title: "Type must be partial",
        messageFormat: "The {0} '{1}' must be declared as 'partial' to allow source generation",
        category: "VKBlocks",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Creates a diagnostic for a non-partial type targeted by generators.
    /// </summary>
    public static Diagnostic CreateTypeMustBePartial(string typeKind, string typeName, Location? location = null)
    {
        return Diagnostic.Create(
            TypeMustBePartialRule,
            location ?? Location.None,
            typeKind,
            typeName);
    }
}
