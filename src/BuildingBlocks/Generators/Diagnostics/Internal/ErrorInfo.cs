namespace VK.Blocks.Generators.Diagnostics.Internal;

/// <summary>
/// Information about a discovered error definition.
/// </summary>
/// <param name="SymbolName">The name of the static field.</param>
/// <param name="FullTypeName">The full type name which contains the field.</param>
/// <param name="Code">The unique error code.</param>
/// <param name="Description">The description of the error.</param>
/// <param name="ErrorType">The type of the error (e.g. Failure, Validation).</param>
internal sealed record ErrorInfo(
    string SymbolName,
    string FullTypeName,
    string Code,
    string Description,
    string ErrorType);
