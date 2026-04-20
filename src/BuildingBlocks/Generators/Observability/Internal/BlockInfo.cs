namespace VK.Blocks.Generators.Observability.Internal;

/// <summary>
/// Information about a block discovered for diagnostics generation.
/// </summary>
/// <param name="Namespace">The namespace of the class.</param>
/// <param name="ClassName">The name of the class.</param>
/// <param name="Identifier">The machine-readable identifier of the block.</param>
/// <param name="Version">The version of the block.</param>
/// <param name="Modifiers">The access modifiers for the generated class.</param>
/// <param name="Description">The description of the block or application.</param>
internal sealed record BlockInfo(string Namespace, string ClassName, string Identifier, string Version, string Modifiers, string? Description = null);
